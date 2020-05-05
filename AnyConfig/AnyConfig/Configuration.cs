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
                    .Where(x => x.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                    .Select(x => x.Value)
                    .FirstOrDefault();
            }
            set
            {
                var item = _configurationSections
                    .Where(x => x.Key.Equals(value, StringComparison.InvariantCultureIgnoreCase))
                    .Select(x => x.Value)
                    .FirstOrDefault();
                item = value;
            }
        }

        /// <summary>
        /// The configuration file that was resolved
        /// </summary>
        public string ResolvedConfigurationFile { get; internal set; }

        public Configuration(string resolvedConfigurationFile)
        {
            ResolvedConfigurationFile = resolvedConfigurationFile;
        }

        public Configuration(List<IConfigurationSection> configurationSections, string resolvedConfigurationFile) : this(resolvedConfigurationFile)
        {
            _configurationSections = configurationSections;
        }

        public void AddSection(JsonNode node)
        {
            var section = new ConfigurationSection(node.FullPath, node.Name, node.Value, node);
            _configurationSections.Add(section);
        }

        public IEnumerable<IConfigurationSection> GetChildren() => _configurationSections;

        public IChangeToken GetReloadToken() => new ConfigurationReloadToken();

        public IConfigurationSection GetSection(string key)
        {
            var configSection = _configurationSections.FirstOrDefault(x => x.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase));
            if (configSection != null)
                return configSection;
            // always return a configuration section
            return new ConfigurationSection(key, key, null, null);
        }
    }
}
