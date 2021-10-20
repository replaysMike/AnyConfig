using AnyConfig.Json;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;

namespace AnyConfig
{
    /// <summary>
    /// Configuration retrieval management
    /// </summary>
    public static partial class ConfigProvider
    {
        public static ConfigValueNotSet Empty => ConfigValueNotSet.Instance;
        public static string LastResolvedConfigurationFilename { get; private set; }
        private static CachedDataProvider<object> _cachedObjects = new CachedDataProvider<object>();
        private static CachedFileProvider _cachedFiles = new CachedFileProvider();

        /// <summary>
        /// Get a connection string by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetConnectionString(string name)
        {
            return InternalGetConnectionString(name);
        }

        /// <summary>
        /// Get a configuration object from a json serialized configuration file
        /// </summary>
        /// <returns></returns>
        public static IConfigurationRoot GetConfiguration()
        {
            return GetConfiguration(DotNetCoreSettingsFilename, null);
        }

        /// <summary>
        /// Get a configuration object from a json serialized configuration file
        /// </summary>
        /// <param name="appSettingsJson">Name of json settings file</param>
        /// <returns></returns>
        public static IConfigurationRoot GetConfiguration(string appSettingsJson)
        {
            return GetConfiguration(appSettingsJson, null);
        }

        /// <summary>
        /// Get a configuration object from a json serialized configuration file
        /// </summary>
        /// <param name="appSettingsJson">Name of json settings file</param>
        /// <param name="path">Optional path to json settings file</param>
        /// <returns></returns>
        public static IConfigurationRoot GetConfiguration(string appSettingsJson, string path)
        {
            var filename = appSettingsJson ?? DotNetCoreSettingsFilename;
            if (string.IsNullOrEmpty(path))
            {
                var resolver = new ConfigurationResolver();
                path = Path.GetDirectoryName(resolver.ResolveFilenamePath(filename));
            }

            var filePath = Path.Combine(path, filename);
            var configurationFileContents = _cachedFiles.AddOrGetFile(filePath);
            var configuration = _cachedObjects.AddOrGet(filePath, () => {
                var jsonParser = new JsonParser();
                var rootNode = jsonParser.Parse(_cachedFiles.AddOrGetFile(filePath));

                var configurationRoot = new ConfigurationRoot(filePath);
                var provider = new JsonConfigurationProvider(filePath);
                configurationRoot.AddProvider(provider);
                foreach (JsonNode node in rootNode.ChildNodes)
                {
                    if (node.ValueType == PrimitiveTypes.Object)
                    {
                        configurationRoot.AddSection(node);
                    }
                    else
                    {
                        // not supported yet
                    }
                }
                provider.SetData(MapAllNodes(rootNode, new List<KeyValuePair<string, string>>()));
                return configurationRoot;
            });

            return configuration as IConfigurationRoot;
        }

        /// <summary>
        /// Get a configuration setting
        /// </summary>
        /// <param name="optionName"></param>
        /// <param name="configSource"></param>
        /// <returns></returns>
        public static string Get(string optionName, string defaultValue)
        {
            InternalTryGet<string>(out var value, optionName, ConfigSource.WebConfig, defaultValue, false);
            return value;
        }

        /// <summary>
        /// Get a configuration setting
        /// </summary>
        /// <param name="optionName"></param>
        /// <param name="configSource"></param>
        /// <returns></returns>
        public static string Get(string optionName, string defaultValue, ConfigSource configSource)
        {
            InternalTryGet<string>(out var value, optionName, configSource, defaultValue, false);
            return value;
        }

        /// <summary>
        /// Get a configuration setting
        /// </summary>
        /// <param name="optionName"></param>
        /// <param name="configSource"></param>
        /// <param name="throwsException">True to throw exception if key is not found</param>
        /// <param name="configParameters">An optional list of key/value parameters to pass to the lookup method. Example: Get(..., SomeKey=>SomeValue, SomeKey2=>SomeValue)</param>
        /// <returns></returns>
        public static string Get(string optionName, string defaultValue, ConfigSource configSource, bool throwsException, params Expression<Func<object, object>>[] configParameters)
        {
            InternalTryGet<string>(out var value, optionName, configSource, defaultValue, throwsException);
            return value;
        }

        /// <summary>
        /// Get a configuration setting
        /// </summary>
        /// <param name="optionName"></param>
        /// <returns></returns>
        public static object Get(Type valueType, string optionName, object defaultValue)
        {
            InternalTryGet(out var value, valueType, optionName, ConfigSource.WebConfig, defaultValue, false);
            return value;
        }

        /// <summary>
        /// Get a configuration setting
        /// </summary>
        /// <param name="optionName"></param>
        /// <param name="configSource"></param>
        /// <param name="throwsException">True to throw exception if key is not found</param>
        /// <param name="configParameters">An optional list of key/value parameters to pass to the lookup method. Example: Get(..., SomeKey=>SomeValue, SomeKey2=>SomeValue)</param>
        /// <returns></returns>
        public static object Get(Type valueType, string optionName, object defaultValue, ConfigSource configSource)
        {
            InternalTryGet(out var value, valueType, optionName, configSource, defaultValue, false);
            return value;
        }

