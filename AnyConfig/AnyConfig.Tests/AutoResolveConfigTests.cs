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
            var config = Config.Get<TestConfiguration>(nameof(TestConfiguration));
            Assert.AreEqual(_testConfig, config);
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
        public void Should_Load_IConfiguration()
        {
            // var t = new TestScaffold(Assembly.GetExecutingAssembly().Location);
            var config = Config.GetConfiguration();
            Assert.NotNull(config);
            Assert.AreEqual(1, config.Providers.Count());

            var provider = config.Providers.First();
            var success = provider.TryGet("TestConfiguration:BoolSetting", out var val);

            Assert.AreEqual(true, success);
            Assert.AreEqual("True", val);
            
            var section = config.GetSection("TestConfiguration");
            Assert.NotNull(section);
            Assert.AreEqual("TestConfiguration", section.Key);
            Assert.AreEqual("/TestConfiguration", section.Path);
            Assert.NotNull(section.Value);
        }
    }
}
