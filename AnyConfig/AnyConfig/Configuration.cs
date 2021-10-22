using AnyConfig.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AnyConfig
{
    /// <summary>
    /// AnyConfig configuration
    /// </summary>
    public class Configuration : IConfiguration
    {
        private readonly SemaphoreSlim _dataLock = new SemaphoreSlim(1, 1);
        protected readonly List<IConfigurationSection> _configurationSections = new List<IConfigurationSection>();

        public string this[string key]
        {
            get
            {
                _dataLock.Wait();
                try
                {
                    return _configurationSections
                        .Where(x => x.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                        .Select(x => x.Value)
                        .FirstOrDefault();
                }
                finally
                {
                    _dataLock.Release();
                }
            }
            set
            {
                _dataLock.Wait();
                try
                {
                    var item = _configurationSections
                        .Where(x => x.Key.Equals(value, StringComparison.InvariantCultureIgnoreCase))
                        .Select(x => x.Value)
                        .FirstOrDefault();
                    item = value;
                }
                finally
                {
                    _dataLock.Release();
                }
            }
        }

        string IConfiguration.this[string key]
        {
            get { return this[key]; }
            set { this[key] = value; }
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
            _dataLock.Wait();
            try
            {
                _configurationSections = configurationSections;
            }
            finally
            {
                _dataLock.Release();
            }
        }

        public void AddSection(JsonNode node)
        {
            var section = new ConfigurationSection(node.FullPath, node.Name, node.Value, node);
            _dataLock.Wait();
            try
            {
                _configurationSections.Add(section);
            }
            finally
            {
                _dataLock.Release();
            }
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            _dataLock.Wait();
            try
            {
                return _configurationSections;
            }
            finally
            {
                _dataLock.Release();
            }
        }

        IEnumerable<IConfigurationSection> IConfiguration.GetChildren() => GetChildren();

        public IChangeToken GetReloadToken() => new ConfigurationReloadToken();

        IChangeToken IConfiguration.GetReloadToken() => GetReloadToken();

        public IConfigurationSection GetSection(string key)
        {
            _dataLock.Wait();
            try
            {
                var configSection = _configurationSections.FirstOrDefault(x => x.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase));
                if (configSection != null)
                    return configSection;
            }
            finally
            {
                _dataLock.Release();
            }
            // always return a configuration section
            return new ConfigurationSection(key, key, null, null);
        }

        IConfigurationSection IConfiguration.GetSection(string key) => GetSection(key);
    }
}
