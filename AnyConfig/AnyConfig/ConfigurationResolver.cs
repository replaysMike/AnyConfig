using AnyConfig.Exceptions;
using AnyConfig.Json;
using AnyConfig.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using TypeSupport;
using TypeSupport.Extensions;

namespace AnyConfig
{
    /// <summary>
    /// Multi-framework configuration resolver
    /// Supports .Net Framework and .Net Core configurations
    /// </summary>
    public class ConfigurationResolver : IConfigurationResolver
    {
        private const string DotNetCoreSettingsFilename = "appsettings.json";
        private readonly Assembly _entryAssembly;
        private static Assembly _registeredEntryAssembly;
        private LegacyConfigurationLoader _legacyConfigurationLoader;

        /// <summary>
        /// Register an entry assembly with the resolver
        /// </summary>
        /// <param name="entryAssembly"></param>
        public static void RegisterEntryAssembly(Assembly entryAssembly)
        {
            _registeredEntryAssembly = entryAssembly;
        }

        /// <summary>
        /// The detected runtime framework
        /// </summary>
        public RuntimeFramework DetectedRuntimeFramework { get; private set; } = RuntimeFramework.DotNetFramework;

        /// <summary>
        /// The detected OS platform
        /// </summary>
        public string DetectedRuntimePlatform { get; private set; }

        /// <summary>
        /// The detected runtime framework as per OS
        /// </summary>
        public string DetectedRuntimeFrameworkDescription { get; private set; }

        /// <summary>
        /// Create a configuration resolver, the entry assembly will be used for locating the configuration
        /// </summary>
        public ConfigurationResolver() : this(_registeredEntryAssembly ?? Assembly.GetEntryAssembly())
        {
        }

        /// <summary>
        /// Create a configuration resolver using a specified entry assembly for locating the configuration
        /// </summary>
        /// <param name="entryAssembly">Specify the assembly of the running application</param>
        public ConfigurationResolver(Assembly entryAssembly)
        {
            _entryAssembly = entryAssembly;
            _legacyConfigurationLoader = new LegacyConfigurationLoader(entryAssembly);
            DetectRuntime();
        }

        /// <summary>
        /// Resolve a configuration for the current runtime platform
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ResolveConfiguration<T>()
        {
            // if on the .net core platform, resolve a configuration from appsettings.json
            switch (DetectedRuntimeFramework)
            {
                case RuntimeFramework.DotNetCore:
                    return LoadDotNetCoreConfiguration<T>();
                case RuntimeFramework.DotNetFramework:
                    return LoadDotNetFrameworkConfiguration<T>();
            }

            throw new UnsupportedPlatformException();
        }

        /// <summary>
        /// Resolve a configuration for the current runtime platform
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ResolveConfiguration<T>(string sectionName)
        {
            var nameOfSection = sectionName ?? typeof(T).Name;
            // if on the .net core platform, resolve a configuration from appsettings.json
            switch (DetectedRuntimeFramework)
            {
                case RuntimeFramework.DotNetCore:
                    return LoadDotNetCoreConfiguration<T>(null, nameOfSection);
                case RuntimeFramework.DotNetFramework:
                    return LoadDotNetFrameworkConfiguration<T>(null, nameOfSection);
            }

            throw new UnsupportedPlatformException();
        }

        /// <summary>
        /// Resolve a legacy configuration
        /// </summary>
        /// <returns></returns>
        public LegacyConfiguration ResolveLegacyConfigurationFromXml()
        {
            return _legacyConfigurationLoader.LoadDotNetFrameworkLegacyConfiguration();
        }

        /// <summary>
        /// Resolve a legacy configuration
        /// </summary>
        /// <param name="filename">Filename of xml configuration to load</param>
        /// <returns></returns>
        public LegacyConfiguration ResolveLegacyConfigurationFromXml(string filename)
        {
            return _legacyConfigurationLoader.LoadDotNetFrameworkLegacyConfiguration(filename);
        }

        /// <summary>
        /// Resolve a legacy configuration
        /// </summary>
        /// <returns></returns>
        public LegacyConfiguration ResolveLegacyConfigurationFromJson()
        {
            return _legacyConfigurationLoader.LoadDotNetCoreLegacyConfiguration();
        }

        /// <summary>
        /// Resolve a configuration for the current runtime platform
        /// </summary>
        /// <returns></returns>
        public LegacyConfiguration ResolveLegacyConfigurationFromJson(string filename)
        {
            return _legacyConfigurationLoader.LoadDotNetCoreLegacyConfiguration(filename);
        }

        /// <summary>
        /// Get configuration as an object from Xml configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public T GetFromXml<T>(string sectionName = null)
        {
            return LoadDotNetFrameworkConfiguration<T>(null, sectionName);
        }

        /// <summary>
        /// Get configuration as an object from Xml configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public T GetFromXmlFile<T>(string filename, string sectionName = null)
        {
            return LoadDotNetFrameworkConfiguration<T>(filename, sectionName);
        }

        /// <summary>
        /// Get configuration as an object from Json configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public T GetFromJson<T>(string sectionName = null)
        {
            return LoadDotNetCoreConfiguration<T>(null, sectionName);
        }

        /// <summary>
        /// Get configuration as an object from Json configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public T GetFromJsonFile<T>(string filename, string sectionName = null)
        {
            return LoadDotNetCoreConfiguration<T>(filename, sectionName);
        }

