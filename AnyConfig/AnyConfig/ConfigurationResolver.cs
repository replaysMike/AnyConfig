using AnyConfig.Exceptions;
using AnyConfig.Json;
using AnyConfig.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TypeSupport;
using TypeSupport.Extensions;
using AnyConfig.Xml;
using System.Linq;
using AnyConfig.Collections;

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
        private static Dictionary<string, Assembly> _registeredEntryAssemblies = new Dictionary<string, Assembly>();
        private LegacyConfigurationLoader _legacyConfigurationLoader;

        /// <summary>
        /// Register an entry assembly with the resolver
        /// </summary>
        /// <param name="entryAssembly"></param>
        public static void RegisterEntryAssembly(Assembly entryAssembly)
        {
            lock (_registeredEntryAssemblies)
            {
                var runtime = RuntimeEnvironment.DetectRuntime(entryAssembly);
                if (!_registeredEntryAssemblies.ContainsKey(runtime.DetectedRuntimeFrameworkDescription))
                    _registeredEntryAssemblies.Add(runtime.DetectedRuntimeFrameworkDescription, entryAssembly);
            }
        }

        /// <summary>
        /// The detected runtime framework
        /// </summary>
        public RuntimeInfo DetectedRuntime { get; private set; } = new RuntimeInfo();

        /// <summary>
        /// Create a configuration resolver, the entry assembly will be used for locating the configuration
        /// </summary>
        public ConfigurationResolver()
        {
            DetectedRuntime = RuntimeEnvironment.DetectRuntime();
            lock (_registeredEntryAssemblies)
            {
                if (_registeredEntryAssemblies.ContainsKey(DetectedRuntime.DetectedRuntimeFrameworkDescription))
                    _entryAssembly = _registeredEntryAssemblies[DetectedRuntime.DetectedRuntimeFrameworkDescription] ?? Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
                _legacyConfigurationLoader = new LegacyConfigurationLoader(_entryAssembly);
            }
        }

        /// <summary>
        /// Create a configuration resolver using a specified entry assembly for locating the configuration
        /// </summary>
        /// <param name="entryAssembly">Specify the assembly of the running application</param>
        public ConfigurationResolver(Assembly entryAssembly)
        {
            _entryAssembly = entryAssembly;
            DetectedRuntime = RuntimeEnvironment.DetectRuntime(_entryAssembly);
            _legacyConfigurationLoader = new LegacyConfigurationLoader(_entryAssembly);
        }

        /// <summary>
        /// Resolve a configuration for the current runtime platform
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ResolveConfiguration<T>()
        {
            // if on the .net core platform, resolve a configuration from appsettings.json
            switch (DetectedRuntime.DetectedRuntimeFramework)
            {
                case RuntimeFramework.DotNetCore:
                    return LoadDotNetCoreConfiguration<T>(default);
                case RuntimeFramework.DotNetFramework:
                    return LoadDotNetFrameworkConfiguration<T>(default);
            }

            throw new UnsupportedPlatformException();
        }

        /// <summary>
        /// Resolve a configuration for the current runtime platform with a specific setting name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settingName">Name of setting to load</param>
        /// <param name="defaultValue">Value to return if setting is not found</param>
        /// <param name="throwsException">True if exceptions should be thrown if data cannot be loaded</param>
        /// <returns></returns>
        public T ResolveConfiguration<T>(string settingName, T defaultValue, bool throwsException = false)
        {
            // if on the .net core platform, resolve a configuration from appsettings.json
            switch (DetectedRuntime.DetectedRuntimeFramework)
            {
                case RuntimeFramework.DotNetCore:
                    return LoadDotNetCoreConfiguration<T>(defaultValue, settingName, null, null, throwsException);
                case RuntimeFramework.DotNetFramework:
                    return LoadDotNetFrameworkConfiguration<T>(defaultValue, settingName, null, null, throwsException);
            }

            throw new UnsupportedPlatformException();
        }

        /// <summary>
        /// Resolve a configuration for the current runtime platform
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName">Name of section to load</param>
        /// <param name="defaultValue">Value to return if setting is not found</param>
        /// <param name="throwsException">True if exceptions should be thrown if data cannot be loaded</param>
        /// <returns></returns>
        public T ResolveConfigurationSection<T>(string sectionName, T defaultValue, bool throwsException = false)
        {
            var nameOfSection = sectionName ?? typeof(T).Name;
            // if on the .net core platform, resolve a configuration from appsettings.json
            switch (DetectedRuntime.DetectedRuntimeFramework)
            {
                case RuntimeFramework.DotNetCore:
                    return LoadDotNetCoreConfiguration<T>(defaultValue, null, null, nameOfSection, throwsException);
                case RuntimeFramework.DotNetFramework:
                    return LoadDotNetFrameworkConfiguration<T>(defaultValue, null, null, nameOfSection, throwsException);
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
            return LoadDotNetFrameworkConfiguration<T>(default, null, null, sectionName, true);
        }

        /// <summary>
        /// Get configuration as an object from Xml configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public T GetFromXmlFile<T>(string filename, string sectionName = null)
        {
            return LoadDotNetFrameworkConfiguration<T>(default, null, filename, sectionName, true);
        }

        /// <summary>
        /// Get configuration as an object from Json configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public T GetFromJson<T>(string sectionName = null)
        {
            return LoadDotNetCoreConfiguration<T>(default, null, null, sectionName, true);
        }

        /// <summary>
        /// Get configuration as an object from Json configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public T GetFromJsonFile<T>(string filename, string sectionName = null)
        {
            return LoadDotNetCoreConfiguration<T>(default, null, filename, sectionName, true);
        }

        /// <summary>
        /// Get an IConfiguration
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public IConfigurationRoot GetConfiguration(string filename = null, string sectionName = null)
        {
            var configFile = filename ?? DotNetCoreSettingsFilename;
            if (!string.IsNullOrEmpty(configFile))
            {
                configFile = Path.GetFullPath(configFile);
                if (!File.Exists(configFile))
                {
                    if (_entryAssembly != null)
                        configFile = Path.Combine(Path.GetDirectoryName(_entryAssembly.Location), filename ?? DotNetCoreSettingsFilename);
                    if (!File.Exists(configFile))
                        throw new FileNotFoundException($"Could not find configuration file '{configFile}'");
                }
            }
            // for .net core configs such as appsettings.json we can serialize from json directly
            return ConfigProvider.GetConfiguration(Path.GetFileName(configFile), Path.GetDirectoryName(configFile));
        }

        private T LoadDotNetCoreConfiguration<T>(T defaultValue, string settingName = null, string filename = null, string sectionName = null, bool throwsException = false)
        {
            var configuration = GetConfiguration(filename, sectionName);
            if (configuration == null)
                throw new ConfigurationMissingException($"Could not load configuration.");

            if (!string.IsNullOrEmpty(settingName))
            {
                if(configuration.Providers.First().TryGet(settingName, out var value))
                    return ((StringValue)value).As<T>();
                return defaultValue;
            }
            else
            {
                var nameOfSection = sectionName ?? typeof(T).Name;
                var configSection = configuration.GetSection(nameOfSection);
                if (configSection == null)
                {
                    if (throwsException)
                        throw new ConfigurationMissingException($"Unable to resolve configuration section of type '{nameOfSection}'. Please ensure your application '{GetAssemblyName()}' is configured correctly for the '{DetectedRuntime.DetectedRuntimeFramework}' framework.");
                    return defaultValue;
                }

                var value = JsonSerializer.Deserialize<T>(configSection.Value);
                return value;
            }
        }

        private T LoadDotNetFrameworkConfiguration<T>(T defaultValue, string settingName = null, string filename = null, string sectionName = null, bool throwsException = false)
        {
            // for .net framework configs such as app.config and web.config we need to serialize individual values

            if (!string.IsNullOrEmpty(settingName))
            {
                object value = null;
                value = ConfigProvider.Get(typeof(T), settingName, ConfigProvider.Empty, ConfigSource.WebConfig);
                if (value == ConfigProvider.Empty)
                {
                    value = ConfigProvider.Get(typeof(T), settingName, ConfigProvider.Empty, ConfigSource.ApplicationConfig);
                    if (value == ConfigProvider.Empty)
                        return defaultValue;
                }
                return (T)value;
            }
            else
            {
                // recursively map the configuration based on legacy property names
                var nameOfSection = sectionName ?? typeof(T).Name;
                var extendedType = typeof(T).GetExtendedType();
                var propertyLegacyAttribute = extendedType.GetAttribute<LegacyConfigurationNameAttribute>();
                if (propertyLegacyAttribute != null)
                    return (T)MapTypeProperties(extendedType, defaultValue, propertyLegacyAttribute.PrependChildrenName, throwsException);

                return (T)MapTypeProperties(extendedType, defaultValue, null, throwsException);
            }
        }

        /// <summary>
        /// Serializes a configuration class from .Net Web/App configs using LegacyConfigurationName Attributes (or property names)
        /// Recursive, supports child configuration class definitions
        /// </summary>
        /// <param name="configurationClassType"></param>
        /// <param name="prependPropertyName">Use this value to prepend a configuration setting name in the config</param>
        /// <returns></returns>
        private object MapTypeProperties(ExtendedType configurationClassType, object defaultValue, string prependPropertyName = null, bool throwsException = false)
        {
            var objectFactory = new ObjectFactory();
            var returnObject = objectFactory.CreateEmptyObject(configurationClassType);

            var legacyCustomSection = ConfigurationManager.GetSection(configurationClassType.Name);
            if (legacyCustomSection != null)
            {
                if (legacyCustomSection.GetType() == typeof(ConfigSectionPair))
                {
                    var configSectionPair = legacyCustomSection as ConfigSectionPair;
                    if (configSectionPair.TypeValue == typeof(RequiresXmlSerialization))
                    {
                        returnObject = XmlSerializer.Deserialize(configSectionPair.Configuration.ToString(), configurationClassType.Type);
                        return returnObject;
                    }
                }
            }

            var anyPropertiesMapped = false;
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
                        value = MapTypeProperties(propertyType, objectFactory.CreateEmptyObject(propertyType), prependPropertyName + propertyLegacyAttribute.PrependChildrenName, throwsException);
                    }
                }
                if (value == null)
                {
                    value = ConfigProvider.Get(propertyType.Type, propertyName, ConfigProvider.Empty, ConfigSource.WebConfig);
                    if (value == ConfigProvider.Empty)
                    {
                        value = ConfigProvider.Get(propertyType.Type, propertyName, ConfigProvider.Empty, ConfigSource.ApplicationConfig);
                        if (isRequired && value == ConfigProvider.Empty)
                            throw new ConfigurationMissingException(propertyName, property.Type);
                    }
                }
                if (value != ConfigProvider.Empty)
                {
                    property.PropertyInfo.SetValue(returnObject, value, null);
                    anyPropertiesMapped = true;
                }
            }

            if (!anyPropertiesMapped)
            {
                if (throwsException)
                    throw new ConfigurationMissingException($"Unable to resolve configuration of type '{configurationClassType.Name}'. Please ensure your application '{GetAssemblyName()}' is configured correctly for the '{DetectedRuntime.DetectedRuntimeFramework}' framework.");
                else
                    returnObject = defaultValue;
            }
            return returnObject;
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
