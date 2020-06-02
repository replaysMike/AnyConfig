using AnyConfig.Collections;
using AnyConfig.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace AnyConfig
{
    /// <summary>
    /// AnyConfig Legacy configuration manager
    /// Supports both Xml and Json configuration
    /// </summary>
    public static partial class ConfigurationManager
    {
        /// <summary>
        /// The source configuration to load
        /// </summary>
        public static ConfigurationManagerSource ConfigurationSource { get; set; } = ConfigurationManagerSource.Auto;

        /// <summary>
        /// Get/set the configuration filename
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

            if (section?.TypeValue == typeof(RequiresJsonSerialization) || section?.TypeValue == typeof(RequiresXmlSerialization))
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

        /// <summary>
        /// Open the configuration for the current executable
        /// </summary>
        /// <param name="userLevel"></param>
        /// <returns></returns>
        public static Models.Configuration OpenExeConfiguration(ConfigurationUserLevel userLevel)
        {
            return _legacyConfiguration.Value.Configuration;
        }
    }
}
