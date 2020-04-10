using NUnit.Framework;
using System.Reflection;

namespace AnyConfig.Tests
{
    [TestFixture]
    public class LegacyConfigurationNameAttributeTests
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            // test projects using nunit/mstest framework will hide the entry assembly, so we must register it manually
            ConfigurationResolver.RegisterEntryAssembly(Assembly.GetExecutingAssembly());
        }

        [Test]
        public void Legacy_SettingName_Json_ShouldMap()
        {
            var config = Config.GetFromJsonFile<LegacySettingNameConfiguration>("legacysettingname.json");

            Assert.NotNull(config);
            Assert.AreEqual("Some value", config.AlternateName);
        }

        [Test]
        public void Legacy_SettingName_Xml_ShouldMap()
        {
            var config = Config.GetFromXmlFile<LegacySettingNameConfiguration>("legacysettingname.config");

            Assert.NotNull(config);
            Assert.AreEqual("Some value", config.AlternateName);
        }

        [Test]
        public void Legacy_TestConfiguration_Json_ShouldFlatMap()
        {
            var config = Config.GetFromJsonFile<TestConfiguration>("legacyflatmap.json");

            Assert.NotNull(config);
            Assert.AreEqual(true, config.BoolSetting);
            Assert.AreEqual("TestValue", config.StringSetting);
            Assert.AreEqual(1, config.IntSetting);
            Assert.AreEqual("TestName", config.TestConfigurationObject.Name);
        }

        [Test]
        public void Legacy_TestConfiguration_Xml_ShouldFlatMap()
        {
            var config = Config.GetFromXmlFile<TestConfiguration>("legacyflatmap.config");

            Assert.NotNull(config);
            Assert.AreEqual(true, config.BoolSetting);
            Assert.AreEqual("TestValue", config.StringSetting);
            Assert.AreEqual(1, config.IntSetting);
            Assert.AreEqual("TestName", config.TestConfigurationObject.Name);
        }
    }
}
