﻿using AnyConfig.Collections;
using AnyConfig.Exceptions;
using AnyConfig.Json;
using AnyConfig.Models;
using AnyConfig.Xml;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using TypeSupport;
using TypeSupport.Extensions;

namespace AnyConfig
{
    /// <summary>
    /// Multi-framework configuration resolver
    /// Supports .Net Framework and .Net Core configurations
    /// </summary>
    public sealed class ConfigurationResolver : IConfigurationResolver
    {
        private const string DotNetCoreSettingsFilename = "appsettings.json";
        private readonly Assembly _entryAssembly;
        private static readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);
        private static readonly Dictionary<string, Assembly> _registeredEntryAssemblies = new Dictionary<string, Assembly>();
        private LegacyConfigurationLoader _legacyConfigurationLoader;
        private static IConfiguration _appConfiguration;
        private ConfigProvider _configProvider = new ConfigProvider();
        private static readonly ConcurrentDictionary<string, CachedConfiguration> _cachedConfigurationFiles = new ConcurrentDictionary<string, CachedConfiguration>();

        /// <summary>
        /// Get the pre-configured IConfiguration. To set, see <see cref="SetAppConfiguration"/>
        /// </summary>
        public IConfiguration AppConfiguration => _appConfiguration;

        /// <summary>
        /// The last configuration filename that was resolved
        /// </summary>
        public string LastResolvedConfigurationFilename { get; private set; }

