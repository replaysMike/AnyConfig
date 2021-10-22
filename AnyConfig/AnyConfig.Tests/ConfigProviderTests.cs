using NUnit.Framework;
using System;

namespace AnyConfig.Tests
{
    [TestFixture]
    [NonParallelizable]
    public class ConfigProviderTests
    {
        [Test]
        public void Should_ConvertStringToNativeType_NullDefaultNullBool()
        {
            var obj = new ConfigProvider().ConvertStringToNativeType(typeof(bool), null, null);
            Assert.AreEqual(null, obj);
        }

        [Test]
        public void Should_ConvertStringToNativeType_NullDefaultNullInt()
        {
            var obj = new ConfigProvider().ConvertStringToNativeType(typeof(int), null, null);
            Assert.AreEqual(null, obj);
        }

        [Test]
        public void Should_ConvertStringToNativeType_NullInt()
        {
            var obj = new ConfigProvider().ConvertStringToNativeType(typeof(int), null, 999);
            Assert.AreEqual(999, obj);
        }

        [Test]
        public void Should_ConvertStringToNativeType_Int()
        {
            var obj = new ConfigProvider().ConvertStringToNativeType(typeof(int), "999", null);
            Assert.AreEqual(999, obj);
        }

        [Test]
        public void Should_ConvertStringToNativeType_InvalidInt()
        {
            var obj = new ConfigProvider().ConvertStringToNativeType(typeof(int), "asdf", 999);
            Assert.AreEqual(999, obj);
        }

        [Test]
        public void Should_ConvertStringToNativeType_NullDefaultInvalidInt()
        {
            var obj = new ConfigProvider().ConvertStringToNativeType(typeof(int), "asdf", null);
            Assert.AreEqual(null, obj);
        }

        [Test]
        public void Should_ConvertStringToNativeType_MismatchedDefaultValue()
        {
            var obj = new ConfigProvider().ConvertStringToNativeType(typeof(int), "asdf", true);
            Assert.AreEqual(true, obj);
        }

        [Test]
        public void Should_ConvertStringToNativeType_Enum()
        {
            var obj = new ConfigProvider().ConvertStringToNativeType(typeof(ConsoleColor), "Red", null);
            Assert.AreEqual(ConsoleColor.Red, obj);
        }

        [Test]
        public void Should_ConvertStringToNativeType_NullEnum()
        {
            var obj = new ConfigProvider().ConvertStringToNativeType(typeof(ConsoleColor), null, null);
            Assert.AreEqual(null, obj);
        }
    }
}
