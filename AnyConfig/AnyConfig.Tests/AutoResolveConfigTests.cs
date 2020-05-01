using AnyConfig.Exceptions;
using AnyConfig.Scaffolding;
using NUnit.Framework;
using System.Linq;
using System.Reflection;

namespace AnyConfig.Tests
{
    [TestFixture]
    public class AutoResolveConfigTests
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
        public void Should_Load_Config()
        {
            var config = Config.Get<TestConfiguration>();
            Assert.AreEqual(_testConfig, config);
        }

        [Test]
        public void Should_Load_ConfigSection()
        {
            var config = Config.GetSection<TestConfiguration>(nameof(TestConfiguration));
            Assert.AreEqual(_testConfig, config);
        }

        [Test]
        public void Should_Load_ConfigSection_DefaultValue()
        {
            var defaultValue = new UnknownConfiguration("default");
            var config = Config.GetSection<UnknownConfiguration>(nameof(UnknownConfiguration), defaultValue);
            Assert.AreEqual(defaultValue, config);
        }

        [Test]
        public void Should_Load_ConfigSection_NoDefaultValueShouldThrow()
        {
            Assert.Throws<ConfigurationMissingException>(() => Config.GetSection<UnknownConfiguration>(nameof(UnknownConfiguration)));
        }

        [Test]
        public void Should_Load_SpecifiedConfig()
        {
            var config = Config.GetFromJsonFile<TestConfiguration>("appsettings.json");
            Assert.AreEqual(_testConfig, config);
        }

        [Test]
        public void Should_Load_SpecifiedConfigSection()
        {
            var config = Config.GetFromJsonFile<TestConfiguration>("appsettings.json", nameof(TestConfiguration));
            Assert.AreEqual(_testConfig, config);
        }

        [Test]
        public void Should_Load_Config_Value()
        {
            var value = Config.Get<string>("RootStringValue");
            Assert.AreEqual("TestRootValue", value);
        }

        [Test]
        public void Should_Load_Config_NonRootValue()
        {
            var value = Config.Get<string>("StringSetting");
            Assert.AreEqual("TestValue", value);
        }

        [Test]
        public void Should_Load_Config_DefaultValue()
        {
            var value = Config.Get<int>("InvalidSetting", 999);
            Assert.AreEqual(999, value);
        }

        [Test]
        public void Should_Load_IConfiguration()
        {
            // var t = new TestScaffold(Assembly.GetExecutingAssembly().Location);
            var config = Config.GetConfiguration();
            Assert.NotNull(config);
            Assert.AreEqual(1, config.Providers.Count());

            var provider = config.Providers.First() as JsonConfigurationProvider;
            var success = provider.TryGet("TestConfiguration:BoolSetting", out var val);

            Assert.AreEqual(true, success);
            Assert.AreEqual("True", val);

            // provider should contain the right number of keys
            Assert.AreEqual(31, provider.Data.Count);

            var rateLimitingKeys = provider.GetChildKeys("IpRateLimiting");
            Assert.AreEqual(21, rateLimitingKeys.Count());

            var section = config.GetSection("TestConfiguration") as ConfigurationSection;
            Assert.NotNull(section);
            Assert.AreEqual("TestConfiguration", section.Key);
            Assert.AreEqual("TestConfiguration", section.Path);
            Assert.NotNull(section.RawText);
        }
    }
}
