using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnyConfig.Tests
{
    [TestFixture]
    [NonParallelizable]
    public class ConfigTests
    {
        [Test]
        public void Should_Get_Byte()
        {
            var value = Config.Get<byte>("ByteSetting");
            Assert.AreEqual(254, value);
        }

        [Test]
        public void Should_Get_Int16()
        {
            var value = Config.Get<short>("ShortSetting");
            Assert.AreEqual(32765, value);
        }

        [Test]
        public void Should_Get_Int32()
        {
            var value = Config.Get<int>("IntSetting");
            Assert.AreEqual(1, value);
        }

        [Test]
        public void Should_Get_Int64()
        {
            var value = Config.Get<long>("LongSetting");
            Assert.AreEqual(6147483647L, value);
        }

        [Test]
        public void Should_Get_Double()
        {
            var value = Config.Get<double>("DoubleSetting");
            Assert.AreEqual(3.14159265359d, value);
        }

        [Test]
        public void Should_Get_Decimal()
        {
            var value = Config.Get<double>("DecimalSetting");
            Assert.AreEqual(6.28318530718m, value);
        }

        [Test]
        public void Should_Get_Bool()
        {
            var value = Config.Get<bool>("BoolSetting");
            Assert.AreEqual(true, value);
        }

        [Test]
        public void Should_Get_String()
        {
            var value = Config.Get<string>("StringSetting");
            Assert.AreEqual("TestValue", value);
        }

        [Test]
        public void Should_Get_Enum()
        {
            var value = Config.Get<CustomEnum>("CustomEnumSetting");
            Assert.AreEqual(CustomEnum.Second, value);
        }

        [Test]
        public void Should_Get_EnumInteger()
        {
            var value = Config.Get<CustomEnum>("CustomEnumNumericSetting");
            Assert.AreEqual(CustomEnum.Second, value);
        }
    }
}
