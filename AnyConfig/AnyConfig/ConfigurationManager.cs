using AnyConfig.Collections;
using AnyConfig.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace AnyConfig
{
    /// <summary>
    /// Legacy configuration manager
    /// Note this version supports both Xml and Json configuration
    /// </summary>
    public static class ConfigurationManager
    {
        private static readonly SemaphoreSlim _loadLock = new SemaphoreSlim(1, 1);
        private static Lazy<LegacyConfiguration> _legacyConfiguration = new Lazy<LegacyConfiguration>(() => LoadConfiguration());
        private static Lazy<ConnectionStringSettingsCollection> _cachedConnectionStrings = new Lazy<ConnectionStringSettingsCollection>(() => GetConnectionStrings());
        private static Lazy<ReadOnlyDictionary<string, ConnectionStringSetting>> _cachedConnectionStringsDictionary = new Lazy<ReadOnlyDictionary<string, ConnectionStringSetting>>(() => GetConnectionStringsAsDictionary());
        private static Lazy<GenericNameValueCollection> _cachedAppSettings = new Lazy<GenericNameValueCollection>(() => GetAppSettings());
        private static Lazy<ReadOnlyDictionary<string, string>> _cachedAppSettingsDictionary = new Lazy<ReadOnlyDictionary<string, string>>(() => GetAppSettingsAsDictionary());

        /// <summary>
        /// The source configuration to load
        /// </summary>
        public static ConfigurationManagerSource ConfigurationSource { get; set; } = ConfigurationManagerSource.Auto;

        /// <summary>
        /// The configuration filename
        /// </summary>
        public static string ConfigurationFilename { get; set; }

        /// <summary>
        /// Connection strings
        /// </summary>
        public static ConnectionStringSettingsCollection ConnectionStrings => _cachedConnectionStrings.Value;

        /// <summary>
        /// Connection strings
        /// </summary>
        public static ReadOnlyDictionary<string, ConnectionStringSetting> ConnectionStringsAsDictionary => _cachedConnectionStringsDictionary.Value;

        /// <summary>
        /// Application settings
        /// </summary>
        public static GenericNameValueCollection AppSettings => _cachedAppSettings.Value;

        /// <summary>
        /// Application settings
        /// </summary>
        public static ReadOnlyDictionary<string, string> AppSettingsAsDictionary => _cachedAppSettingsDictionary.Value;

        /// <summary>
        /// Get a custom configuration section
        /// </summary>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public static object GetSection(string sectionName)
        {
            return _legacyConfiguration.Value.Configuration.ConfigSections
                .Where(x => x.Name.Equals(sectionName))
                .Select(x => x.Configuration)
                .FirstOrDefault();
        }

        /// <summary>
        /// Force reset of the configuration to default values
        /// </summary>
        public static void ResetDefaults()
        {
            ConfigurationSource = ConfigurationManagerSource.Auto;
            ConfigurationFilename = null;
        }

        /// <summary>
        /// Force reload of the configuration
        /// </summary>
        public static void Reload()
        {
            ResetCachedData();
            _legacyConfiguration = new Lazy<LegacyConfiguration>(() => LoadConfiguration());
        }

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

        private static LegacyConfiguration LoadConfiguration()
        {
            _loadLock.Wait();
            try
            {
                // reset all cached data
                ResetCachedData();

                // resolve configuration file
                LegacyConfiguration config;
                var resolver = new ConfigurationResolver();
                switch (ConfigurationSource)
                {
                    case ConfigurationManagerSource.Xml:
                        config = resolver.ResolveLegacyConfigurationFromXml(ConfigurationFilename);
                        break;
                    case ConfigurationManagerSource.Json:
                        config = resolver.ResolveLegacyConfigurationFromJson(ConfigurationFilename);
                        break;
                    case ConfigurationManagerSource.Auto:
                    default:
                        config = resolver.ResolveLegacyConfigurationFromXml(ConfigurationFilename) ?? resolver.ResolveLegacyConfigurationFromJson(ConfigurationFilename);
                        break;
                }
                return config;
            }
            finally
            {
                _loadLock.Release();
            }
        }

        private static void ResetCachedData()
        {
            _cachedConnectionStrings = new Lazy<ConnectionStringSettingsCollection>(() => GetConnectionStrings());
            _cachedConnectionStringsDictionary = new Lazy<ReadOnlyDictionary<string, ConnectionStringSetting>>(() => GetConnectionStringsAsDictionary());
            _cachedAppSettings = new Lazy<GenericNameValueCollection>(() => GetAppSettings());
            _cachedAppSettingsDictionary = new Lazy<ReadOnlyDictionary<string, string>>(() => GetAppSettingsAsDictionary());
        }
    }
}