        /// <summary>
        /// Get a configuration setting
        /// </summary>
        /// <param name="optionName"></param>
        /// <param name="configSource"></param>
        /// <param name="throwsException">True to throw exception if key is not found</param>
        /// <param name="configParameters">An optional list of key/value parameters to pass to the lookup method. Example: Get(..., SomeKey=>SomeValue, SomeKey2=>SomeValue)</param>
        /// <returns></returns>
        public static object Get(Type valueType, string optionName, object defaultValue, ConfigSource configSource, bool throwsException, params Expression<Func<object, object>>[] configParameters)
        {
            InternalTryGet(out var value, valueType, optionName, configSource, defaultValue, throwsException, false, configParameters);
            return value;
        }

        /// <summary>
        /// Get a configuration setting
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="optionName"></param>
        /// <param name="configSource"></param>
        /// <returns></returns>
        public static T Get<T>(string optionName, T defaultValue)
        {
            InternalTryGet<T>(out var value, optionName, ConfigSource.WebConfig, defaultValue, false);
            return value;
        }

        /// <summary>
        /// Get a configuration setting
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="optionName"></param>
        /// <param name="configSource"></param>
        /// <returns></returns>
        public static T Get<T>(string optionName, T defaultValue, ConfigSource configSource)
        {
            InternalTryGet<T>(out var value, optionName, configSource, defaultValue, false);
            return value;
        }

        /// <summary>
        /// Get a configuration setting
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="optionName"></param>
        /// <param name="defaultValue">Default value to return if setting is not found</param>
        /// <param name="configSource"></param>
        /// <param name="throwsException">True to throw exception if key is not found</param>
        /// <param name="configParameters">An optional list of key/value parameters to pass to the lookup method. Example: Get(..., SomeKey=>SomeValue, SomeKey2=>SomeValue)</param>
        /// <returns></returns>
        public static T Get<T>(string optionName, T defaultValue, ConfigSource configSource, bool throwsException, params Expression<Func<object, object>>[] configParameters)
        {
            InternalTryGet<T>(out var value, optionName, configSource, defaultValue, throwsException, false, configParameters);
            return value;
        }

        /// <summary>
        /// Get a configuration setting
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configSource"></param>
        /// <param name="throwsException">True to throw exception if key is not found</param>
        /// <param name="configParameters">An optional list of key/value parameters to pass to the lookup method. Example: Get(..., SomeKey=>SomeValue, SomeKey2=>SomeValue)</param>
        /// <returns></returns>
        public static T Get<T>(ConfigSource configSource, bool throwsException, params Expression<Func<object, object>>[] configParameters)
        {
            InternalTryGet<T>(out var value, string.Empty, configSource, default, throwsException, false, configParameters);
            return value;
        }

        /// <summary>
        /// Get a configuration setting
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defaultValue">Default value to return if setting is not found</param>
        /// <param name="configSource"></param>
        /// <param name="throwsException">True to throw exception if key is not found</param>
        /// <param name="configParameters">An optional list of key/value parameters to pass to the lookup method. Example: Get(..., SomeKey=>SomeValue, SomeKey2=>SomeValue)</param>
        /// <returns></returns>
        public static T Get<T>(T defaultValue, ConfigSource configSource, bool throwsException, params Expression<Func<object, object>>[] configParameters)
        {
            InternalTryGet<T>(out var value, string.Empty, configSource, defaultValue, throwsException, false, configParameters);
            return value;
        }

        /// <summary>
        /// Get a configuration setting
        /// </summary>
        /// <param name="optionName"></param>
        /// <param name="configSource"></param>
        /// <returns></returns>
        public static string Get(string optionName)
        {
            InternalTryGet<string>(out var value, optionName, ConfigSource.WebConfig, default, false);
            return value;
        }

        /// <summary>
        /// Get a configuration setting
        /// </summary>
        /// <param name="optionName"></param>
        /// <param name="configSource"></param>
        /// <returns></returns>
        public static string Get(string optionName, ConfigSource configSource)
        {
            InternalTryGet<string>(out var value, optionName, configSource, default, false);
            return value;
        }

        /// <summary>
        /// Get a configuration setting
        /// </summary>
        /// <param name="optionName"></param>
        /// <param name="configSource"></param>
        /// <param name="throwsException">True to throw exception if key is not found</param>
        /// <param name="configParameters">An optional list of key/value parameters to pass to the lookup method. Example: Get(..., SomeKey=>SomeValue, SomeKey2=>SomeValue)</param>
        /// <returns></returns>
        public static string Get(string optionName, ConfigSource configSource, bool throwsException, params Expression<Func<object, object>>[] configParameters)
        {
            InternalTryGet<string>(out var value, optionName, configSource, default, throwsException);
            return value;
        }

