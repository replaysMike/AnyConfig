using NUnit.Framework;

namespace AnyConfig.NetFrameworkOnlyTests
{
    [TestFixture]
    public class ConfigTests
    {
        [Test]
        public void Should_RenderConfig()
        {
            var value = Config.Get<bool>("NonExistantKey", false);
            Assert.AreEqual(false, value);
            var value2 = Config.Get<bool>("testMode", false);
            Assert.AreEqual(true, value2);
        }
    }
}
