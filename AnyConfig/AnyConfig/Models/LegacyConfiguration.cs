using System;
using System.Collections.Generic;

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
