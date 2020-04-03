using NUnit.Framework;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace AnyConfig.Tests
{
    [TestFixture]
    [NonParallelizable] // don't run these tests in parallel, they may resolve different configs and overwrite each other
    public class ConfigurationManagerTests
    {
        private SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            // test projects using nunit/mstest framework will hide the entry assembly, so we must register it manually
            ConfigurationResolver.RegisterEntryAssembly(Assembly.GetExecutingAssembly());
        }

        [Test]
        public void Should_Load_ConfigurationManager()
        {
            _lock.Wait();
            try
            {
                ConfigurationManager.ResetCache();
                var connectionString = ConfigurationManager.ConnectionStrings["TestConnection"];
                var stringSetting = ConfigurationManager.AppSettings["StringSetting"];
                var boolSetting = ConfigurationManager.AppSettings["BoolSetting"];
                var intSetting = ConfigurationManager.AppSettings["IntSetting"];
                var section = ConfigurationManager.GetSection("nlog") as NLog.Config.XmlLoggingConfiguration;

                // connection strings
                Assert.AreEqual("TestConnection", connectionString.Name);
                Assert.AreEqual("host=localhost;", connectionString.ConnectionString);

                // app settings
                Assert.AreEqual("TestValue", stringSetting);
                Assert.AreEqual("true", boolSetting);
                Assert.AreEqual("1", intSetting);

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
            finally
            {
                _lock.Release();
            }
        }

        [Test]
        public void Should_Load_ConfigurationManagerFromJson()
        {
            _lock.Wait();
            try
            {
                ConfigurationManager.ResetCache();
                ConfigurationManager.ConfigurationSource = ConfigurationManagerSource.Json;
                ConfigurationManager.ConfigurationFilename = "legacyappsettings.json";
                var connectionString = ConfigurationManager.ConnectionStrings["TestConnection"];
                var stringSetting = ConfigurationManager.AppSettings["StringSetting"];
                var boolSetting = ConfigurationManager.AppSettings["BoolSetting"];
                var intSetting = ConfigurationManager.AppSettings["IntSetting"];

                // connection strings
                Assert.AreEqual("TestConnection", connectionString.Name);
                Assert.AreEqual("host=localhost;", connectionString.ConnectionString);

                // app settings
                Assert.AreEqual("TestValue", stringSetting);
                Assert.AreEqual("true", boolSetting);
                Assert.AreEqual("1", intSetting);
            }
            finally
            {
                _lock.Release();
            }
        }

        [Test]
        public void Should_Load_ConfigurationManagerFromXml()
        {
            _lock.Wait();
            try
            {
                ConfigurationManager.ResetCache();
                ConfigurationManager.ConfigurationSource = ConfigurationManagerSource.Xml;
                var connectionString = ConfigurationManager.ConnectionStrings["TestConnection"];
                var stringSetting = ConfigurationManager.AppSettings["StringSetting"];
                var boolSetting = ConfigurationManager.AppSettings["BoolSetting"];
                var intSetting = ConfigurationManager.AppSettings["IntSetting"];

                // connection strings
                Assert.AreEqual("TestConnection", connectionString.Name);
                Assert.AreEqual("host=localhost;", connectionString.ConnectionString);

                // app settings
                Assert.AreEqual("TestValue", stringSetting);
                Assert.AreEqual("true", boolSetting);
                Assert.AreEqual("1", intSetting);
            }
            finally
            {
                _lock.Release();
            }
        }

        [Test]
        public void Should_LoadRootAppSettings_ConfigurationManagerFromJson()
        {
            _lock.Wait();
            try
            {
                ConfigurationManager.ResetCache();
                ConfigurationManager.ConfigurationSource = ConfigurationManagerSource.Json;
                ConfigurationManager.ConfigurationFilename = "legacyrootappsettings.json";
                var connectionString = ConfigurationManager.AppSettings["TestConnection"];
                var stringSetting = ConfigurationManager.AppSettings["StringSetting"];
                var boolSetting = ConfigurationManager.AppSettings["BoolSetting"];
                var intSetting = ConfigurationManager.AppSettings["IntSetting"];

                // connection strings (this is just another app setting)
                Assert.AreEqual("host=localhost;", connectionString);

                // app settings
                Assert.AreEqual("TestValue", stringSetting);
                Assert.AreEqual("true", boolSetting);
                Assert.AreEqual("1", intSetting);
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
