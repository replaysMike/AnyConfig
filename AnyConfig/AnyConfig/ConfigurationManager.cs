using AnyConfig.Collections;
using AnyConfig.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
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
        private static Lazy<ReadOnlyDictionary<string, AnyConfigAppSettingCollection>> _cachedAnyConfigGroupedAppSettings = new Lazy<ReadOnlyDictionary<string, AnyConfigAppSettingCollection>>(() => GetAnyConfigGroupedAppSettings());

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
        /// AnyConfig grouped settings
        /// </summary>
        public static ReadOnlyDictionary<string, AnyConfigAppSettingCollection> AnySettings => _cachedAnyConfigGroupedAppSettings.Value;

        /// <summary>
        /// Get a custom configuration section
        /// </summary>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public static object GetSection(string sectionName)
        {
            var section = _legacyConfiguration.Value.Configuration.ConfigSections
                .Where(x => x.Name.Equals(sectionName))
                .FirstOrDefault();

            if (section?.TypeValue == typeof(RequiresJsonSerialization))
            {
                // we don't know the intended type, so return the data that contains everything required to understand serialization requirements
                return section;
            }

            return section?.Configuration;
        }

        /// <summary>
        /// Get a configuration value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Key name</param>
        /// <returns></returns>
        public static T Get<T>(string key) => Get<T>(key, true);

        /// <summary>
        /// Get a configuration value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Key name</param>
        /// <param name="throwsException">True to throw an exception if setting is not found</param>
        /// <returns></returns>
        public static T Get<T>(string key, bool throwsException) => _legacyConfiguration.Value.Configuration.Get<T>(key, throwsException);

        /// <summary>
        /// Get a configuration value from an AnyConfig custom group
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="groupName"></param>
        /// <param name="key">Key name</param>
        /// <returns></returns>
        public static T Get<T>(string groupName, string key) => Get<T>(groupName, key, true);

        /// <summary>
        /// Get a configuration value from an AnyConfig custom group
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="groupName"></param>
        /// <param name="key">Key name</param>
        /// <param name="throwsException">True to throw an exception if setting is not found</param>
        /// <returns></returns>
        public static T Get<T>(string groupName, string key, bool throwsException) => _legacyConfiguration.Value.Configuration.Get<T>(groupName, key, throwsException);

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
                        configurationSource = ConfigurationManagerSource.Xml;
                        break;
                }
            }
            return configurationSource;
        }

        private static void ResetCachedData()
        {
            _cachedConnectionStrings = new Lazy<ConnectionStringSettingsCollection>(() => GetConnectionStrings());
            _cachedConnectionStringsDictionary = new Lazy<ReadOnlyDictionary<string, ConnectionStringSetting>>(() => GetConnectionStringsAsDictionary());
            _cachedAppSettings = new Lazy<GenericNameValueCollection>(() => GetAppSettings());
            _cachedAppSettingsDictionary = new Lazy<ReadOnlyDictionary<string, string>>(() => GetAppSettingsAsDictionary());
            _cachedAnyConfigGroupedAppSettings = new Lazy<ReadOnlyDictionary<string, AnyConfigAppSettingCollection>>(() => GetAnyConfigGroupedAppSettings());
        }
    }
}
