using NUnit.Framework;

namespace AnyConfig.Tests
{
    [TestFixture]
    public class ConfigProviderTests
    {
        [Test]
        public void Should_ConvertStringToNativeType_NullDefaultNullBool()
        {
            var obj = ConfigProvider.ConvertStringToNativeType(typeof(bool), null, null);
            Assert.AreEqual(false, obj);
        }

        [Test]
        public void Should_ConvertStringToNativeType_NullDefaultNullInt()
        {
            var obj = ConfigProvider.ConvertStringToNativeType(typeof(int), null, null);
            Assert.AreEqual(null, obj);
        }

        [Test]
        public void Should_ConvertStringToNativeType_NullInt()
        {
            var obj = ConfigProvider.ConvertStringToNativeType(typeof(int), null, 999);
            Assert.AreEqual(999, obj);
        }

        [Test]
        public void Should_ConvertStringToNativeType_Int()
        {
            var obj = ConfigProvider.ConvertStringToNativeType(typeof(int), "999", null);
            Assert.AreEqual(999, obj);
        }

        [Test]
        public void Should_ConvertStringToNativeType_InvalidInt()
        {
            var obj = ConfigProvider.ConvertStringToNativeType(typeof(int), "asdf", 999);
            Assert.AreEqual(999, obj);
        }

        [Test]
        public void Should_ConvertStringToNativeType_NullDefaultInvalidInt()
        {
            var obj = ConfigProvider.ConvertStringToNativeType(typeof(int), "asdf", null);
            Assert.AreEqual(null, obj);
        }
    }
}
