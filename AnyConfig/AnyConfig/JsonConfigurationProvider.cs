using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AnyConfig
{
    public class JsonConfigurationProvider : IConfigurationProvider
    {
        private readonly SemaphoreSlim _dataLock = new SemaphoreSlim(1, 1);
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
        {
            _dataLock.Wait();
            try
            {
                return Data
                    .Select(x => x.Key)
                    .ToList();
            }
            finally
            {
                _dataLock.Release();
            }
        }

        /// <summary>
        /// Get child keys that start with <paramref name="key"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IEnumerable<string> GetChildKeys(string key)
        {
            _dataLock.Wait();
            try
            {
                return Data
                    .Where(x => x.Key.StartsWith(key))
                    .Select(x => x.Key)
                    .ToList();
            }
            finally
            {
                _dataLock.Release();
            }
        }

        /// <summary>
        /// Get child keys containing certain text
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IEnumerable<string> GetChildKeysContaining(string key)
        {
            _dataLock.Wait();
            try
            {
                return Data
                    .Where(x => x.Key.Contains(key))
                    .Select(x => x.Key)
                    .ToList();
            }
            finally
            {
                _dataLock.Release();
            }
        }

        public IChangeToken GetReloadToken() => new ConfigurationReloadToken();

        public void Load()
        {
            // does nothing
        }

        internal void SetData(ICollection<KeyValuePair<string, string>> data)
        {
            _dataLock.Wait();
            try
            {
                Data = data;
                SortDataInternal();
            }
            finally
            {
                _dataLock.Release();
            }
        }

        private void SortDataInternal()
        {
            // sorting the data mimick's Microsoft's behavior
            Data = Data.OrderBy(x => x.Key).ToList();
        }


        public void Set(string key, string value)
        {
            _dataLock.Wait();
            try
            {
                Data.Add(new KeyValuePair<string, string>(key, value));
            }
            finally
            {
                _dataLock.Release();
            }
        }

        public bool TryGet(string key, out string value)
        {
            value = string.Empty;
            _dataLock.Wait();
            try
            {
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
            }
            finally
            {
                _dataLock.Release();
            }
            return false;
        }
    }
}
