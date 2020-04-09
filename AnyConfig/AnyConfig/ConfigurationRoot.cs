using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace AnyConfig
{
    public class ConfigurationRoot : Configuration, IConfigurationRoot
    {
        private readonly List<IConfigurationProvider> _configurationProviders = new List<IConfigurationProvider>();

        public ConfigurationRoot()
        {

        }

        public ConfigurationRoot(List<IConfigurationSection> configurationSections) : base(configurationSections)
        {
        }

        public ConfigurationRoot(List<IConfigurationSection> configurationSections, List<IConfigurationProvider> configurationProviders) : base(configurationSections)
        {
            _configurationProviders = configurationProviders;
        }

        public IEnumerable<IConfigurationProvider> Providers => _configurationProviders;

        internal void AddProvider(IConfigurationProvider configurationProvider)
        {
            _configurationProviders.Add(configurationProvider);
        }

        public void Reload()
        {
            
        }
    }
}
