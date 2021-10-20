using AnyConfig.Scaffolding;
using NUnit.Framework;
using System.Linq;
using System.Reflection;

namespace AnyConfig.Tests
{
    [TestFixture]
    [NonParallelizable]
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
            var section = ConfigurationManager.GetSection("nlog");
            var sectionTyped = section.As<NLog.Config.XmlLoggingConfiguration>();

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
            Assert.NotNull(sectionTyped);
            Assert.AreEqual(typeof(NLog.Config.XmlLoggingConfiguration), sectionTyped.GetType());
            Assert.AreEqual(1, sectionTyped.AllTargets.Count);
            Assert.AreEqual(1, sectionTyped.LoggingRules.Count);
            Assert.AreEqual("trace", sectionTyped.AllTargets.First().Name);
            Assert.AreEqual("*", sectionTyped.LoggingRules.First().LoggerNamePattern);

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

        [Test]
        public void Should_Load_ConfigurationManagerFromXml_HierarchicalValue()
        {
            ConfigurationManager.ResetDefaults();
            ConfigurationManager.ConfigurationSource = ConfigurationManagerSource.Xml;
            ConfigurationManager.Reload();
            var rootName = ConfigurationManager.Get<string>("appSettings:StringSetting");
            Assert.AreEqual("TestValue", rootName);
        }

        [Test]
        public void Should_Load_ConfigurationManagerCustomSectionFromJson()
        {
            ConfigurationManager.ConfigurationFilename = "appsettings.json";
            ConfigurationManager.Reload();
            var testConfiguration = ConfigurationManager.GetSection("TestConfiguration").As<TestConfiguration>();

            Assert.NotNull(testConfiguration);
            Assert.AreEqual(true, testConfiguration.BoolSetting);
            Assert.AreEqual("TestValue", testConfiguration.StringSetting);
            Assert.AreEqual(1, testConfiguration.IntSetting);
            Assert.AreEqual(CustomEnum.Second, testConfiguration.CustomEnumSetting);
            Assert.AreEqual(CustomEnum.Second, testConfiguration.CustomEnumNumericSetting);
            Assert.NotNull(testConfiguration.TestConfigurationObject);
            Assert.AreEqual("TestName", testConfiguration.TestConfigurationObject.Name);
            Assert.AreEqual("TestValue", testConfiguration.TestConfigurationObject.Value);
        }

        [Test]
        [Category("RequiresEncryption")]
        [Ignore("Temporarily disabled because of AppVeyor failures")]
        public void Should_Load_Protected_Dapi_ConfigurationManager()
        {
            ConfigurationManager.ResetDefaults();
            ConfigurationManager.ConfigurationFilename = "DapiProtectedApp.config";
            ConfigurationManager.ConfigurationSource = ConfigurationManagerSource.Xml;
            ConfigurationManager.Reload();
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var connectionStrings = config.GetSection("connectionStrings");
            Assert.IsTrue(connectionStrings.SectionInformation.IsProtected);
            Assert.IsTrue(connectionStrings.SectionInformation.ProtectionProvider.Name.Equals("DataProtectionConfigurationProvider", System.StringComparison.InvariantCultureIgnoreCase));
            Assert.AreEqual("host=localhost;", ConfigurationManager.ConnectionStrings["TestConnection"].ConnectionString);
        }

        [Test]
        [Category("RequiresEncryption")]
        [Ignore("Temporarily disabled because of AppVeyor failures")]
        public void Should_Load_Protected_Rsa_ConfigurationManager()
        {
            ConfigurationManager.ResetDefaults();
            ConfigurationManager.ConfigurationFilename = "RsaProtectedApp.config";
            ConfigurationManager.ConfigurationSource = ConfigurationManagerSource.Xml;
            ConfigurationManager.Reload();
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var connectionStrings = config.GetSection("connectionStrings");
            Assert.IsTrue(connectionStrings.SectionInformation.IsProtected);
            Assert.IsTrue(connectionStrings.SectionInformation.ProtectionProvider.Name.Equals("RsaProtectedConfigurationProvider", System.StringComparison.InvariantCultureIgnoreCase));
            Assert.AreEqual("host=localhost;", ConfigurationManager.ConnectionStrings["TestConnection"].ConnectionString);
        }
    }
}
