using NUnit.Framework;
using System;

namespace AnyConfig.Tests
{
    [TestFixture]
    [NonParallelizable]
    public class CachedDataProviderTests
    {
        [Test]
        public void Should_CacheObject()
        {
            var provider = new CachedDataProvider<object>();
            var cacheCount = 0;
            var key = nameof(Should_CacheObject);

            Func<object> cacheCounter = () =>
            {
                cacheCount++;
                return "test";  
            };

            // add a new value to the cache
            var value = provider.AddOrGet(key, cacheCounter);
            Assert.AreEqual("test", value);
            Assert.AreEqual(1, cacheCount);

            // add method should not get called and return previous data
            value = provider.AddOrGet(key, cacheCounter);
            Assert.AreEqual("test", value);
            Assert.AreEqual(1, cacheCount);
        }

        [Test]
        public void Should_RemoveFromCache()
        {
            var provider = new CachedDataProvider<object>();

            // add a new value to the cache
            var key = nameof(Should_RemoveFromCache);
            var value = provider.AddOrGet(key, () => {
                return "test";  
            });
            Assert.AreEqual("test", value);

            provider.Remove(key);

            Assert.AreEqual(false, provider.ContainsKey(key));
        }

        [Test]
        public void Should_ClearCache()
        {
            var provider = new CachedDataProvider<object>();

            // add a new value to the cache
            var key = nameof(Should_ClearCache);
            var value = provider.AddOrGet(key, () => {
                return "test";  
            });
            Assert.AreEqual("test", value);
            Assert.Greater(provider.Count, 0);

            provider.ClearCache();

            Assert.AreEqual(0, provider.Count);
        }
    }
}
