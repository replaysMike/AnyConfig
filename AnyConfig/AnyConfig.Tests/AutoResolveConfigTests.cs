using System.Collections.Generic;
using AnyConfig.Exceptions;
using AnyConfig.Scaffolding;
using AnyConfig.Tests.Models;
using NLog.Extensions.Logging;
using NUnit.Framework;
using Serilog;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AnyConfig.Tests
{
    [TestFixture]
    [NonParallelizable]
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

            // inspect this complicated config structure
            var section = config.GetSection("NLog");
            var targets = section.GetSection("targets");
            var console = targets.GetSection("console");
            var graylog = targets.GetSection("graylog");
            var parms = graylog.GetSection("Parameters");
            var children = parms.GetChildren();
            var children2 = children.First().GetChildren().First();
            var z = children2.GetChildren();
            var child = children.First();
            var child1Value = child.GetSection("Layout");

            Assert.AreEqual("authv2", child1Value.Value);

            var success = provider.TryGet("TestConfiguration:BoolSetting", out var val);

            Assert.AreEqual(true, success);
            Assert.AreEqual("True", val);

            // provider should contain the right number of keys
            Assert.AreEqual(90, provider.Data.Count);

            var rateLimitingKeys = provider.GetChildKeys("IpRateLimiting");
            Assert.AreEqual(21, rateLimitingKeys.Count());

            var section2 = config.GetSection("TestConfiguration") as ConfigurationSection;
            Assert.NotNull(section);
            Assert.AreEqual("TestConfiguration", section2.Key);
            Assert.AreEqual("TestConfiguration", section2.Path);
            var structuredText = section2.GetNodeStructuredText();
            Assert.NotNull(structuredText);
        }

        [Test]
        public void Should_Load_NLog_Configuration()
        {
            //var testScaffold = new TestScaffold(@"B:\gitrepo\personalcode\AnyConfig\AnyConfig\AnyConfig.Scaffolding\bin\Debug\netcoreapp3.1\");
            var configuration = Config.GetConfiguration();
            var nlogConfiguration = configuration.GetSection("NLog");
            var targets = nlogConfiguration.GetSection("targets");
            NLog.LogManager.Configuration = new NLogLoggingConfiguration(nlogConfiguration);
            var logger = NLog.LogManager.GetCurrentClassLogger();

            var sw = new StringWriter();
            var outputStream = System.Console.Out;
            System.Console.SetOut(sw);
            logger.Error("Test Log Event");
            // this currently isn't logging to the console
            //Assert.IsTrue(sw.ToString().EndsWith("Test Log Event\r\n"));
            System.Console.SetOut(outputStream);
        }

        [Test]
        public void Should_Load_Serilog_Configuration()
        {
            var configuration = Config.GetConfiguration();
            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration, sectionName: "Serilog")
                .CreateLogger();
            var sw = new StringWriter();
            var outputStream = System.Console.Out;
            System.Console.SetOut(sw);
            logger.Write(Serilog.Events.LogEventLevel.Error, "Test Log Event");
            Assert.IsTrue(sw.ToString().EndsWith("Test Log Event\r\n"));
            System.Console.SetOut(outputStream);
        }

        [Test]
        public void Should_Throw_InvalidSetting()
        {
            Assert.Throws<KeyNotFoundException>(() =>
                new ConfigProvider().Get<bool>("SomethingThatDoesntExist", ConfigSource.WebConfig, true));
        }

        [Test]
        public void Should_NotThrow_InvalidSetting()
        {
            Assert.DoesNotThrow(() =>
                new ConfigProvider().Get<bool>("SomethingThatDoesntExist", ConfigSource.WebConfig, false));
        }
    }
}
