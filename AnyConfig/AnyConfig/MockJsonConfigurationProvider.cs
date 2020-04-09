using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnyConfig
{
    public class MockJsonConfigurationProvider : IConfigurationProvider
    {
        public ICollection<KeyValuePair<string, string>> Data { get; private set; }
        public ICollection<string> Source { get; }

        public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string parentPath)
        {
            return new List<string>();
        }


        public IChangeToken GetReloadToken()
        {
            throw new NotImplementedException();
        }

        public void Load()
        {
            // does nothing
        }

        internal void SetData(ICollection<KeyValuePair<string, string>> data)
        {
            Data = data;
        }

        public void Set(string key, string value)
        {
            throw new NotImplementedException();
        }

        public bool TryGet(string key, out string value)
        {
            value = string.Empty;
            if (Data.Any(x => x.Key == key))
            {
                value = Data
                    .Where(x => x.Key == key)
                    .Select(x => x.Value)
                    .FirstOrDefault();
                return true;
            }
            if (Data.Any(x => x.Key.EndsWith($":{key}")))
            {
                value = Data
                    .Where(x => x.Key.EndsWith($":{key}"))
                    .Select(x => x.Value)
                    .FirstOrDefault();
                return true;
            }

            return false;
        }
    }
}
