using NUnit.Framework;
using System.Linq;
using System.Reflection;

namespace AnyConfig.Tests
{
    [TestFixture]
    public class ConfigurationManagerTests
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            // test projects using nunit/mstest framework will hide the entry assembly, so we must register it manually
            ConfigurationResolver.RegisterEntryAssembly(Assembly.GetExecutingAssembly());
        }

        [Test]
        public void Should_Load_ConfigurationManager()
        {
            ConfigurationManager.ResetDefaults();
            ConfigurationManager.Reload();
            var connectionString = ConfigurationManager.ConnectionStrings["TestConnection"];
            var stringSetting = ConfigurationManager.AppSettings["StringSetting"];
            var boolSetting = ConfigurationManager.AppSettings["BoolSetting"];
            var intSetting = ConfigurationManager.AppSettings["IntSetting"];
            var customEnum = ConfigurationManager.AppSettings["CustomEnumSetting"];
            var customEnumNumeric = ConfigurationManager.AppSettings["CustomEnumNumericSetting"];
            var section = ConfigurationManager.GetSection("nlog") as NLog.Config.XmlLoggingConfiguration;

            // connection strings
            Assert.AreEqual("TestConnection", connectionString.Name);
            Assert.AreEqual("host=localhost;", connectionString.ConnectionString);

            // app settings
            Assert.AreEqual("TestValue", stringSetting);
            Assert.AreEqual("true", boolSetting);
            Assert.AreEqual("1", intSetting);
            Assert.AreEqual("Second", customEnum);
            Assert.AreEqual("2", customEnumNumeric);

#if NETFRAMEWORK
            // Custom section loading
            Assert.NotNull(section);
            Assert.AreEqual(1, section.AllTargets.Count);
            Assert.AreEqual(1, section.LoggingRules.Count);
            Assert.AreEqual("trace", section.AllTargets.First().Name);
            Assert.AreEqual("*", section.LoggingRules.First().LoggerNamePattern);
#else
            // nlog configuration section not available in .net core
#endif
        }

        [Test]
        public void Should_Load_ConfigurationManager_AppSettings_TypedGenerics()
        {
            ConfigurationManager.ResetDefaults();
            ConfigurationManager.Reload();
            var stringSetting = ConfigurationManager.AppSettings["StringSetting"];
            var boolSetting = ConfigurationManager.AppSettings["BoolSetting"].As<bool>();
            var intSetting = ConfigurationManager.AppSettings["IntSetting"].AsInt32();
            var customEnum = ConfigurationManager.AppSettings["CustomEnumSetting"].As<CustomEnum>();
            var customEnumNumeric = ConfigurationManager.AppSettings["CustomEnumNumericSetting"].As<CustomEnum>();

            Assert.AreEqual("TestValue", stringSetting);
            Assert.AreEqual(true, boolSetting);
            Assert.AreEqual(1, intSetting);
            Assert.AreEqual(CustomEnum.Second, customEnum);
            Assert.AreEqual(CustomEnum.Second, customEnumNumeric);
        }

        [Test]
        public void Should_Load_ConfigurationManagerFromJson()
        {
            ConfigurationManager.ConfigurationSource = ConfigurationManagerSource.Json;
            ConfigurationManager.ConfigurationFilename = "legacyappsettings.json";
            ConfigurationManager.Reload();
            var connectionString = ConfigurationManager.ConnectionStrings["TestConnection"];
            var stringSetting = ConfigurationManager.AppSettings["StringSetting"];
            var boolSetting = ConfigurationManager.AppSettings["BoolSetting"];
            var intSetting = ConfigurationManager.AppSettings["IntSetting"];
            var customEnum = ConfigurationManager.AppSettings["CustomEnumSetting"];
            var customEnumNumeric = ConfigurationManager.AppSettings["CustomEnumNumericSetting"];

            // connection strings
            Assert.AreEqual("TestConnection", connectionString.Name);
            Assert.AreEqual("host=localhost;", connectionString.ConnectionString);

            // app settings
            Assert.AreEqual("TestValue", stringSetting);
            Assert.AreEqual("true", boolSetting);
            Assert.AreEqual("1", intSetting);
            Assert.AreEqual("Second", customEnum);
            Assert.AreEqual("2", customEnumNumeric);
        }

        [Test]
        public void Should_Load_ConfigurationManagerFromXml()
        {
            ConfigurationManager.ResetDefaults();
            ConfigurationManager.ConfigurationSource = ConfigurationManagerSource.Xml;
            ConfigurationManager.Reload();
            var connectionString = ConfigurationManager.ConnectionStrings["TestConnection"];
            var stringSetting = ConfigurationManager.AppSettings["StringSetting"];
            var boolSetting = ConfigurationManager.AppSettings["BoolSetting"];
            var intSetting = ConfigurationManager.AppSettings["IntSetting"];
            var customEnum = ConfigurationManager.AppSettings["CustomEnumSetting"];
            var customEnumNumeric = ConfigurationManager.AppSettings["CustomEnumNumericSetting"];

            // connection strings
            Assert.AreEqual("TestConnection", connectionString.Name);
            Assert.AreEqual("host=localhost;", connectionString.ConnectionString);

            // app settings
            Assert.AreEqual("TestValue", stringSetting);
            Assert.AreEqual("true", boolSetting);
            Assert.AreEqual("1", intSetting);
            Assert.AreEqual("Second", customEnum);
            Assert.AreEqual("2", customEnumNumeric);
        }

        [Test]
        public void Should_LoadRootAppSettings_ConfigurationManagerFromJson()
        {
            ConfigurationManager.ResetDefaults();
            ConfigurationManager.ConfigurationSource = ConfigurationManagerSource.Json;
            ConfigurationManager.ConfigurationFilename = "legacyrootappsettings.json";
            ConfigurationManager.Reload();
            var connectionString = ConfigurationManager.AppSettings["TestConnection"];
            var stringSetting = ConfigurationManager.AppSettings["StringSetting"];
            var boolSetting = ConfigurationManager.AppSettings["BoolSetting"];
            var intSetting = ConfigurationManager.AppSettings["IntSetting"];
            var customEnum = ConfigurationManager.AppSettings["CustomEnumSetting"];
            var customEnumNumeric = ConfigurationManager.AppSettings["CustomEnumNumericSetting"];

            // app settings
            Assert.AreEqual("host=localhost;", connectionString);
            Assert.AreEqual("TestValue", stringSetting);
            Assert.AreEqual("true", boolSetting);
            Assert.AreEqual("1", intSetting);
            Assert.AreEqual("Second", customEnum);
            Assert.AreEqual("2", customEnumNumeric);
        }
    }
}
