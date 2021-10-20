using AnyConfig.Collections;
using AnyConfig.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using TypeSupport.Extensions;

namespace AnyConfig.Models
{
    public class Configuration
    {
        public SectionCollection<ConnectionStringPair> ConnectionStrings { get; set; } = new SectionCollection<ConnectionStringPair>();
        public SectionCollection<AppSettingPair> AppSettings { get; set; } = new SectionCollection<AppSettingPair>();
        public SectionCollection<ConfigSectionPair> ConfigSections { get; set; } = new SectionCollection<ConfigSectionPair>();
        public SectionCollection<AppSettingPair> AnyConfigSettings { get; set; } = new SectionCollection<AppSettingPair>();
        public SectionCollection<AnyConfigGroup> AnyConfigGroups { get; set; } = new SectionCollection<AnyConfigGroup>();

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

            if (key.Contains(":"))
            {
                var value = GetHeirarchyValue(key);
                if (value != null)
                    return new StringValue(value).As<T>();
            }

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

            foreach (var section in ConfigSections)
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

        public LegacyConfigurationSection GetSection(string name)
        {
            switch (name.ToLower())
            {
                case "appsettings":
                    return new LegacyConfigurationSection(this, AppSettings);
                case "connectionstrings":
                    return new LegacyConfigurationSection(this, ConnectionStrings);
                default:
                    return new LegacyConfigurationSection(this, ConfigSections.FirstOrDefault(x => x.Name.Equals(name)).Configuration);
            }
        }

        public string GetHeirarchyValue(string key)
        {
            var parts = key.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
            {
                switch (parts[0].ToLower())
                {
                    case "appsettings":
                        return AppSettings[parts[1]].Value;
                    case "connectionstrings":
                        return ConnectionStrings[parts[1]].ConnectionStringSetting.ConnectionString;
                    case "anyconfig":
                        return AnyConfigSettings[parts[1]].Value;
                }
            }
            return null;
        }
    }

    public class AnyConfigGroup : IKeyable<AnyConfigGroup>
    {
        public string GroupName { get; set; }
        public List<AnyConfigAppSettingPair> Settings { get; set; } = new List<AnyConfigAppSettingPair>();

        public string Key => GroupName;

        public void Set(AnyConfigGroup value)
        {
            GroupName = value.GroupName;
            Settings = value.Settings;
        }

        public void Set(object value) => Set(value as AnyConfigGroup);
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

    public class ConfigSectionPair : IKeyable<ConfigSectionPair>
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public Type TypeValue { get; set; }
        public string ConfigProtectionProvider { get; set; }

        public object Configuration { get; set; }

        public string Key => Name;

        public void Set(ConfigSectionPair value)
        {
            Name = value.Name;
            Type = value.Type;
            TypeValue = value.TypeValue;
            ConfigProtectionProvider = value.ConfigProtectionProvider;
            Configuration = value.Configuration;
        }

        public void Set(object value) => Set(value as ConfigSectionPair);

        public override string ToString()
        {
            return $"{Name}:{Type}";
        }
    }

    public class AppSettingPair : IKeyable<AppSettingPair>
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public void Set(AppSettingPair value)
        {
            Key = value.Key;
            Value = value.Value;
        }

        public void Set(object value) => Set(value as AppSettingPair);

        public override string ToString()
        {
            return $"{Key} => {Value}";
        }
    }

    public class ConnectionStringPair : IKeyable<ConnectionStringPair>
    {
        public string Name { get; set; }
        public ConnectionStringSetting ConnectionStringSetting { get; set; }

        public string Key => Name;

        public void Set(ConnectionStringPair value)
        {
            Name = value.Name;
            ConnectionStringSetting = value.ConnectionStringSetting;
        }

        public void Set(object value) => Set(value as ConnectionStringPair);

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
