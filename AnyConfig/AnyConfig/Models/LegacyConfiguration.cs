using AnyConfig.Collections;
using AnyConfig.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using TypeSupport.Extensions;

namespace AnyConfig.Models
{
    public class LegacyConfiguration
    {
        public Configuration Configuration { get; set; } = new Configuration();
    }

    public class Configuration
    {
        public List<ConnectionStringPair> ConnectionStrings { get; set; } = new List<ConnectionStringPair>();
        public List<AppSettingPair> AppSettings { get; set; } = new List<AppSettingPair>();
        public List<ConfigSectionPair> ConfigSections { get; set; } = new List<ConfigSectionPair>();
        public List<AppSettingPair> AnyConfigSettings { get; set; } = new List<AppSettingPair>();
        public List<AnyConfigGroup> AnyConfigGroups { get; set; } = new List<AnyConfigGroup>();

        /// <summary>
        /// Get a configuration value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key) => Get<T>(key, true);

        /// <summary>
        /// Get a configuration value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="throwsException">True to throw an exception if setting is not found</param>
        /// <returns></returns>
        public T Get<T>(string key, bool throwsException)
        {
            var anyConfigSetting = AnyConfigSettings.Where(x => x.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                .Select(x => x.Value)
                .FirstOrDefault();
            if (anyConfigSetting != null)
                return new StringValue(anyConfigSetting).As<T>();

            var appSetting = AppSettings.Where(x => x.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                .Select(x => x.Value)
                .FirstOrDefault();
            if (appSetting != null)
                return new StringValue(appSetting).As<T>();

            var anyConfigGroupSetting = AnyConfigGroups
                .SelectMany(x => x.Settings.Where(y => y.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)))
                .Select(x => x.Value)
                .FirstOrDefault();
            if (anyConfigGroupSetting.Value != null)
                return anyConfigGroupSetting.As<T>();
          
            var connectionString = ConnectionStrings.Where(x => x.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                .Select(x => x.ConnectionStringSetting.ConnectionString)
                .FirstOrDefault();
            if (connectionString != null)
                return new StringValue(connectionString).As<T>();
            
            foreach(var section in ConfigSections)
            {
                var matchingProperty = section.Configuration.GetProperties(PropertyOptions.Public)
                    .FirstOrDefault(x => x.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase));
                if (matchingProperty != null)
                    return (T)matchingProperty.PropertyInfo.GetValue(section);
            }
            if (throwsException)
                throw new ConfigurationMissingException($"Unknown setting '{key}'");
            return default;
        }

        /// <summary>
        /// Get a configuration value from a specified AnyConfig group
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="groupName">Name of custom group</param>
        /// <param name="key">Key name</param>
        /// <returns></returns>
        public T Get<T>(string groupName, string key) => Get<T>(groupName, key, true);

        /// <summary>
        /// Get a configuration value from a specified AnyConfig group
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="groupName">Name of custom group</param>
        /// <param name="key">Key name</param>
        /// <param name="throwsException">True to throw an exception if setting is not found</param>
        /// <returns></returns>
        public T Get<T>(string groupName, string key, bool throwsException)
        {
            var group = AnyConfigGroups
                .Where(x => x.GroupName.Equals(groupName, StringComparison.InvariantCultureIgnoreCase))
                .SelectMany(x => x.Settings);
            if (group == null)
            {
                if (throwsException)
                    throw new ConfigurationMissingException($"Unknown configuration group named '{groupName}'");
                else
                    return default;
            }
            return group
                .Where(x => x.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                .Select(x => x.Value)
                .FirstOrDefault()
                .As<T>();
        }
    }

    public class AnyConfigGroup
    {
        public string GroupName { get; set; }
        public List<AnyConfigAppSettingPair> Settings { get; set; } = new List<AnyConfigAppSettingPair>();
    }

    public class AnyConfigAppSettingPair
    {
        public string Key { get; set; }
        public StringValue Value { get; set; }

        public override string ToString()
        {
            return $"{Key} => {Value}";
        }
    }

    public class ConfigSectionPair
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public Type TypeValue { get; set; }
        public object Configuration { get; set; }

        public override string ToString()
        {
            return $"{Name}:{Type}";
        }
    }

    public class AppSettingPair
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            return $"{Key} => {Value}";
        }
    }

    public class ConnectionStringPair
    {
        public string Name { get; set; }
        public ConnectionStringSetting ConnectionStringSetting { get; set; }

        public override string ToString()
        {
            return $"{Name}:{ConnectionStringSetting}";
        }
    }

    public class ConnectionStringSetting
    {
        public string ConnectionString { get; set; }
        public string Name { get; set; }
        public string ProviderName { get; set; }

        public override string ToString()
        {
            return $"{ConnectionString}";
        }
    }
}