        /// <summary>
        /// Register an entry assembly with the resolver
        /// </summary>
        /// <param name="entryAssembly"></param>
        public static void RegisterEntryAssembly(Assembly entryAssembly)
        {
            _cacheLock.Wait();
            try
            {
                var runtime = RuntimeEnvironment.DetectRuntime(entryAssembly);
                if (!_registeredEntryAssemblies.ContainsKey(runtime.DetectedRuntimeFrameworkDescription))
                    _registeredEntryAssemblies.Add(runtime.DetectedRuntimeFrameworkDescription, entryAssembly);
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        /// <summary>
        /// Gives AnyConfig a pre-configured IConfiguration instance to use for getting config on .net core platforms
        /// </summary>
        public static void SetAppConfiguration(IConfiguration configuration) => _appConfiguration = configuration;

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
            if (_registeredEntryAssemblies.ContainsKey(DetectedRuntime.DetectedRuntimeFrameworkDescription))
                _entryAssembly = _registeredEntryAssemblies[DetectedRuntime.DetectedRuntimeFrameworkDescription] ?? Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            _legacyConfigurationLoader = new LegacyConfigurationLoader(_entryAssembly);
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
                case RuntimeFramework.DotNet6:
                case RuntimeFramework.DotNet5:
                case RuntimeFramework.DotNetCore:
                default:
                    return LoadJsonConfiguration<T>(default);
                case RuntimeFramework.DotNetFramework:
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
                case RuntimeFramework.DotNet5:
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
                case RuntimeFramework.DotNet5:
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
                case RuntimeFramework.DotNet5:
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
                case RuntimeFramework.DotNet5:
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
        /// Get configuration as an object from Xml configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settingName"></param>
        /// <returns></returns>
        public T GetSettingFromXmlFile<T>(string filename, string settingName) => LoadXmlConfiguration<T>(default, settingName, filename, null, true);

        /// <summary>
        /// Get configuration as an object from Xml configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetFromXmlFile<T>(string filename, string settingName, T defaultValue) => LoadXmlConfiguration(defaultValue, settingName, filename);

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
        /// Get configuration as an object from Json configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetFromJsonFile<T>(string filename, string settingName, T defaultValue) => LoadJsonConfiguration(defaultValue, settingName, filename);

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

            var configuration = _configProvider.GetConfiguration(Path.GetFileName(filename), Path.GetDirectoryName(filename));
            return configuration;
        }

        /// <summary>
        /// Resolve the configuration filename path
        /// </summary>
        /// <param name="filename">The configuration filename to resolve</param>
        /// <returns></returns>
        public string ResolveFilenamePath(string filename)
        {
            var configFile = filename;
            if (!string.IsNullOrEmpty(configFile))
            {
                // try the default path
                configFile = Path.GetFullPath(configFile);
                if (!File.Exists(configFile))
                {
                    // try the entry assembly
                    if (_entryAssembly != null)
                        configFile = Path.Combine(Path.GetDirectoryName(_entryAssembly.Location), filename);
                    else
                        configFile = Path.GetFullPath(filename);
                    if (!File.Exists(configFile))
                    {
                        // try the current process
                        configFile = Path.Combine(GetCurrentProcessPath(), filename);
                    }
                    if (!File.Exists(configFile))
                        throw new ConfigurationMissingException($"Could not find configuration file '{configFile}'");
                }
            }
            LastResolvedConfigurationFilename = configFile;
            return configFile;
        }

        private IConfigurationRoot LoadConfigurationFromFile(string filename, string sectionName, out string resolvedFilename)
        {
            resolvedFilename = ResolveFilenamePath(filename ?? DotNetCoreSettingsFilename);
            var configuration = GetConfiguration(resolvedFilename, sectionName);
            if (configuration == null)
                throw new ConfigurationMissingException($"Could not load configuration from file named '{resolvedFilename}'!");
            return configuration;
        }

        public object LoadJsonConfiguration(object defaultValue, Type type, string settingName = null, string filename = null, string sectionName = null, bool throwsException = false)
        {
            IConfigurationRoot configuration;
            if (!string.IsNullOrEmpty(settingName))
            {
                if (_appConfiguration is not null)
                    return _appConfiguration.GetValue(type, settingName, defaultValue);

                configuration = LoadConfigurationFromFile(filename, sectionName, out _);
                if (configuration.Providers.First().TryGet(settingName, out var settingValue))
                    return ((StringValue)settingValue).As(type);
                return defaultValue;
            }

            configuration = LoadConfigurationFromFile(filename, sectionName, out _);
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

        public T LoadJsonConfiguration<T>(T defaultValue, string settingName = null, string filename = null, string sectionName = null, bool throwsException = false)
        {
            IConfigurationRoot configuration;
            string configurationFilename;
            if (!string.IsNullOrEmpty(settingName))
            {
                // if we were injected an IConfiguration, simply use it
                if (_appConfiguration is not null)
                    return _appConfiguration.GetValue(settingName, defaultValue);
                
                configuration = LoadConfigurationFromFile(filename, sectionName, out configurationFilename);
                if (!configuration.Providers.Any())
                    throw new ConfigurationMissingException($"Could not resolve a json configuration provider!");
                if (configuration.Providers
                        .Where(x => x?.GetType() == typeof(JsonConfigurationProvider))
                        .First()
                        .TryGet(settingName, out var settingValue))
                    return ((StringValue)settingValue).As<T>();
                return defaultValue;
            }

            configuration = LoadConfigurationFromFile(filename, sectionName, out configurationFilename);
            var nameOfSection = sectionName ?? typeof(T).Name;
            var configSection = configuration.GetSection(nameOfSection) as ConfigurationSection;
            if (string.IsNullOrEmpty(configSection.GetNodeStructuredText()))
            {
                if (sectionName == null)
                {
                    // try flat mapping
                    var flatMapValue = new ConfigProvider().Get<T>(ConfigSource.JsonFile, throwsException, Filename => configurationFilename);
                    if (flatMapValue != null)
                        return flatMapValue;
                }
                if (throwsException)
                    throw new ConfigurationMissingException($"Unable to resolve configuration section of type '{nameOfSection}'. Please ensure your application '{GetAssemblyName()}' is configured correctly for the '{DetectedRuntime.DetectedRuntimeFramework}' framework.");
                return defaultValue;
            }

            var value = JsonSerializer.Deserialize<T>(configSection.GetNodeStructuredText());
            return value;
        }

        public object LoadXmlConfiguration(object defaultValue, Type type, string settingName = null, string filename = null, string sectionName = null, bool throwsException = false)
        {
            // if we were injected an IConfiguration, simply use it
            if (_appConfiguration is not null && !string.IsNullOrEmpty(settingName))
                return _appConfiguration.GetValue(type, settingName, defaultValue);

            // for .net framework configs such as app.config and web.config we need to serialize individual values
            filename = ResolveFilenamePath(filename);
            if (!string.IsNullOrEmpty(settingName))
            {
                object value = null;
                if (!string.IsNullOrEmpty(filename))
                    value = _configProvider.Get(type, settingName, ConfigProvider.Empty, ConfigSource.XmlFile, throwsException, Filename => filename);
                if (value == ConfigProvider.Empty)
                {
                    value = _configProvider.Get(type, settingName, ConfigProvider.Empty, ConfigSource.WebConfig);
                    if (value == ConfigProvider.Empty)
                    {
                        value = _configProvider.Get(type, settingName, ConfigProvider.Empty, ConfigSource.ApplicationConfig);
                        if (value == ConfigProvider.Empty)
                        {
                            var currentProcessFilename = GetCurrentProcessFilename() + ".config";
                            value = _configProvider.Get(type, settingName, ConfigProvider.Empty, ConfigSource.XmlFile, throwsException, Filename => currentProcessFilename);
                            if (value == ConfigProvider.Empty)
                                return defaultValue;
                        }
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
                    return MapTypeProperties(extendedType, defaultValue, filename, sectionName, propertyLegacyAttribute.PrependChildrenName, throwsException);

                return MapTypeProperties(extendedType, defaultValue, filename, sectionName, null, throwsException);
            }
        }

        public T LoadXmlConfiguration<T>(T defaultValue, string settingName = null, string filename = null, string sectionName = null, bool throwsException = false)
        {
            // if we were injected an IConfiguration, simply use it
            if (_appConfiguration is not null && !string.IsNullOrEmpty(settingName))
                return _appConfiguration.GetValue(settingName, defaultValue);

            // for .net framework configs such as app.config and web.config we need to serialize individual values
            filename = ResolveFilenamePath(filename);
            if (!string.IsNullOrEmpty(settingName))
            {

                var valueExists = false;
                object value = defaultValue;
                if (!string.IsNullOrEmpty(filename))
                    valueExists = _configProvider.TryGet(out value, typeof(T), settingName, ConfigProvider.Empty, ConfigSource.XmlFile, throwsException, Filename => filename);
                if (!valueExists)
                {
                    valueExists = _configProvider.TryGet(out value, typeof(T), settingName, ConfigProvider.Empty, ConfigSource.WebConfig, throwsException);
                    if (!valueExists)
                    {
                        valueExists = _configProvider.TryGet(out value, typeof(T), settingName, ConfigProvider.Empty, ConfigSource.ApplicationConfig, throwsException);
                        if (!valueExists)
                        {
                            var currentProcessFilename = GetCurrentProcessFilename() + ".config";
                            valueExists = _configProvider.TryGet(out value, typeof(T), settingName, ConfigProvider.Empty, ConfigSource.XmlFile, throwsException, Filename => currentProcessFilename);
                        }
                    }
                }
                if (valueExists)
                    return (T)value;
                else
                    return defaultValue;
            }
            else
            {
                // recursively map the configuration based on legacy property names
                var nameOfSection = sectionName ?? typeof(T).Name;
                var extendedType = typeof(T).GetExtendedType();
                var propertyLegacyAttribute = extendedType.GetAttribute<LegacyConfigurationNameAttribute>();
                if (propertyLegacyAttribute != null)
                    return (T)MapTypeProperties(extendedType, defaultValue, filename, sectionName, propertyLegacyAttribute.PrependChildrenName, throwsException);

                return (T)MapTypeProperties(extendedType, defaultValue, filename, sectionName, null, throwsException);
            }
        }

        /// <summary>
        /// Serializes a configuration class from .Net Web/App configs using LegacyConfigurationName Attributes (or property names)
        /// Recursive, supports child configuration class definitions
        /// </summary>
        /// <param name="configurationClassType"></param>
        /// <param name="prependPropertyName">Use this value to prepend a configuration setting name in the config</param>
        /// <returns></returns>
        private object MapTypeProperties(ExtendedType configurationClassType, object defaultValue, string filename = null, string sectionName = null, string prependPropertyName = null, bool throwsException = false)
        {
            var objectFactory = new ObjectFactory();
            var returnObject = objectFactory.CreateEmptyObject(configurationClassType);

            object legacyCustomSection = null;
            if (!string.IsNullOrEmpty(sectionName))
                legacyCustomSection = ConfigurationManager.GetSection(sectionName);
            if (legacyCustomSection == null)
                legacyCustomSection = ConfigurationManager.GetSection(configurationClassType.Name);
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
                else
                {
                    return legacyCustomSection;
                }
            }

            var anyPropertiesMapped = false;
            foreach (var property in configurationClassType.Properties)
            {
                var isRequired = false;
                var propertyName = property.Name;
                var propertyType = property.Type;
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
                        propertyType = propertyType.Refresh(TypeSupportOptions.All);
                        value = MapTypeProperties(propertyType, objectFactory.CreateEmptyObject(propertyType), filename, sectionName, prependPropertyName + propertyLegacyAttribute.PrependChildrenName, throwsException);
                    }
                }
                if (value.IsNullOrEmpty())
                {
                    if (!string.IsNullOrEmpty(filename))
                        value = _configProvider.Get(propertyType, propertyName, ConfigProvider.Empty, ConfigSource.XmlFile, throwsException, Filename => filename);
                    if (value.IsNullOrEmpty())
                    {
                        value = _configProvider.Get(propertyType, propertyName, ConfigProvider.Empty, ConfigSource.WebConfig);
                        if (value.IsNullOrEmpty())
                        {
                            value = _configProvider.Get(propertyType, propertyName, ConfigProvider.Empty, ConfigSource.ApplicationConfig);
                            if (isRequired && value.IsNullOrEmpty())
                                throw new ConfigurationMissingException(propertyName, propertyType);
                            if (value.IsNullOrEmpty())
                            {
                                var currentProcessFilename = GetCurrentProcessFilename() + ".config";
                                value = _configProvider.Get(propertyType, propertyName, ConfigProvider.Empty, ConfigSource.XmlFile, throwsException, Filename => currentProcessFilename);
                                if (isRequired && value.IsNullOrEmpty())
                                    throw new ConfigurationMissingException(propertyName, propertyType);
                            }
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
            LastResolvedConfigurationFilename = _configProvider.LastResolvedConfigurationFilename;
            return returnObject;
        }

        /// <summary>
        /// Get the path of the current process (cached)
        /// </summary>
        /// <returns></returns>
        private string GetCurrentProcessPath() => Path.GetDirectoryName(GetCurrentProcessFilename());

        /// <summary>
        /// Get the path of the current process (cached)
        /// </summary>
        /// <returns></returns>
        private string GetCurrentProcessFilename() => CachedProcess.GetCurrentProcessFilename();

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
