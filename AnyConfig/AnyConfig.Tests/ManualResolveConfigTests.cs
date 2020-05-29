using AnyConfig.Collections;
using AnyConfig.Models;
using AnyConfig.Tests.Models;
using Microsoft.Extensions.Configuration;
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

        [Test]
        public void Should_Load_AuthorizationClientConfiguration()
        {
            var value = Config.GetFromJsonFile<AuthorizationClientConfiguration>("appsettings_full.json");
            Assert.NotNull(value);
            Assert.AreEqual("https://localhost:1111", value.Endpoint);
            Assert.AreEqual(false, value.UseClientSideCaching);
            Assert.AreEqual(true, value.AllowUntrustedCertificates);
            Assert.NotNull(value.CertificateConfiguration);
            Assert.AreEqual(LoadCertificateType.Embedded, value.CertificateConfiguration.CertificateType);
            Assert.AreEqual("Root", value.CertificateConfiguration.Store);
            Assert.AreEqual("CurrentUser", value.CertificateConfiguration.StoreLocation);
            Assert.AreEqual("", value.CertificateConfiguration.IssuerName);
        }

        [Test]
        public void Should_Load_IConfiguration_AndBind()
        {
            var config = Config.GetConfiguration("appsettings_full.json");
            Assert.NotNull(config);
            var webHostServiceConfigurationSection = config.GetSection("WebHostServiceConfiguration");
            Assert.NotNull(webHostServiceConfigurationSection);
            
            var webHostServiceConfiguration = webHostServiceConfigurationSection.Get<WebHostServiceConfiguration>();
            Assert.NotNull(webHostServiceConfiguration);
            Assert.AreEqual("Test service", webHostServiceConfiguration.Name);
            Assert.AreEqual(LoadCertificateType.Embedded, webHostServiceConfiguration.CertificateType);
            Assert.AreEqual(5433, webHostServiceConfiguration.Port);
            Assert.AreEqual(1024, webHostServiceConfiguration.MaxCacheItems);
            Assert.AreEqual("*", webHostServiceConfiguration.IP);
            Assert.AreEqual(3, webHostServiceConfiguration.AuthorizedIPs.Count);
            var securityConfigurationSection = config.GetSection("SecurityConfiguration");
            var securityConfiguration = securityConfigurationSection.Get<SecurityConfiguration>();
            Assert.NotNull(securityConfiguration);
        }

        [Test]
        public void Should_Load_IConfiguration_InvalidSectionReturnsNull()
        {
            var config = Config.GetConfiguration("appsettings_full.json");
            Assert.NotNull(config);
            var invalidSection = config.GetSection("InvalidConfiguration");
            Assert.NotNull(invalidSection);
            var invalidConfiguration = invalidSection.Get<InvalidConfiguration>();
            Assert.Null(invalidConfiguration);
        }

        [Test]
        public void Should_Load_IConfiguration_ReturnsReloadToken()
        {
            var config = Config.GetConfiguration("appsettings_full.json");
            Assert.NotNull(config);
            var token = config.GetReloadToken();
            Assert.NotNull(token);
        }

        [Test]
        public void Should_Load_IConfiguration_BackslashEncodingIsCorrect()
        {
            var config = Config.GetConfiguration("appsettings_full.json");
            Assert.NotNull(config);
            var securityConfigurationSection = config.GetSection("SecurityConfiguration");
            Assert.NotNull(securityConfigurationSection);
            var securityConfiguration = securityConfigurationSection.Get<SecurityConfiguration>();
            Assert.NotNull(securityConfiguration);
            Assert.AreEqual(@"FAKEPASSWORDSALT*{3-\?{", securityConfiguration.MasterUserPasswordSalt);
        }

        [Test]
        public void Should_Load_Setting_FromXmlFile()
        {
            var value = Config.GetFromXmlFile<int>("empty.config", "Invalid", 999);
            Assert.AreEqual(999, value);
        }

        [Test]
        public void Should_Load_Setting_FromJsonFile()
        {
            var value = Config.GetFromJsonFile<int>("empty.json", "Invalid", 999);
            Assert.AreEqual(999, value);
        }

        [Test]
        [Category("RequiresEncryption")]
        [Ignore("Temporarily disabled because of AppVeyor failures")]
        public void Should_Load_Protected_Dapi_Xml()
        {
            // the underlying xml loading uses ConfigurationManager, so reset it as other tests can affect this
            ConfigurationManager.ResetDefaults();
            ConfigurationManager.Reload();

            var config = Config.GetFromXmlFile<SectionCollection<ConnectionStringPair>>("DapiProtectedApp.config", "connectionStrings");
            var connection = config["TestConnection"].ConnectionStringSetting.ConnectionString;
            Assert.AreEqual("host=localhost;", connection);
        }

        [Test]
        [Category("RequiresEncryption")]
        [Ignore("Temporarily disabled because of AppVeyor failures")]
        public void Should_Load_Protected_Rsa_ConfigurationManager()
        {
            // the underlying xml loading uses ConfigurationManager, so reset it as other tests can affect this
            ConfigurationManager.ResetDefaults();
            ConfigurationManager.Reload();

            var config = Config.GetFromXmlFile<SectionCollection<ConnectionStringPair>>("RsaProtectedApp.config", "connectionStrings");
            var connection = config["TestConnection"].ConnectionStringSetting.ConnectionString;
            Assert.AreEqual("host=localhost;", connection);
        }
    }
}
