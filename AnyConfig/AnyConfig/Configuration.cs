using AnyConfig.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnyConfig
{
    /// <summary>
    /// AnyConfig configuration
    /// </summary>
    public class Configuration : IConfiguration
    {
        protected readonly List<IConfigurationSection> _configurationSections = new List<IConfigurationSection>();

        public string this[string key]
        {
            get
            {
                return _configurationSections
                    .Where(x => x.Key == key)
                    .Select(x => x.Value)
                    .FirstOrDefault();
            }
            set
            {
                var item = _configurationSections
                    .Where(x => x.Key == key)
                    .Select(x => x.Value)
                    .FirstOrDefault();
                item = value;
            }
        }

        public Configuration() { }

        public Configuration(List<IConfigurationSection> configurationSections)
        {
            _configurationSections = configurationSections;
        }

        public void AddSection(JsonNode node)
        {
            var section = new ConfigurationSection(node.FullPath, node.Name, node.OuterText, node.OuterText);
            _configurationSections.Add(section);
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            return _configurationSections;
        }

        public IChangeToken GetReloadToken()
        {
            return new ConfigurationReloadToken();
        }

        public IConfigurationSection GetSection(string key)
        {
            var configSection = _configurationSections.FirstOrDefault(x => x.Key == key);
            if (configSection != null)
                return configSection;
            // always return a configuration section
            return new ConfigurationSection(key, key, null, null);
        }
    }
}
