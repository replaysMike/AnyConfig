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

        [Test]
        public void Should_Load_ConfigurationManagerFromXml_AnyConfigGroups()
        {
            ConfigurationManager.ResetDefaults();
            ConfigurationManager.ConfigurationSource = ConfigurationManagerSource.Xml;
            ConfigurationManager.Reload();
            var rootName = ConfigurationManager.Get<string>("rootName");
            var settingName = ConfigurationManager.Get<string>("settings", "name");
            var settingIsEnabled = ConfigurationManager.Get<bool>("settings", "isEnabled");
            var configName = ConfigurationManager.Get<string>("config", "name");
            var configIsEnabled = ConfigurationManager.Get<bool>("config", "isEnabled");

            // there are 2 groups, and a single root element (which appears as a group)
            Assert.AreEqual(3, ConfigurationManager.AnySettings.Count);

            // root settings
            Assert.AreEqual("rootValue1", rootName);
            Assert.AreEqual("rootValue1", ConfigurationManager.AnySettings["rootName"]);

            // settings section
            Assert.AreEqual("value1", ConfigurationManager.AnySettings["settings"]["name"].As<string>());
            Assert.AreEqual(true, ConfigurationManager.AnySettings["settings"]["isEnabled"].As<bool>());
            Assert.AreEqual("value1", settingName);
            Assert.AreEqual(true, settingIsEnabled);

            // config section
            Assert.AreEqual("othervalue1", ConfigurationManager.AnySettings["config"]["name"].As<string>());
            Assert.AreEqual(false, ConfigurationManager.AnySettings["config"]["isEnabled"].As<bool>());
            Assert.AreEqual("othervalue1", configName);
            Assert.AreEqual(false, configIsEnabled);
        }
    }
}
