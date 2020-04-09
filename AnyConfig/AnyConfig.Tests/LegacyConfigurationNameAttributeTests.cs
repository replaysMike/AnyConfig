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
            var legacySettingNameConfig = Config.GetFromJsonFile<LegacySettingNameConfiguration>("legacysettingname.json");

            Assert.NotNull(legacySettingNameConfig);
            Assert.AreEqual("Some value", legacySettingNameConfig.AlternateName);
        }

        [Test]
        public void Legacy_SettingName_Xml_ShouldMap()
        {
            var legacySettingNameConfig = Config.GetFromXmlFile<LegacySettingNameConfiguration>("legacysettingname.config");

            Assert.NotNull(legacySettingNameConfig);
            Assert.AreEqual("Some value", legacySettingNameConfig.AlternateName);
        }
    }
}
