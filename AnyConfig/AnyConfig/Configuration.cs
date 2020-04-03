using AnyConfig.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnyConfig
{
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
                // do nothing
            }
        }

        public Configuration() { }

        public Configuration(List<IConfigurationSection> configurationSections)
        {
            _configurationSections = configurationSections;
        }

        public void AddSection(JsonNode node)
        {
            var section = new ConfigurationSection(node.FullPath, node.Name, node.Json);
            _configurationSections.Add(section);
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            return _configurationSections;
        }

        public IChangeToken GetReloadToken()
        {
            throw new NotImplementedException();
        }

        public IConfigurationSection GetSection(string key)
        {
            return _configurationSections.FirstOrDefault(x => x.Key == key);
        }
    }
}
