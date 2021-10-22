using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace AnyConfig
{
    /// <summary>
    /// AnyConfig configuration root
    /// </summary>
    public class ConfigurationRoot : Configuration, IConfigurationRoot
    {
        private readonly List<IConfigurationProvider> _configurationProviders = new List<IConfigurationProvider>();

        public ConfigurationRoot(string resolvedConfigurationFile) : base(resolvedConfigurationFile)
        {
        }

        public ConfigurationRoot(List<IConfigurationSection> configurationSections, string resolvedConfigurationFile) 
            : base(configurationSections, resolvedConfigurationFile)
        {
        }

        public ConfigurationRoot(List<IConfigurationSection> configurationSections, List<IConfigurationProvider> configurationProviders, string resolvedConfigurationFile) 
            : base(configurationSections, resolvedConfigurationFile)
        {
            _configurationProviders = configurationProviders;
        }

        public IEnumerable<IConfigurationProvider> Providers => _configurationProviders;
        IEnumerable<IConfigurationProvider> IConfigurationRoot.Providers => Providers;

        internal void AddProvider(IConfigurationProvider configurationProvider)
        {
            _configurationProviders.Add(configurationProvider);
        }

        public void Reload()
        {
            // does nothing
        }

        void IConfigurationRoot.Reload() => Reload();
    }
}
