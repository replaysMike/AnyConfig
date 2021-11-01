using AnyConfig.Exceptions;
using System;
using System.Collections.Generic;

namespace AnyConfig
{
    /// <summary>
    /// Manages a static cache
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class CachedDataProvider<T>
    {
        private static object _dataLock = new object();
        private static Dictionary<string, T> _cachedObjects = new Dictionary<string, T>();

        /// <summary>
        /// Returns the number of items in the cache
        /// </summary>
        internal int Count
        {
            get
            {
                lock (_dataLock)
                {
                    return _cachedObjects.Count;
                }
            }
        }

        /// <summary>
        /// Add file to cache and return its contents, or get cached file contents
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        internal virtual object AddOrGet(string key, Func<T> addMethod)
        {
            lock (_dataLock)
            {
                if (_cachedObjects.ContainsKey(key))
                    return _cachedObjects[key];

                var contents = addMethod();
                _cachedObjects.Add(key, contents);
                return contents;
            }
            throw new ConfigurationException($"Failed to add object named '{key}' to the cache!");
        }

        /// <summary>
        /// Remove item from the cache
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal bool Remove(string key)
        {
            lock (_dataLock)
            {
                return _cachedObjects.Remove(key);
            }
        }

        /// <summary>
        /// Clear the cache
        /// </summary>
        internal void ClearCache()
        {
            lock (_dataLock)
            {
                _cachedObjects.Clear();
            }
        }

        /// <summary>
        /// True if key exists in the cache
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal bool ContainsKey(string key)
        {
            lock (_dataLock)
            {
                return _cachedObjects.ContainsKey(key);
            }
        }
    }
}
