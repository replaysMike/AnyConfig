using AnyConfig.Collections;
using AnyConfig.Exceptions;
using AnyConfig.Json;
using AnyConfig.Models;
using AnyConfig.Xml;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
                    return LoadJsonConfiguration<T>(default);
                case RuntimeFramework.DotNetFramework:
                default:
                    return LoadXmlConfiguration<T>(default);
            }

            throw new UnsupportedPlatformException();
        }

        /// <summary>
        /// Resolve a configuration for the current runtime platform with a specific setting name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settingName">Name of setting to load</param>
        /// <returns></returns>
        public T ResolveConfiguration<T>(string settingName) => ResolveConfiguration<T>(settingName, default, false);

        /// <summary>
        /// Resolve a configuration for the current runtime platform with a specific setting name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settingName">Name of setting to load</param>
        /// <param name="defaultValue">Value to return if setting is not found</param>
        /// <returns></returns>
        public T ResolveConfiguration<T>(string settingName, T defaultValue) => ResolveConfiguration<T>(settingName, defaultValue, false);

        /// <summary>
        /// Resolve a configuration for the current runtime platform with a specific setting name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settingName">Name of setting to load</param>
        /// <param name="defaultValue">Value to return if setting is not found</param>
        /// <param name="throwsException">True if exceptions should be thrown if data cannot be loaded</param>
        /// <returns></returns>
        public T ResolveConfiguration<T>(string settingName, T defaultValue, bool throwsException)
        {
            // if on the .net core platform, resolve a configuration from appsettings.json
            switch (DetectedRuntime.DetectedRuntimeFramework)
            {
                case RuntimeFramework.DotNetCore:
                    return LoadJsonConfiguration<T>(defaultValue, settingName, null, null, throwsException);
                case RuntimeFramework.DotNetFramework:
                default:
                    return LoadXmlConfiguration<T>(defaultValue, settingName, null, null, throwsException);
            }

            throw new UnsupportedPlatformException();
        }

        /// <summary>
        /// Resolve a configuration for the current runtime platform with a specific setting name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settingName">Name of setting to load</param>
        /// <param name="type">The type to return</param>
        /// <returns></returns>
        public object ResolveConfiguration(string settingName, Type type) => ResolveConfiguration(settingName, type, default, false);

        /// <summary>
        /// Resolve a configuration for the current runtime platform with a specific setting name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settingName">Name of setting to load</param>
        /// <param name="type">The type to return</param>
        /// <param name="defaultValue">Value to return if setting is not found</param>
        /// <returns></returns>
        public object ResolveConfiguration(string settingName, Type type, object defaultValue) => ResolveConfiguration(settingName, type, default, false);

        /// <summary>
        /// Resolve a configuration for the current runtime platform with a specific setting name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settingName">Name of setting to load</param>
        /// <param name="type">The type to return</param>
        /// <param name="defaultValue">Value to return if setting is not found</param>
        /// <param name="throwsException">True if exceptions should be thrown if data cannot be loaded</param>
        /// <returns></returns>
        public object ResolveConfiguration(string settingName, Type type, object defaultValue, bool throwsException)
        {
            // if on the .net core platform, resolve a configuration from appsettings.json
            switch (DetectedRuntime.DetectedRuntimeFramework)
            {
                case RuntimeFramework.DotNetCore:
                    return LoadJsonConfiguration(defaultValue, type, settingName, null, null, throwsException);
                case RuntimeFramework.DotNetFramework:
                default:
                    return LoadXmlConfiguration(defaultValue, type, settingName, null, null, throwsException);
            }

            throw new UnsupportedPlatformException();
        }

        /// <summary>
        /// Resolve a configuration for the current runtime platform
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName">Name of section to load</param>
        /// <param name="defaultValue">Value to return if setting is not found</param>
        /// <returns></returns>
        public T ResolveConfigurationSection<T>(string sectionName, T defaultValue) => ResolveConfigurationSection<T>(sectionName, default, false);

        /// <summary>
        /// Resolve a configuration for the current runtime platform
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName">Name of section to load</param>
        /// <param name="defaultValue">Value to return if setting is not found</param>
        /// <param name="throwsException">True if exceptions should be thrown if data cannot be loaded</param>
        /// <returns></returns>
        public T ResolveConfigurationSection<T>(string sectionName, T defaultValue, bool throwsException)
        {
            var nameOfSection = sectionName ?? typeof(T).Name;
            // if on the .net core platform, resolve a configuration from appsettings.json
            switch (DetectedRuntime.DetectedRuntimeFramework)
            {
                case RuntimeFramework.DotNetCore:
                    return LoadJsonConfiguration<T>(defaultValue, null, null, nameOfSection, throwsException);
                case RuntimeFramework.DotNetFramework:
                default:
                    return LoadXmlConfiguration<T>(defaultValue, null, null, nameOfSection, throwsException);
            }

            throw new UnsupportedPlatformException();
        }

        /// <summary>
        /// Resolve a configuration for the current runtime platform
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName">Name of section to load</param>
        /// <param name="type">The type to return</param>
        /// <param name="defaultValue">Value to return if setting is not found</param>
        /// <returns></returns>
        public object ResolveConfigurationSection(string sectionName, Type type, object defaultValue) => ResolveConfigurationSection(sectionName, type, default, false);

        /// <summary>
        /// Resolve a configuration for the current runtime platform
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName">Name of section to load</param>
        /// <param name="type">The type to return</param>
        /// <param name="defaultValue">Value to return if setting is not found</param>
        /// <param name="throwsException">True if exceptions should be thrown if data cannot be loaded</param>
        /// <returns></returns>
        public object ResolveConfigurationSection(string sectionName, Type type, object defaultValue, bool throwsException)
        {
            var nameOfSection = sectionName ?? type.Name;
            // if on the .net core platform, resolve a configuration from appsettings.json
            switch (DetectedRuntime.DetectedRuntimeFramework)
            {
                case RuntimeFramework.DotNetCore:
                    return LoadJsonConfiguration(defaultValue, type, null, null, nameOfSection, throwsException);
                case RuntimeFramework.DotNetFramework:
                default:
                    return LoadXmlConfiguration(defaultValue, type, null, null, nameOfSection, throwsException);
            }

            throw new UnsupportedPlatformException();
        }

        /// <summary>
        /// Resolve a legacy configuration
        /// </summary>
        /// <returns></returns>
        public LegacyConfiguration ResolveLegacyConfigurationFromXml() => _legacyConfigurationLoader.LoadDotNetFrameworkLegacyConfiguration();

        /// <summary>
        /// Resolve a legacy configuration
        /// </summary>
        /// <param name="filename">Filename of xml configuration to load</param>
        /// <returns></returns>
        public LegacyConfiguration ResolveLegacyConfigurationFromXml(string filename) => _legacyConfigurationLoader.LoadDotNetFrameworkLegacyConfiguration(filename);

        /// <summary>
        /// Resolve a legacy configuration
        /// </summary>
        /// <returns></returns>
        public LegacyConfiguration ResolveLegacyConfigurationFromJson() => _legacyConfigurationLoader.LoadDotNetCoreLegacyConfiguration();

        /// <summary>
        /// Resolve a configuration for the current runtime platform
        /// </summary>
        /// <returns></returns>
        public LegacyConfiguration ResolveLegacyConfigurationFromJson(string filename) => _legacyConfigurationLoader.LoadDotNetCoreLegacyConfiguration(filename);

        /// <summary>
        /// Get configuration as an object from Xml configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetFromXml<T>() => GetFromXml<T>(null);

        /// <summary>
        /// Get configuration as an object from Xml configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public T GetFromXml<T>(string sectionName) => LoadXmlConfiguration<T>(default, null, null, sectionName, true);

        /// <summary>
        /// Get configuration as an object from Xml configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetFromXmlFile<T>(string filename) => GetFromXmlFile<T>(filename, null);

        /// <summary>
        /// Get configuration as an object from Xml configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public T GetFromXmlFile<T>(string filename, string sectionName) => LoadXmlConfiguration<T>(default, null, filename, sectionName, true);

        /// <summary>
        /// Get configuration as an object from Json configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetFromJson<T>() => GetFromJson<T>(null);

        /// <summary>
        /// Get configuration as an object from Json configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public T GetFromJson<T>(string sectionName) => LoadJsonConfiguration<T>(default, null, null, sectionName, true);

        /// <summary>
        /// Get configuration as an object from Json configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public T GetFromJsonFile<T>(string filename) => GetFromJsonFile<T>(filename, null);

        /// <summary>
        /// Get configuration as an object from Json configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public T GetFromJsonFile<T>(string filename, string sectionName) => LoadJsonConfiguration<T>(default, null, filename, sectionName, true);

        /// <summary>
        /// Get an IConfiguration
        /// </summary>
        /// <returns></returns>
        public IConfigurationRoot GetConfiguration() => GetConfiguration(null, null);

        /// <summary>
        /// Get an IConfiguration
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public IConfigurationRoot GetConfiguration(string filename) => GetConfiguration(filename, null);

        /// <summary>
        /// Get an IConfiguration
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public IConfigurationRoot GetConfiguration(string filename, string sectionName)
        {
            // for .net core configs such as appsettings.json we can serialize from json directly
            filename = ResolveFilenamePath(filename ?? DotNetCoreSettingsFilename);
            return ConfigProvider.GetConfiguration(Path.GetFileName(filename), Path.GetDirectoryName(filename));
        }

        private string ResolveFilenamePath(string filename)
        {
            var configFile = filename;
            if (!string.IsNullOrEmpty(configFile))
            {
                configFile = Path.GetFullPath(configFile);
                if (!File.Exists(configFile))
                {
                    if (_entryAssembly != null)
                        configFile = Path.Combine(Path.GetDirectoryName(_entryAssembly.Location), filename);
                    else
                        configFile = Path.GetFullPath(filename);
                    if (!File.Exists(configFile))
                        throw new FileNotFoundException($"Could not find configuration file '{configFile}'");
                }
            }
            return configFile;
        }

        private object LoadJsonConfiguration(object defaultValue, Type type, string settingName = null, string filename = null, string sectionName = null, bool throwsException = false)
        {
            filename = ResolveFilenamePath(filename ?? DotNetCoreSettingsFilename);
            var configuration = GetConfiguration(filename, sectionName);
            if (configuration == null)
                throw new ConfigurationMissingException($"Could not load configuration.");

            if (!string.IsNullOrEmpty(settingName))
            {
                if (configuration.Providers.First().TryGet(settingName, out var value))
                    return ((StringValue)value).As(type);
                return defaultValue;
            }
            else
            {
                var nameOfSection = sectionName ?? type.Name;
                var configSection = configuration.GetSection(nameOfSection);
                if (configSection == null)
                {
                    if (throwsException)
                        throw new ConfigurationMissingException($"Unable to resolve configuration section of type '{nameOfSection}'. Please ensure your application '{GetAssemblyName()}' is configured correctly for the '{DetectedRuntime.DetectedRuntimeFramework}' framework.");
                    return defaultValue;
                }

                var value = JsonSerializer.Deserialize(configSection.Value, type);
                return value;
            }
        }

        private T LoadJsonConfiguration<T>(T defaultValue, string settingName = null, string filename = null, string sectionName = null, bool throwsException = false)
        {
            filename = ResolveFilenamePath(filename ?? DotNetCoreSettingsFilename);
            var configuration = GetConfiguration(filename, sectionName);
            if (configuration == null)
                throw new ConfigurationMissingException($"Could not load configuration.");

            if (!string.IsNullOrEmpty(settingName))
            {
                if (configuration.Providers.First().TryGet(settingName, out var value))
                    return ((StringValue)value).As<T>();
                return defaultValue;
            }
            else
            {
                var nameOfSection = sectionName ?? typeof(T).Name;
                var configSection = configuration.GetSection(nameOfSection);
                if (configSection == null)
                {
                    if (sectionName == null)
                    {
                        // try flat mapping
                        var flatMapValue = ConfigProvider.Get<T>(ConfigSource.JsonFile, throwsException, Filename => filename);
                        if (flatMapValue != null)
                            return flatMapValue;
                    }
                    if (throwsException)
                        throw new ConfigurationMissingException($"Unable to resolve configuration section of type '{nameOfSection}'. Please ensure your application '{GetAssemblyName()}' is configured correctly for the '{DetectedRuntime.DetectedRuntimeFramework}' framework.");
                    return defaultValue;
                }

                var value = JsonSerializer.Deserialize<T>(configSection.Value);
                return value;
            }
        }

        private object LoadXmlConfiguration(object defaultValue, Type type, string settingName = null, string filename = null, string sectionName = null, bool throwsException = false)
        {
            // for .net framework configs such as app.config and web.config we need to serialize individual values
            filename = ResolveFilenamePath(filename);
            if (!string.IsNullOrEmpty(settingName))
            {
                object value = null;
                if (!string.IsNullOrEmpty(filename))
                    value = ConfigProvider.Get(type, settingName, ConfigProvider.Empty, ConfigSource.XmlFile, throwsException, Filename => filename);
                if (value == ConfigProvider.Empty)
                {
                    value = ConfigProvider.Get(type, settingName, ConfigProvider.Empty, ConfigSource.WebConfig);
                    if (value == ConfigProvider.Empty)
                    {
                        value = ConfigProvider.Get(type, settingName, ConfigProvider.Empty, ConfigSource.ApplicationConfig);
                        if (value == ConfigProvider.Empty)
                            return defaultValue;
                    }
                }
                return value;
            }
            else
            {
                // recursively map the configuration based on legacy property names
                var nameOfSection = sectionName ?? type.Name;
                var extendedType = type.GetExtendedType();
                var propertyLegacyAttribute = extendedType.GetAttribute<LegacyConfigurationNameAttribute>();
                if (propertyLegacyAttribute != null)
                    return MapTypeProperties(extendedType, defaultValue, filename, propertyLegacyAttribute.PrependChildrenName, throwsException);

                return MapTypeProperties(extendedType, defaultValue, filename, null, throwsException);
            }
        }

        private T LoadXmlConfiguration<T>(T defaultValue, string settingName = null, string filename = null, string sectionName = null, bool throwsException = false)
        {
            // for .net framework configs such as app.config and web.config we need to serialize individual values
            filename = ResolveFilenamePath(filename);
            if (!string.IsNullOrEmpty(settingName))
            {
                object value = null;
                if (!string.IsNullOrEmpty(filename))
                    value = ConfigProvider.Get(typeof(T), settingName, ConfigProvider.Empty, ConfigSource.XmlFile, throwsException, Filename => filename);
                if (value.IsNullOrEmpty())
                {
                    value = ConfigProvider.Get(typeof(T), settingName, ConfigProvider.Empty, ConfigSource.WebConfig);
                    if (value.IsNullOrEmpty())
                    {
                        value = ConfigProvider.Get(typeof(T), settingName, ConfigProvider.Empty, ConfigSource.ApplicationConfig);
                        if (value.IsNullOrEmpty())
                            return defaultValue;
                    }
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
                    return (T)MapTypeProperties(extendedType, defaultValue, filename, propertyLegacyAttribute.PrependChildrenName, throwsException);

                return (T)MapTypeProperties(extendedType, defaultValue, filename, null, throwsException);
            }
        }

        /// <summary>
        /// Serializes a configuration class from .Net Web/App configs using LegacyConfigurationName Attributes (or property names)
        /// Recursive, supports child configuration class definitions
        /// </summary>
        /// <param name="configurationClassType"></param>
        /// <param name="prependPropertyName">Use this value to prepend a configuration setting name in the config</param>
        /// <returns></returns>
        private object MapTypeProperties(ExtendedType configurationClassType, object defaultValue, string filename = null, string prependPropertyName = null, bool throwsException = false)
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
                        value = MapTypeProperties(propertyType, objectFactory.CreateEmptyObject(propertyType), filename, prependPropertyName + propertyLegacyAttribute.PrependChildrenName, throwsException);
                    }
                }
                if (value.IsNullOrEmpty())
                {
                    if (!string.IsNullOrEmpty(filename))
                        value = ConfigProvider.Get(propertyType.Type, propertyName, ConfigProvider.Empty, ConfigSource.XmlFile, throwsException, Filename => filename);
                    if (value.IsNullOrEmpty())
                    {
                        value = ConfigProvider.Get(propertyType.Type, propertyName, ConfigProvider.Empty, ConfigSource.WebConfig);
                        if (value.IsNullOrEmpty())
                        {
                            value = ConfigProvider.Get(propertyType.Type, propertyName, ConfigProvider.Empty, ConfigSource.ApplicationConfig);
                            if (isRequired && value.IsNullOrEmpty())
                                throw new ConfigurationMissingException(propertyName, property.Type);
                        }
                    }
                }
                if (!value.IsNullOrEmpty())
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
