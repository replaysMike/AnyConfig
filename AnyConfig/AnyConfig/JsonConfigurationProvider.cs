using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Linq;

namespace AnyConfig
{
    public class JsonConfigurationProvider : IConfigurationProvider
    {
        public ICollection<KeyValuePair<string, string>> Data { get; private set; }
        public ICollection<string> Source { get; }

        /// <summary>
        /// The configuration file that was resolved
        /// </summary>
        public string ResolvedConfigurationFile { get; internal set; }

        public JsonConfigurationProvider(string resolvedConfigurationFile)
        {
            ResolvedConfigurationFile = resolvedConfigurationFile;
        }

        public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string parentPath) 
            => Data.Select(x => x.Key).ToList();

        /// <summary>
        /// Get child keys that start with <paramref name="key"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IEnumerable<string> GetChildKeys(string key)
            => Data.Where(x => x.Key.StartsWith(key)).Select(x => x.Key).ToList();

        /// <summary>
        /// Get child keys containing certain text
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IEnumerable<string> GetChildKeysContaining(string key)
            => Data.Where(x => x.Key.Contains(key)).Select(x => x.Key).ToList();

        public IChangeToken GetReloadToken() => new ConfigurationReloadToken();

        public void Load()
        {
            // does nothing
        }

        private void SortData()
        {
            // sorting the data mimick's Microsoft's behavior
            Data = Data.OrderBy(x => x.Key).ToList();
        }

        internal void SetData(ICollection<KeyValuePair<string, string>> data)
        {
            Data = data;
            SortData();
        }

        public void Set(string key, string value)
        {
            Data.Add(new KeyValuePair<string, string>(key, value));
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
