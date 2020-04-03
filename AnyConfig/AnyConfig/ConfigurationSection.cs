using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

namespace AnyConfig
{
    public class ConfigurationSection : IConfigurationSection
    {
        public string this[string key]
        {
            get
            {
                return null;
            }
            set
            {

            }
        }

        public string Key { get; private set; }

        public string Path { get; private set; }

        public string Value { get; set; }

        public ConfigurationSection(string path, string key, string value)
        {
            Path = path;
            Key = key;
            Value = value;
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            throw new NotImplementedException();
        }

        public IChangeToken GetReloadToken()
        {
            throw new NotImplementedException();
        }

        public IConfigurationSection GetSection(string key)
        {
            throw new NotImplementedException();
        }
    }
}
