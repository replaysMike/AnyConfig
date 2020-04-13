using NUnit.Framework;
using System;
using System.Reflection;

namespace AnyConfig.Tests
{
    [TestFixture]
    public class ManualResolveConfigTests
    {
        TestConfiguration _testConfig;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            // test projects using nunit/mstest framework will hide the entry assembly, so we must register it manually
            ConfigurationResolver.RegisterEntryAssembly(Assembly.GetExecutingAssembly());
            _testConfig = new TestConfiguration
            {
                BoolSetting = true,
                StringSetting = "TestValue",
                IntSetting = 1,
                CustomEnumSetting = CustomEnum.Second,
                CustomEnumNumericSetting = CustomEnum.Second,
                TestConfigurationObject = new TestConfigurationObject
                {
                    Name = "TestName",
                    Value = "TestValue"
                }
            };
        }

        [Test]
        public void Should_Load_Xml_Config()
        {
            // the underlying xml loading uses ConfigurationManager, so reset it as other tests can affect this
            ConfigurationManager.ResetDefaults();
            ConfigurationManager.Reload();

            var config = Config.GetFromXml<TestConfiguration>();
            Assert.AreEqual(_testConfig, config);
        }

        [Test]
        public void Should_Load_EntityFramework_Xml_Config()
        {
            // the underlying xml loading uses ConfigurationManager, so reset it as other tests can affect this
            ConfigurationManager.ResetDefaults();
            ConfigurationManager.Reload();

            var type = Type.GetType("System.Data.Entity.Internal.ConfigFile.EntityFrameworkSection, EntityFramework, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            var config = Config.GetFromXml("entityFramework", type);
            Assert.NotNull(config);
            Assert.AreEqual(type, config.GetType());
        }

        [Test]
        public void Should_Load_Xml_SimpleConfig()
        {
            // the underlying xml loading uses ConfigurationManager, so reset it as other tests can affect this
            ConfigurationManager.ResetDefaults();
            ConfigurationManager.Reload();

            var simpleConfig = new SimpleConfiguration
            {
                BoolSetting = true,
                StringSetting = "TestValue",
                IntSetting = 1,
                Child = new ChildSimpleConfiguration
                {
                    Name = "TestName",
                    Value = "TestValue"
                }
            };

            var config = Config.GetFromXml<SimpleConfiguration>();
            Assert.AreEqual(simpleConfig, config);
        }

        [Test]
        public void Should_Load_Json_Config()
        {
            var config = Config.GetFromJson<TestConfiguration>();
            Assert.AreEqual(_testConfig, config);
        }
    }
}
