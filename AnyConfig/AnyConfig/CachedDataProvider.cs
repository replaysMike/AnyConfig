using AnyConfig.Exceptions;
using System;
using System.Collections.Concurrent;

namespace AnyConfig
{
    /// <summary>
    /// Manages a static cache
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class CachedDataProvider<T>
    {
        internal static ConcurrentDictionary<string, T> _cachedObjects = new ConcurrentDictionary<string, T>();

        /// <summary>
        /// Returns the number of items in the cache
        /// </summary>
        internal int Count => _cachedObjects.Count;


        /// <summary>
        /// Add file to cache and return its contents, or get cached file contents
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        internal virtual object AddOrGet(string key, Func<T> addMethod)
        {
            if (_cachedObjects.ContainsKey(key))
                return _cachedObjects[key];

            var contents = addMethod();
            if (_cachedObjects.TryAdd(key, contents))
                return contents;

            throw new ConfigurationException($"Failed to add object named '{key}' to the cache!");
        }

        /// <summary>
        /// Remove item from the cache
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal bool Remove(string key)
        {
            return _cachedObjects.TryRemove(key, out var _);
        }

        /// <summary>
        /// Clear the cache
        /// </summary>
        internal void ClearCache()
        {
            _cachedObjects.Clear();
        }

        /// <summary>
        /// True if key exists in the cache
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal bool ContainsKey(string key) => _cachedObjects.ContainsKey(key);
    }
}
