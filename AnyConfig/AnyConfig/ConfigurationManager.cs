using AnyConfig.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        private static SemaphoreSlim _loadLock = new SemaphoreSlim(1, 1);
        private static Lazy<LegacyConfiguration> _legacyConfiguration = new Lazy<LegacyConfiguration>(() => LoadConfiguration());

        /// <summary>
        /// The source configuration to load
        /// </summary>
        public static ConfigurationManagerSource ConfigurationSource = ConfigurationManagerSource.Auto;

        /// <summary>
        /// The configuration filename
        /// </summary>
        public static string ConfigurationFilename { get; set; }

        /// <summary>
        /// Connection strings
        /// </summary>
        public static ConnectionStringSettingsCollection ConnectionStrings => GetConnectionStrings();

        /// <summary>
        /// Connection strings
        /// </summary>
        public static Dictionary<string, ConnectionStringSetting> ConnectionStringsAsDictionary => GetConnectionStringsAsDictionary();

        /// <summary>
        /// Application settings
        /// </summary>
        public static NameValueCollection AppSettings => GetAppSettings();

        /// <summary>
        /// Application settings
        /// </summary>
        public static Dictionary<string, string> AppSettingsAsDictionary => GetAppSettingsAsDictionary();

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
        /// Force reset the configuration manager cache
        /// </summary>
        public static void ResetCache()
        {
            _legacyConfiguration = new Lazy<LegacyConfiguration>(() => LoadConfiguration());
        }

        private static ConnectionStringSettingsCollection GetConnectionStrings()
        {
            return new ConnectionStringSettingsCollection(GetConnectionStringsAsDictionary());
        }

        private static Dictionary<string, ConnectionStringSetting> GetConnectionStringsAsDictionary()
        {
            return _legacyConfiguration.Value.Configuration.ConnectionStrings.ToDictionary(x => x.Name, x => x.ConnectionStringSetting);
        }

        private static NameValueCollection GetAppSettings()
        {
            return _legacyConfiguration.Value.Configuration.AppSettings.Aggregate(new NameValueCollection(), (seed, current) =>
            {
                seed.Add(current.Key, current.Value);
                return seed;
            });
        }

        private static Dictionary<string, string> GetAppSettingsAsDictionary()
        {
            return _legacyConfiguration.Value.Configuration.AppSettings.ToDictionary(x => x.Key, x => x.Value);
        }

        private static LegacyConfiguration LoadConfiguration()
        {
            _loadLock.Wait();
            try
            {
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
    }
}
