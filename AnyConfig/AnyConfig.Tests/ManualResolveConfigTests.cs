using NUnit.Framework;
using System.Reflection;

namespace AnyConfig.Tests
{
    [TestFixture]
    public class ManualResolveConfigTests
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            // test projects using nunit/mstest framework will hide the entry assembly, so we must register it manually
            ConfigurationResolver.RegisterEntryAssembly(Assembly.GetExecutingAssembly());
        }

        [Test]
        public void Should_Load_Xml_Config()
        {
            var testConfig = new TestConfiguration
            {
                BoolSetting = true,
                StringSetting = "TestValue",
                IntSetting = 1,
                TestConfigurationObject = new TestConfigurationObject
                {
                    Name = "TestName",
                    Value = "TestValue"
                }
            };
            var config = Config.GetFromXml<TestConfiguration>();
            Assert.AreEqual(testConfig, config);
        }

        [Test]
        public void Should_Load_Json_Config()
        {
            var testConfig = new TestConfiguration
            {
                BoolSetting = true,
                StringSetting = "TestValue",
                IntSetting = 1,
                TestConfigurationObject = new TestConfigurationObject
                {
                    Name = "TestName",
                    Value = "TestValue"
                }
            };
            var config = Config.GetFromJson<TestConfiguration>();
            Assert.AreEqual(testConfig, config);
        }
    }
}