        /// <summary>
        /// Get a configuration setting
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="optionName"></param>
        /// <param name="configSource"></param>
        /// <returns></returns>
        public static T Get<T>(string optionName)
        {
            InternalTryGet<T>(out var value, optionName, ConfigSource.WebConfig, default, false);
            return value;
        }

        /// <summary>
        /// Get a configuration setting
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="optionName"></param>
        /// <param name="configSource"></param>
        /// <returns></returns>
        public static T Get<T>(string optionName, ConfigSource configSource)
        {
            InternalTryGet<T>(out var value, optionName, configSource, default, false);
            return value;
        }

        /// <summary>
        /// Get a configuration setting
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="optionName"></param>
        /// <param name="configSource"></param>
        /// <param name="throwsException">True to throw exception if key is not found</param>
        /// <param name="configParameters">An optional list of key/value parameters to pass to the lookup method. Example: Get(..., SomeKey=>SomeValue, SomeKey2=>SomeValue)</param>
        /// <returns></returns>
        public static T Get<T>(string optionName, ConfigSource configSource, bool throwsException, params Expression<Func<object, object>>[] configParameters)
        {
            InternalTryGet<T>(out var value, optionName, configSource, default, throwsException, false, configParameters);
            return value;
        }

        /// <summary>
        /// Get a configuration setting
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="optionName"></param>
        /// <param name="configSource"></param>
        /// <param name="throwsException">True to throw exception if key is not found</param>
        /// <param name="expandEnvironmentVariables">True to expand environment variables</param>
        /// <param name="configParameters">An optional list of key/value parameters to pass to the lookup method. Example: Get(..., SomeKey=>SomeValue, SomeKey2=>SomeValue)</param>
        /// <returns></returns>
        public static T Get<T>(string optionName, ConfigSource configSource, bool throwsException, bool expandEnvironmentVariables, params Expression<Func<object, object>>[] configParameters)
        {
            InternalTryGet<T>(out var value, optionName, configSource, default, throwsException, expandEnvironmentVariables, configParameters);
            return value;
        }

        /// <summary>
        /// Get a configuration setting from a specific web/app config section name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="optionName"></param>
        /// <returns></returns>
        public static T GetFromSection<T>(string optionName)
        {
            GetWebConfigSetting<T>(out var value, optionName, "appSettings", default, false);
            return value;
        }

        /// <summary>
        /// Get a configuration setting from a specific web/app config section name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="optionName"></param>
        /// <param name="sectionName">The config section name to read from</param>
        /// <returns></returns>
        public static T GetFromSection<T>(string optionName, string sectionName)
        {
            GetWebConfigSetting<T>(out var value, optionName, sectionName, default, false);
            return value;
        }

        /// <summary>
        /// Get a configuration setting from a specific web/app config section name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="optionName"></param>
        /// <param name="sectionName">The config section name to read from</param>
        /// <param name="throwsException">True to throw exception if key is not found</param>
        /// <returns></returns>
        public static T GetFromSection<T>(string optionName, string sectionName, bool throwsException)
        {
            GetWebConfigSetting<T>(out var value, optionName, sectionName, default, throwsException);
            return value;
        }

        /// <summary>
        /// Get a configuration setting
        /// </summary>
        /// <param name="value">The value to output</param>
        /// <param name="valueType">The type of the configuration value</param>
        /// <param name="optionName">The configuration name of the option</param>
        /// <param name="defaultValue">The default value if the configuration is not found</param>
        /// <param name="configSource">Which source the configuration should be loaded from</param>
        /// <param name="throwsException">True to throw exception if key is not found</param>
        /// <param name="configParameters">An optional list of key/value parameters to pass to the lookup method. Example: Get(..., SomeKey=>SomeValue, SomeKey2=>SomeValue)</param>
        /// <returns></returns>
        public static bool TryGet(out object value, Type valueType, string optionName, object defaultValue, ConfigSource configSource, bool throwsException, params Expression<Func<object, object>>[] configParameters)
        {
            return InternalTryGet(out value, valueType, optionName, configSource, defaultValue, throwsException, false, configParameters);
        }

        /// <summary>
        /// Get a configuration setting
        /// </summary>
        /// <param name="value">The value to output</param>
        /// <param name="optionName">The configuration name of the option</param>
        /// <param name="defaultValue">The default value if the configuration is not found</param>
        /// <param name="configSource">Which source the configuration should be loaded from</param>
        /// <param name="throwsException">True to throw exception if key is not found</param>
        /// <param name="configParameters">An optional list of key/value parameters to pass to the lookup method. Example: Get(..., SomeKey=>SomeValue, SomeKey2=>SomeValue)</param>
        /// <returns></returns>
        public static bool TryGet<T>(out T value, string optionName, T defaultValue, ConfigSource configSource, bool throwsException, params Expression<Func<object, object>>[] configParameters)
        {
            return InternalTryGet<T>(out value, optionName, configSource, defaultValue, throwsException, false, configParameters);
        }
    }
}