        /// <summary>
        /// Get an IConfiguration
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public IConfigurationRoot GetConfiguration(string filename = null, string sectionName = null)
        {
            if (!string.IsNullOrEmpty(filename))
            {
                filename = Path.GetFullPath(filename);
                if (!File.Exists(filename))
                    throw new FileNotFoundException($"Could not find configuration file '{filename}'");
            }
            var configFile = filename ?? DotNetCoreSettingsFilename;
            // for .net core configs such as appsettings.json we can serialize from json directly
            return ConfigProvider.GetConfiguration(configFile, _entryAssembly.Location);
        }

        private T LoadDotNetCoreConfiguration<T>(string filename = null, string sectionName = null)
        {
            var configuration = GetConfiguration(filename, sectionName);
            if (configuration == null)
                throw new InvalidOperationException($"Could not load configuration.");

            var nameOfSection = sectionName ?? typeof(T).Name;
            var configSection = configuration.GetSection(nameOfSection);
            if (configSection == null)
                throw new ConfigurationMissingException($"Unable to resolve configuration section of type '{nameOfSection}'. Please ensure your application '{GetAssemblyName()}' is configured correctly for the '{DetectedRuntimeFramework}' framework.");

            var value = JsonSerializer.Deserialize<T>(configSection.Value);

            return value;
        }

        private T LoadDotNetFrameworkConfiguration<T>(string filename = null, string sectionName = null)
        {
            // for .net framework configs such as app.config and web.config we need to serialize individual values
            // recursively map the configuration based on legacy property names
            var nameOfSection = sectionName ?? typeof(T).Name;
            var extendedType = typeof(T).GetExtendedType();
            var propertyLegacyAttribute = extendedType.GetAttribute<LegacyConfigurationNameAttribute>();
            if (propertyLegacyAttribute != null)
                return (T)MapTypeProperties(extendedType, propertyLegacyAttribute.PrependChildrenName);
            return (T)MapTypeProperties(extendedType);
        }

        /// <summary>
        /// Serializes a configuration class from .Net Web/App configs using LegacyConfigurationName Attributes (or property names)
        /// Recursive, supports child configuration class definitions
        /// </summary>
        /// <param name="configurationClassType"></param>
        /// <param name="prependPropertyName">Use this value to prepend a configuration setting name in the config</param>
        /// <returns></returns>
        private object MapTypeProperties(ExtendedType configurationClassType, string prependPropertyName = null)
        {
            var returnObject = new ObjectFactory().CreateEmptyObject(configurationClassType);

            foreach (var property in configurationClassType.Properties)
            {
                var isRequired = false;
                var propertyName = property.Name;
                var propertyType = property.Type.GetExtendedType();
                if (!string.IsNullOrEmpty(prependPropertyName))
                    propertyName = prependPropertyName + propertyName;
                object value = null;
                // check if the property has a custom attribute specifying the a legacy .net framework setting name
                var propertyLegacyAttribute = property.GetAttribute<LegacyConfigurationNameAttribute>();
                if (propertyLegacyAttribute != null)
                {
                    if (!string.IsNullOrEmpty(propertyLegacyAttribute.SettingName))
                        propertyName = prependPropertyName + propertyLegacyAttribute.SettingName;
                    isRequired = propertyLegacyAttribute.IsRequired;
                    if (propertyType.IsReferenceType && propertyLegacyAttribute.ChildrenMapped)
                    {
                        // map all of this objects properties and attributes
                        value = MapTypeProperties(propertyType, prependPropertyName + propertyLegacyAttribute.PrependChildrenName);
                    }
                }
                if (value == null)
                {
                    value = ConfigProvider.Get(propertyType.Type, propertyName, ConfigProvider.Empty, ConfigSource.WebConfig);
                    if (value == ConfigProvider.Empty)
                        value = ConfigProvider.Get(propertyType.Type, propertyName, ConfigProvider.Empty, ConfigSource.ApplicationConfig);
                    if (isRequired && value == ConfigProvider.Empty)
                        throw new ConfigurationMissingException(propertyName, property.Type);
                }
                if (value != ConfigProvider.Empty)
                    property.PropertyInfo.SetValue(returnObject, value, null);
            }
            return returnObject;
        }

        private void DetectRuntime()
        {
            string framework = null;
            DetectedRuntimePlatform = RuntimeInformation.OSDescription;
            DetectedRuntimeFrameworkDescription = RuntimeInformation.FrameworkDescription;

#if NETFRAMEWORK
            if (_entryAssembly == null)
            {
                var appDomain = AppDomain.CurrentDomain.SetupInformation;
                var configFile = appDomain.ConfigurationFile;
                DetectedRuntimeFramework = RuntimeFramework.DotNetFramework;
                framework = appDomain.TargetFrameworkName;
            }
#endif
            // if we have a known entry assembly, use that as it may be more reliable
            if (framework == null)
                framework = _entryAssembly?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;
            if (framework?.Contains(".NETCoreApp") == true) // ".NETCoreApp,Version=v2.1"
                DetectedRuntimeFramework = RuntimeFramework.DotNetCore;
            if (framework?.Contains(".NETFramework") == true) // ".NETFramework,Version=v4.8"
                DetectedRuntimeFramework = RuntimeFramework.DotNetFramework;
        }

        private string GetAssemblyName()
        {
#if NETFRAMEWORK
            if (_entryAssembly == null)
            {
                return AppDomain.CurrentDomain.SetupInformation.ApplicationName;
            }
#endif
            return _entryAssembly?.GetName().Name ?? string.Empty;
        }
    }
}
