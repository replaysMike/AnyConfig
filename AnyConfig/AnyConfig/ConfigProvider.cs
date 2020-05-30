using AnyConfig.Exceptions;
using AnyConfig.Json;
using AnyConfig.Models;
using AnyConfig.Xml;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using TypeSupport;
using TypeSupport.Extensions;

[assembly: InternalsVisibleTo("AnyConfig.Tests")]
namespace AnyConfig
{
    /// <summary>
    /// Configuration retrieval management
    /// </summary>
    public static class ConfigProvider
    {
        private const string DotNetCoreSettingsFilename = "appsettings.json";
        private const string DotNetFrameworkSettingsFilename = "App.config";
        private static readonly Dictionary<string, string> _cachedConfigurationFiles = new Dictionary<string, string>();

        public static ConfigValueNotSet Empty => ConfigValueNotSet.Instance;
        public static string LastResolvedConfigurationFilename { get; private set; }


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
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"The configuration file named '{filePath}' was not found.");

            var jsonParser = new JsonParser();
            var rootNode = jsonParser.Parse(File.ReadAllText(filePath));

            var configuration = new ConfigurationRoot(filePath);
            var provider = new JsonConfigurationProvider(filePath);
            configuration.AddProvider(provider);
            foreach (JsonNode node in rootNode.ChildNodes)
            {
                if (node.ValueType == PrimitiveTypes.Object)
                {
                    configuration.AddSection(node);
                }
                else
                {
                    // not supported yet
                }
            }
            provider.SetData(MapAllNodes(rootNode, new List<KeyValuePair<string, string>>()));
            return configuration as IConfigurationRoot;
        }

        private static List<KeyValuePair<string, string>> MapAllNodes(JsonNode node, List<KeyValuePair<string, string>> values)
        {
            if (node.NodeType == JsonNodeType.Object)
            {
                foreach (JsonNode childNode in node.ChildNodes)
                    values = MapAllNodes(childNode, values);
            }
            else if (node.NodeType == JsonNodeType.Value)
            {
                var key = $"{node.FullPathWithArrayHints.Replace("/", ":").Substring(1)}";
                key = RemapIConfigurationArrayPositionText(key);
                var kvp = new KeyValuePair<string, string>(key, GetNodeValue(node.Value, node.ValueType));
                values.Add(kvp);
            }
            else if (node.NodeType == JsonNodeType.Array)
            {
                foreach (JsonNode arrayNode in node.ArrayNodes)
                    values = MapAllNodes(arrayNode, values);
            }
            else
            {
                // not yet supported
            }
            return values;
        }

        /// <summary>
        /// Remap array positions from [x] to :x
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal static string RemapIConfigurationArrayPositionText(string key)
        {
            if (key.Contains('['))
            {
                var builder = new StringBuilder();
                foreach (var c in key)
                {
                    if (c == '[')
                    {
                        builder.Append(':');
                        continue;
                    }
                    else if (c == ']')
                        continue;
                    builder.Append(c);
                }
                return builder.ToString();
            }
            return key;
        }

        /// <summary>
        /// Remap array positions from :x to [x]
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal static string RemapINodeArrayPositionText(string key)
        {
            var matches = Regex.Match(key, @"\/[\d+]+(?:\/)?", RegexOptions.Compiled);
            if (matches.Success)
            {
                foreach (Capture match in matches.Captures)
                {
                    var arrayIndex = key.Substring(match.Index + 1, match.Length - 1).Replace("/", "");
                    var pathStart = key.Substring(0, match.Index);
                    var pathEnd = key.Substring(match.Index + match.Length, key.Length - (match.Index + match.Length));
                    if (pathEnd.Length > 0)
                        pathEnd = "/" + pathEnd;
                    key = $"{pathStart}[{arrayIndex}]{pathEnd}";
                }
            }
            return key;
        }

        private static string GetNodeValue(string value, PrimitiveTypes type)
        {
            switch (type)
            {
                case PrimitiveTypes.Boolean:
                    return bool.Parse(value).ToString();
                default:
                    return value;
            }
        }

        /// <summary>
        /// Get a configuration setting
        /// </summary>
        /// <param name="optionName"></param>
        /// <param name="configSource"></param>
        /// <returns></returns>
        public static string Get(string optionName, string defaultValue)
        {
            return InternalGet<string>(optionName, ConfigSource.WebConfig, defaultValue, false);
        }

        /// <summary>
        /// Get a configuration setting
        /// </summary>
        /// <param name="optionName"></param>
        /// <param name="configSource"></param>
        /// <returns></returns>
        public static string Get(string optionName, string defaultValue, ConfigSource configSource)
        {
            return InternalGet<string>(optionName, configSource, defaultValue, false);
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
            return InternalGet<string>(optionName, configSource, defaultValue, throwsException);
        }

        /// <summary>
        /// Get a configuration setting
        /// </summary>
        /// <param name="optionName"></param>
        /// <returns></returns>
        public static object Get(Type valueType, string optionName, object defaultValue)
        {
            return InternalGet(valueType, optionName, ConfigSource.WebConfig, defaultValue, false);
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
            return InternalGet(valueType, optionName, configSource, defaultValue, false);
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
            return InternalGet(valueType, optionName, configSource, defaultValue, throwsException, configParameters);
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
            return InternalGet<T>(optionName, ConfigSource.WebConfig, defaultValue, false);
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
            return InternalGet<T>(optionName, configSource, defaultValue, false);
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
            return InternalGet<T>(optionName, configSource, defaultValue, throwsException, configParameters);
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
            return InternalGet<T>(string.Empty, configSource, default, throwsException, configParameters);
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
            return InternalGet<T>(string.Empty, configSource, defaultValue, throwsException, configParameters);
        }

        /// <summary>
        /// Get a configuration setting
        /// </summary>
        /// <param name="optionName"></param>
        /// <param name="configSource"></param>
        /// <returns></returns>
        public static string Get(string optionName)
        {
            return InternalGet<string>(optionName, ConfigSource.WebConfig, default, false);
        }

        /// <summary>
        /// Get a configuration setting
        /// </summary>
        /// <param name="optionName"></param>
        /// <param name="configSource"></param>
        /// <returns></returns>
        public static string Get(string optionName, ConfigSource configSource)
        {
            return InternalGet<string>(optionName, configSource, default, false);
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
            return InternalGet<string>(optionName, configSource, default, throwsException);
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
            return InternalGet<T>(optionName, ConfigSource.WebConfig, default, false);
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
            return InternalGet<T>(optionName, configSource, default, false);
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
            return InternalGet<T>(optionName, configSource, default, throwsException, configParameters);
        }

        /// <summary>
        /// Get a configuration setting from a specific web/app config section name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="optionName"></param>
        /// <returns></returns>
        public static T GetFromSection<T>(string optionName)
        {
            return GetWebConfigSetting<T>(optionName, "appSettings", default, false);
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
            return GetWebConfigSetting<T>(optionName, sectionName, default, false);
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
            return GetWebConfigSetting<T>(optionName, sectionName, default, throwsException);
        }

        private static T InternalGet<T>(string optionName, ConfigSource configSource, T defaultValue, bool throwsException = false, params Expression<Func<object, object>>[] configParameters)
        {
            T result;
            if (typeof(T) == typeof(string))
                result = (T)(object)null;
            else
                result = new ObjectFactory().CreateEmptyObject<T>();

            switch (configSource)
            {
                case ConfigSource.WebConfig:
                case ConfigSource.ApplicationConfig:
                    // Standard .net config file
                    result = GetWebConfigSetting<T>(optionName, defaultValue, throwsException, configParameters);
                    break;
                case ConfigSource.EmbeddedResource:
                    // embedded resource dictionary value
                    result = GetEmbeddedResourceSetting<T>(optionName, defaultValue, throwsException, configParameters);
                    break;
                case ConfigSource.Registry:
                    // implement registry requirements
                    break;
                case ConfigSource.Custom:
                    // implement custom requirements
                    break;
                case ConfigSource.JsonFile:
                    // parse a Json file and load a config value
                    result = GetJsonConfigSetting<T>(optionName, defaultValue, throwsException, configParameters);
                    break;
                case ConfigSource.XmlFile:
                    // parse a Xml file and load a config value
                    result = GetXmlConfigSetting<T>(optionName, defaultValue, throwsException, configParameters);
                    break;
                default:
                    break;
            }

            return result;
        }

        private static object InternalGet(Type valueType, string optionName, ConfigSource configSource, object defaultValue, bool throwsException = false, params Expression<Func<object, object>>[] configParameters)
        {
            object result;
            if (valueType == typeof(string))
                result = (string)(object)null;
            else
                result = new ObjectFactory().CreateEmptyObject(valueType);

            switch (configSource)
            {
                case ConfigSource.WebConfig:
                    // Standard web.config file
                    result = GetWebConfigSetting(valueType, optionName, defaultValue, throwsException, configParameters);
                    break;
                case ConfigSource.ApplicationConfig:
                    // Standard app.config file
                    result = GetWebConfigSetting(valueType, optionName, defaultValue, throwsException, configParameters);
                    break;
                case ConfigSource.EmbeddedResource:
                    // embedded resource dictionary value
                    result = GetEmbeddedResourceSetting(valueType, optionName, defaultValue, throwsException, configParameters);
                    break;
                case ConfigSource.Registry:
                    // implement registry requirements
                    break;
                case ConfigSource.Custom:
                    // implement custom requirements
                    break;
                case ConfigSource.JsonFile:
                    // parse a Json file and load a config value
                    result = GetJsonConfigSetting(valueType, optionName, defaultValue, throwsException, configParameters);
                    break;
                case ConfigSource.XmlFile:
                    // parse a Xml file and load a config value
                    result = GetXmlConfigSetting(valueType, optionName, defaultValue, throwsException, configParameters);
                    break;
                default:
                    break;
            }

            return result;
        }

        private static T GetWebConfigSetting<T>(string optionName, string configSectionName, T defaultValue, bool throwsException = false)
        {
            var result = defaultValue;

            if (!string.IsNullOrEmpty(configSectionName))
            {
                if (ConfigurationManager.GetSection(configSectionName) is NameValueCollection config)
                {
                    if (config[optionName] == null)
                    {
                        if (throwsException)
                            throw new KeyNotFoundException($"The configuration key '{optionName}' does not exist.");
                    }
                    else
                    {
                        var val = System.Environment.ExpandEnvironmentVariables(config[optionName]);
                        result = ConvertStringToNativeType<T>(val, defaultValue);
                    }
                }
                else
                {
                    if (throwsException)
                        throw new KeyNotFoundException(
                            $"The configuration section '{configSectionName}' does not exist.");
                }
            }
            else
            {
                if (throwsException)
                    throw new KeyNotFoundException("Invalid config parameters provided. Expecting ConfigSection => \"CustomSectionName\".");
            }

            return result;
        }

        private static object GetWebConfigSetting(Type valueType, string optionName, string configSectionName, object defaultValue, bool throwsException = false)
        {
            var result = defaultValue;

            if (!string.IsNullOrEmpty(configSectionName))
            {
                if (ConfigurationManager.GetSection(configSectionName) is NameValueCollection config)
                {
                    if (config[optionName] == null)
                    {
                        if (throwsException)
                            throw new KeyNotFoundException($"The configuration key '{optionName}' does not exist.");
                    }
                    else
                    {
                        var val = System.Environment.ExpandEnvironmentVariables(config[optionName]);
                        result = ConvertStringToNativeType(valueType, val, defaultValue);
                    }
                }
                else
                {
                    if (throwsException)
                        throw new KeyNotFoundException(
                            $"The configuration section '{configSectionName}' does not exist.");
                }
            }
            else
            {
                if (throwsException)
                    throw new KeyNotFoundException("Invalid config parameters provided. Expecting ConfigSection => \"CustomSectionName\".");
            }

            return result;
        }

        private static string InternalGetConnectionString(string name)
        {
            var connectionStringKey = ConfigurationManager.ConnectionStrings[name];
            if (connectionStringKey != null)
                return connectionStringKey.ConnectionString;
            else
                return null;
        }

        private static T GetWebConfigSetting<T>(string optionName, T defaultValue, bool throwsException = false, params Expression<Func<object, object>>[] configParameters)
        {
            var result = defaultValue;

            if (configParameters != null && configParameters.Length > 0)
            {
                // process a custom config section
                var configSectionName = configParameters.GetExpressionValue("SectionName");

                if (!string.IsNullOrEmpty(configSectionName))
                {
                    return GetWebConfigSetting<T>(optionName, configSectionName, defaultValue, throwsException);
                }
                else
                {
                    if (throwsException)
                        throw new KeyNotFoundException("Invalid config parameters provided. Expecting ConfigSection => \"CustomSectionName\".");
                }
            }
            else
            {
                // process an appsettings config value
                if (ConfigurationManager.AppSettings[optionName] != null)
                {
                    var val = System.Environment.ExpandEnvironmentVariables(ConfigurationManager.AppSettings[optionName]);
                    result = ConvertStringToNativeType<T>(val, defaultValue);
                }
                else
                {
                    if (throwsException)
                        throw new KeyNotFoundException($"The configuration key '{optionName}' does not exist.");
                }
            }

            return result;
        }

        private static object GetWebConfigSetting(Type valueType, string optionName, object defaultValue, bool throwsException = false, params Expression<Func<object, object>>[] configParameters)
        {
            var result = defaultValue;

            if (configParameters != null && configParameters.Length > 0)
            {
                // process a custom config section
                var configSectionName = configParameters.GetExpressionValue("SectionName");

                if (!string.IsNullOrEmpty(configSectionName))
                {
                    return GetWebConfigSetting(valueType, optionName, configSectionName, defaultValue, throwsException);
                }
                else
                {
                    if (throwsException)
                        throw new KeyNotFoundException("Invalid config parameters provided. Expecting ConfigSection => \"CustomSectionName\".");
                }
            }
            else
            {
                // process an appsettings config value
                if (ConfigurationManager.AppSettings[optionName] != null)
                {
                    var val = System.Environment.ExpandEnvironmentVariables(ConfigurationManager.AppSettings[optionName]);
                    result = ConvertStringToNativeType(valueType, val, defaultValue);
                }
                else
                {
                    if (throwsException)
                        throw new KeyNotFoundException($"The configuration key '{optionName}' does not exist.");
                }
            }

            return result;
        }

        private static T GetEmbeddedResourceSetting<T>(string optionName, T defaultValue, bool throwsException = false, params Expression<Func<object, object>>[] configParameters)
        {
            var result = defaultValue;

            var assembly = Assembly.GetExecutingAssembly();
            var rm = new ResourceManager("items", assembly)
            {
                IgnoreCase = true
            };

            string val;
            try
            {

                val = rm.GetString(optionName);
            }
            catch (MissingManifestResourceException)
            {
                // no value available
                if (throwsException)
                    throw new KeyNotFoundException($"The configuration key '{optionName}' does not exist.");
                return result;
            }

            return ConvertStringToNativeType<T>(val, defaultValue);
        }

        private static object GetEmbeddedResourceSetting(Type valueType, string optionName, object defaultValue, bool throwsException = false, params Expression<Func<object, object>>[] configParameters)
        {
            var result = defaultValue;

            var assembly = Assembly.GetExecutingAssembly();
            var rm = new ResourceManager("items", assembly)
            {
                IgnoreCase = true
            };

            string val;
            try
            {

                val = rm.GetString(optionName);
            }
            catch (MissingManifestResourceException)
            {
                // no value available
                if (throwsException)
                    throw new KeyNotFoundException($"The configuration key '{optionName}' does not exist.");
                return result;
            }

            return ConvertStringToNativeType(valueType, val, defaultValue);
        }

        private static T GetJsonConfigSetting<T>(string optionName, T defaultValue, bool throwsException = false, params Expression<Func<object, object>>[] configParameters)
        {
            return (T)GetJsonConfigSetting(typeof(T), optionName, defaultValue, throwsException, configParameters);
        }

        private static object GetJsonConfigSetting(Type valueType, string optionName, object defaultValue, bool throwsException = false, params Expression<Func<object, object>>[] configParameters)
        {
            var json = string.Empty;
            var result = defaultValue;
            var filename = Path.GetFullPath(configParameters.GetExpressionValue("Filename"));
            if (_cachedConfigurationFiles.ContainsKey(filename))
            {
                json = _cachedConfigurationFiles[filename];
            }
            else
            {
                if (File.Exists(filename))
                {
                    LastResolvedConfigurationFilename = filename;
                    json = File.ReadAllText(filename);
                    _cachedConfigurationFiles.Add(filename, json);
                }
                else if (throwsException)
                {
                    throw new ConfigurationMissingException($"Configuration file '{filename}' not found.");
                }
            }
            if (!string.IsNullOrEmpty(optionName))
            {
                var parser = new JsonParser();
                var objects = parser.Parse(json);
                var val = objects.SelectValueByName(optionName);
                result = ConvertStringToNativeType(valueType, val, defaultValue);
            }
            else
            {
                // mapping an entire object
                return JsonSerializer.Deserialize(json, valueType);
            }
            return result;
        }

        private static T GetXmlConfigSetting<T>(string optionName, T defaultValue, bool throwsException = false, params Expression<Func<object, object>>[] configParameters)
        {
            return (T)GetXmlConfigSetting(typeof(T), optionName, defaultValue, throwsException, configParameters);
        }

        private static object GetXmlConfigSetting(Type valueType, string optionName, object defaultValue, bool throwsException = false, params Expression<Func<object, object>>[] configParameters)
        {
            var xml = string.Empty;
            var result = defaultValue;
            var filenameExpressionValue = configParameters.GetExpressionValue("Filename");
            var filename = Path.GetFullPath(filenameExpressionValue);
            if (_cachedConfigurationFiles.ContainsKey(filename))
            {
                xml = _cachedConfigurationFiles[filename];
            }
            else
            {
                if (File.Exists(filename))
                {
                    LastResolvedConfigurationFilename = filename;
                    xml = File.ReadAllText(filename);
                    if (string.IsNullOrEmpty(xml))
                        return null;
                    _cachedConfigurationFiles.Add(filename, xml);
                }
                else if (throwsException)
                {
                    throw new ConfigurationMissingException($"Configuration file '{filename}' not found.");
                }
            }

            if (!string.IsNullOrEmpty(optionName) && !string.IsNullOrEmpty(xml))
            {
                var parser = new XmlParser();
                var objects = parser.Parse(xml);
                var val = objects?.SelectValueByName(optionName);
                if (val == null && objects != null)
                {
                    // if no value is found, try selecting it from appSettings nodes
                    var node = objects
                        .SelectNodeByName("appSettings", StringComparison.InvariantCultureIgnoreCase)
                        ?.QueryNodes(x => x.Name?.Equals("add", StringComparison.InvariantCultureIgnoreCase) == true
                            && ((XmlNode)x).Attributes?.Any(y => y.Name?.Equals("key", StringComparison.InvariantCultureIgnoreCase) == true && y.Value?.Equals(optionName, StringComparison.InvariantCultureIgnoreCase) == true) == true)
                        .FirstOrDefault();
                    if (node != null)
                    {
                        val = node.As<XmlNode>()?.Attributes
                            .Where(x => x.Name?.Equals("value", StringComparison.InvariantCultureIgnoreCase) == true)
                            .Select(x => x.Value)
                            .FirstOrDefault();
                    }
                }
                if (val == null && throwsException)
                    throw new ConfigurationMissingException($"The configuration key '{optionName}' does not exist.");
                result = ConvertStringToNativeType(valueType, val, defaultValue);
            }
            else if (!string.IsNullOrEmpty(xml))
            {
                // mapping an entire object
                return XmlSerializer.Deserialize(xml, valueType);
            }

            return result;
        }

        /// <summary>
        /// Convert a string to a native type using parsing or casting.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private static T ConvertStringToNativeType<T>(string val, T defaultValue)
        {
            var result = defaultValue;
            var type = typeof(T);
            var isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            // support nullable types
            if (isNullable)
            {
                // if type is nullable and value is empty, return null (undefined)
                if (string.IsNullOrEmpty(val))
                    return result;
                type = Nullable.GetUnderlyingType(type);
            }
            if (type == typeof(string))
                result = (T)(object)val;
            else if (type == typeof(bool))
            {
                var boolval = val.Trim().Equals("true", StringComparison.InvariantCultureIgnoreCase)
                    || val.Trim().Equals("1");
                result = (T)(object)boolval;
            }
            else if (type == typeof(float))
            {
                if (float.TryParse(val, out var fltval))
                    result = (T)(object)fltval;
            }
            else if (type == typeof(double))
            {
                if (double.TryParse(val, out var dblval))
                    result = (T)(object)dblval;
            }
            else if (type == typeof(decimal))
            {
                if (decimal.TryParse(val, out var decval))
                    result = (T)(object)decval;
            }
            else if (type == typeof(int))
            {
                if (int.TryParse(val, out var intval))
                    result = (T)(object)intval;
            }
            else if (type == typeof(byte))
            {
                if (byte.TryParse(val, out var byteval))
                    result = (T)(object)byteval;
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                // generic list
                var genericArgumentType = type.GenericTypeArguments.First();
                var values = val.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
                var genericListType = typeof(List<>).MakeGenericType(genericArgumentType);
                var instanceOf = Activator.CreateInstance(genericListType);

                foreach (var value in values)
                    genericListType.GetMethod("Add").Invoke(instanceOf, new object[] { ConvertStringToNativeType(genericArgumentType, value) });
                result = (T)(object)instanceOf;
            }
            return result;
        }

        /// <summary>
        /// Convert a string to a native type using parsing or casting.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        internal static object ConvertStringToNativeType(Type type, string val, object defaultValue)
        {
            var result = defaultValue;
            var isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);

            // support nullable types
            if (isNullable)
            {
                // if type is nullable and value is empty, return null (undefined)
                if (val == null)
                    return result;
                type = Nullable.GetUnderlyingType(type);
            }
            // support enums
            if (type.IsEnum && val != null)
                result = Convert.ChangeType(Enum.Parse(type, val), type);

            // support built-in types
            if (type == typeof(string))
                result = (object)val;
            else if (type == typeof(bool))
            {
                if (val == null)
                    result = defaultValue;
                else {
                    var boolval = val.Trim().Equals("true", StringComparison.InvariantCultureIgnoreCase)
                        || val.Trim().Equals("1");
                    result = boolval;
                }
            }
            else if (type == typeof(float))
            {
                if (float.TryParse(val, out var fltval))
                    result = fltval;
            }
            else if (type == typeof(double))
            {
                if (double.TryParse(val, out var dblval))
                    result = dblval;
            }
            else if (type == typeof(decimal))
            {
                if (decimal.TryParse(val, out var decval))
                    result = decval;
            }
            else if (type == typeof(int))
            {
                if (int.TryParse(val, out var intval))
                    result = intval;
            }
            else if (type == typeof(byte))
            {
                if (byte.TryParse(val, out var byteval))
                    result = byteval;
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                // generic list
                var genericArgumentType = type.GenericTypeArguments.First();
                var values = val.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
                var genericListType = typeof(List<>).MakeGenericType(genericArgumentType);
                var instanceOf = Activator.CreateInstance(genericListType);

                foreach (var value in values)
                    genericListType.GetMethod("Add").Invoke(instanceOf, new object[] { ConvertStringToNativeType(genericArgumentType, value) });
                result = instanceOf;
            }
            return result;
        }

        private static object ConvertStringToNativeType(Type t, string val)
        {
            object result = null;
            if (t == typeof(string))
                result = val;
            else if (t == typeof(bool))
            {
                var boolval = val.Trim().Equals("true", StringComparison.InvariantCultureIgnoreCase)
                    || val.Trim().Equals("1");
                result = boolval;
            }
            else if (t == typeof(float))
            {
                float.TryParse(val, out var fltval);
                result = fltval;
            }
            else if (t == typeof(double))
            {
                double.TryParse(val, out var dblval);
                result = dblval;
            }
            else if (t == typeof(decimal))
            {
                decimal.TryParse(val, out var decval);
                result = decval;
            }
            else if (t == typeof(int))
            {
                int.TryParse(val, out var intval);
                result = intval;
            }
            else if (t == typeof(byte))
            {
                byte.TryParse(val, out var byteval);
                result = byteval;
            }
            return result;
        }

        /// <summary>
        /// Get the value of an expression of type Func<object, object>
        /// </summary>
        /// <param name="expressions"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string GetExpressionValue(this Expression<Func<object, object>>[] expressions, string name)
        {
            var val = "";
            foreach (var expression in expressions)
            {
                if (expression.Parameters[0].Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    switch (expression.Body)
                    {
                        case BlockExpression e:
                            break;
                        case DefaultExpression e:
                            break;
                        case DynamicExpression e:
                            break;
                        case MemberExpression e:
                            var m = e.Expression as ConstantExpression;
                            val = m.Value.GetFieldValue(e.Member.Name).ToString();
                            break;
                        case MethodCallExpression e:
                            break;
                        case LambdaExpression e:
                            break;
                        case ConstantExpression e:
                            val = e.Value.ToString().Replace("\"", "");
                            break;
                        case UnaryExpression e:
                            break;
                    }
                    break;
                }
            }
            return val;
        }
    }
}
