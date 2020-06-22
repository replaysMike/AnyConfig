using AnyConfig.Collections;
using AnyConfig.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;

namespace AnyConfig
{
    public static partial class ConfigurationManager
    {
        private static readonly SemaphoreSlim _loadLock = new SemaphoreSlim(1, 1);
        private static Lazy<LegacyConfiguration> _legacyConfiguration = new Lazy<LegacyConfiguration>(() => LoadConfiguration());
        private static Lazy<ConnectionStringSettingsCollection> _cachedConnectionStrings = new Lazy<ConnectionStringSettingsCollection>(() => GetConnectionStrings());
        private static Lazy<ReadOnlyDictionary<string, ConnectionStringSetting>> _cachedConnectionStringsDictionary = new Lazy<ReadOnlyDictionary<string, ConnectionStringSetting>>(() => GetConnectionStringsAsDictionary());
        private static Lazy<GenericNameValueCollection> _cachedAppSettings = new Lazy<GenericNameValueCollection>(() => GetAppSettings());
        private static Lazy<ReadOnlyDictionary<string, string>> _cachedAppSettingsDictionary = new Lazy<ReadOnlyDictionary<string, string>>(() => GetAppSettingsAsDictionary());
        private static Lazy<ReadOnlyDictionary<string, AnyConfigAppSettingCollection>> _cachedAnyConfigGroupedAppSettings = new Lazy<ReadOnlyDictionary<string, AnyConfigAppSettingCollection>>(() => GetAnyConfigGroupedAppSettings());

        private static ConnectionStringSettingsCollection GetConnectionStrings()
        {
            return new ConnectionStringSettingsCollection(GetConnectionStringsAsDictionary());
        }

        private static ReadOnlyDictionary<string, ConnectionStringSetting> GetConnectionStringsAsDictionary()
        {
            return new ReadOnlyDictionary<string, ConnectionStringSetting>(_legacyConfiguration.Value.Configuration.ConnectionStrings.ToDictionary(x => x.Name, x => x.ConnectionStringSetting));
        }

        private static GenericNameValueCollection GetAppSettings()
        {
            return new GenericNameValueCollection(_legacyConfiguration.Value.Configuration.AppSettings.Aggregate(new GenericNameValueCollection(), (seed, current) =>
            {
                seed.Add(current.Key, current.Value);
                return seed;
            }));
        }

        private static ReadOnlyDictionary<string, string> GetAppSettingsAsDictionary()
        {
            return new ReadOnlyDictionary<string, string>(_legacyConfiguration.Value.Configuration.AppSettings.ToDictionary(x => x.Key, x => x.Value));
        }

        private static ReadOnlyDictionary<string, AnyConfigAppSettingCollection> GetAnyConfigGroupedAppSettings()
        {
            return new ReadOnlyDictionary<string, AnyConfigAppSettingCollection>(_legacyConfiguration.Value.Configuration.AnyConfigGroups.ToDictionary(x => x.GroupName, x => new AnyConfigAppSettingCollection(x.Settings)));
        }

        private static LegacyConfiguration LoadConfiguration()
        {
            _loadLock.Wait();
            try
            {
                // reset all cached data
                ResetCachedData();

                // attempt to detect source type based on file extension if specified
                var configurationSource = AutoDetectSource(ConfigurationFilename, ConfigurationSource);

                // resolve configuration file
                LegacyConfiguration config;
                var resolver = new ConfigurationResolver();
                switch (configurationSource)
                {
                    case ConfigurationManagerSource.Xml:
                        config = resolver.ResolveLegacyConfigurationFromXml(ConfigurationFilename);
                        break;
                    case ConfigurationManagerSource.Json:
                        config = resolver.ResolveLegacyConfigurationFromJson(ConfigurationFilename);
                        break;
                    case ConfigurationManagerSource.Auto:
                    default:
                        // auto-detect based on available detected file
                        config = resolver.ResolveLegacyConfigurationFromXml(ConfigurationFilename) ?? resolver.ResolveLegacyConfigurationFromJson(ConfigurationFilename);
                        break;
                }
                // update the configuration filename to what was actually loaded
                ConfigurationFilename = config.Filename;
                return config;
            }
            finally
            {
                _loadLock.Release();
            }
        }

        private static ConfigurationManagerSource AutoDetectSource(string filename, ConfigurationManagerSource configurationSource)
        {
            if (configurationSource == ConfigurationManagerSource.Auto && !string.IsNullOrEmpty(filename))
            {
                var fileExtension = Path.GetExtension(filename).ToLower();
                switch (fileExtension)
                {
                    case ".json":
                        configurationSource = ConfigurationManagerSource.Json;
                        break;
                    case ".xml":
                    case ".config":
                    default:
                        configurationSource = ConfigurationManagerSource.Xml;
                        break;
                }
            }
            return configurationSource;
        }

        private static void ResetCachedData()
        {
            if(_cachedConnectionStrings.IsValueCreated)
                _cachedConnectionStrings = new Lazy<ConnectionStringSettingsCollection>(() => GetConnectionStrings());
            if (_cachedConnectionStringsDictionary.IsValueCreated)
                _cachedConnectionStringsDictionary = new Lazy<ReadOnlyDictionary<string, ConnectionStringSetting>>(() => GetConnectionStringsAsDictionary());
            if (_cachedAppSettings.IsValueCreated)
                _cachedAppSettings = new Lazy<GenericNameValueCollection>(() => GetAppSettings());
            if (_cachedAppSettingsDictionary.IsValueCreated)
                _cachedAppSettingsDictionary = new Lazy<ReadOnlyDictionary<string, string>>(() => GetAppSettingsAsDictionary());
            if (_cachedAnyConfigGroupedAppSettings.IsValueCreated)
                _cachedAnyConfigGroupedAppSettings = new Lazy<ReadOnlyDictionary<string, AnyConfigAppSettingCollection>>(() => GetAnyConfigGroupedAppSettings());
        }
    }
}
