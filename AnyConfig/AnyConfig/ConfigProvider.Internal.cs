using AnyConfig.Exceptions;
using AnyConfig.Json;
using AnyConfig.Xml;
using System;
using System.Collections.Concurrent;
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
using TypeSupport.Extensions;

[assembly: InternalsVisibleTo("AnyConfig.Tests,PublicKey=00240000048000009400000006020000002400005253413100040000010001001996994e108c546699147c2adcce1926f2c7045588113cdccfe7c8a0c87830ac7f23347fe7ee39c65ded9cef5a82568da5dd2329434c20912db075e4af8ecbf162f108d3afadd0215a17b6ccdbf58b3501244c56a853b22e243d5b731daf9810394ae16bc1511937bbf764d3c344fcf2a31ff59fe9375ad93ed5932147ad7d9c")]
namespace AnyConfig
{
    public static partial class ConfigProvider
    {
        private const string DotNetCoreSettingsFilename = "appsettings.json";
        private const string DotNetFrameworkSettingsFilename = "App.config";
        private static readonly ConcurrentDictionary<string, CachedConfiguration> _cachedConfigurationFiles = new ConcurrentDictionary<string, CachedConfiguration>();
        private static readonly ConcurrentDictionary<ObjectCacheKey, object> _cachedConfigurationValues = new ConcurrentDictionary<ObjectCacheKey, object>();

        private static bool InternalTryGet(out object value, Type valueType, string optionName, ConfigSource configSource, object defaultValue, bool throwsException = false, bool expandEnvironmentVariables = false, params Expression<Func<object, object>>[] configParameters)
        {
            var valueExists = false;
            value = defaultValue;

            switch (configSource)
            {
                case ConfigSource.WebConfig:
                    // Standard web.config file
                    valueExists = GetWebConfigSetting(out value, valueType, optionName, defaultValue, throwsException, expandEnvironmentVariables, configParameters);
                    break;
                case ConfigSource.ApplicationConfig:
                    // Standard app.config file
                    valueExists = GetWebConfigSetting(out value, valueType, optionName, defaultValue, throwsException, expandEnvironmentVariables, configParameters);
                    break;
                case ConfigSource.EmbeddedResource:
                    // embedded resource dictionary value
                    valueExists = GetEmbeddedResourceSetting(out value, valueType, optionName, defaultValue, throwsException, configParameters);
                    break;
                case ConfigSource.Registry:
                    // implement registry requirements
                    break;
                case ConfigSource.Custom:
                    // implement custom requirements
                    break;
                case ConfigSource.JsonFile:
                    // parse a Json file and load a config value
                    valueExists = GetJsonConfigSetting(out value, valueType, optionName, defaultValue, throwsException, configParameters);
                    break;
                case ConfigSource.XmlFile:
                    // parse a Xml file and load a config value
                    valueExists = GetXmlConfigSetting(out value, valueType, optionName, defaultValue, throwsException, configParameters);
                    break;
                default:
                    break;
            }

            return valueExists;
        }

        private static bool InternalTryGet<T>(out T value, string optionName, ConfigSource configSource, T defaultValue, bool throwsException = false, bool expandEnvironmentVariables = false, params Expression<Func<object, object>>[] configParameters)
        {
            var valueExists = false;
            value = defaultValue;

            switch (configSource)
            {
                case ConfigSource.WebConfig:
                case ConfigSource.ApplicationConfig:
                    // Standard .net config file
                    valueExists = GetWebConfigSetting<T>(out value, optionName, defaultValue, throwsException, expandEnvironmentVariables, configParameters);
                    break;
                case ConfigSource.EmbeddedResource:
                    // embedded resource dictionary value
                    valueExists = GetEmbeddedResourceSetting<T>(out value, optionName, defaultValue, throwsException, configParameters);
                    break;
                case ConfigSource.Registry:
                    // implement registry requirements
                    break;
                case ConfigSource.Custom:
                    // implement custom requirements
                    break;
                case ConfigSource.JsonFile:
                    // parse a Json file and load a config value
                    valueExists = GetJsonConfigSetting<T>(out value, optionName, defaultValue, throwsException, configParameters);
                    break;
                case ConfigSource.XmlFile:
                    // parse a Xml file and load a config value
                    valueExists = GetXmlConfigSetting<T>(out value, optionName, defaultValue, throwsException, configParameters);
                    break;
                default:
                    break;
            }

            return valueExists;
        }

        internal static bool GetWebConfigSetting<T>(out T value, string optionName, string configSectionName, T defaultValue, bool throwsException = false, bool expandEnvironmentVariables = false)
        {
            var valueExists = false;
            value = defaultValue;

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

                        var valueAsString = expandEnvironmentVariables ? System.Environment.ExpandEnvironmentVariables(config[optionName]) : config[optionName];
                        value = ConvertStringToNativeType<T>(valueAsString, defaultValue);
                        valueExists = true;
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

            return valueExists;
        }

        internal static bool GetWebConfigSetting(out object value, Type valueType, string optionName, string configSectionName, object defaultValue, bool throwsException = false, bool expandEnvironmentVariables = false)
        {
            var valueExists = false;
            value = defaultValue;

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
                        var valueAsString = expandEnvironmentVariables ? System.Environment.ExpandEnvironmentVariables(config[optionName]) : config[optionName];
                        value = ConvertStringToNativeType(valueType, valueAsString, defaultValue);
                        valueExists = true;
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

            return valueExists;
        }

        internal static bool GetWebConfigSetting<T>(out T value, string optionName, T defaultValue, bool throwsException = false, bool expandEnvironmentVariables = false, params Expression<Func<object, object>>[] configParameters)
        {
            var valueExists = false;
            value = defaultValue;

            if (configParameters != null && configParameters.Length > 0)
            {
                // process a custom config section
                var configSectionName = configParameters.GetExpressionValue("SectionName");

                if (!string.IsNullOrEmpty(configSectionName))
                {
                    return GetWebConfigSetting<T>(out value, optionName, configSectionName, defaultValue, throwsException);
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
                var valueFromConfig = ConfigurationManager.AppSettings[optionName];
                if (valueFromConfig != null)
                {
                    var valueAsString = expandEnvironmentVariables ? System.Environment.ExpandEnvironmentVariables(valueFromConfig.Value) : valueFromConfig.Value;
                    value = ConvertStringToNativeType<T>(valueAsString, defaultValue);
                }
                else
                {
                    if (throwsException)
                        throw new KeyNotFoundException($"The configuration key '{optionName}' does not exist.");
                }
            }

            return valueExists;
        }

        internal static bool GetWebConfigSetting(out object value, Type valueType, string optionName, object defaultValue, bool throwsException = false, bool expandEnvironmentVariables = false, params Expression<Func<object, object>>[] configParameters)
        {
            var valueExists = false;
            value = defaultValue;

            if (configParameters != null && configParameters.Length > 0)
            {
                // process a custom config section
                var configSectionName = configParameters.GetExpressionValue("SectionName");

                if (!string.IsNullOrEmpty(configSectionName))
                {
                    return GetWebConfigSetting(out value, valueType, optionName, configSectionName, defaultValue, throwsException);
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
                var valueFromConfig = ConfigurationManager.AppSettings[optionName];
                if (valueFromConfig != null)
                {
                    var valueAsString = expandEnvironmentVariables ? System.Environment.ExpandEnvironmentVariables(valueFromConfig.Value) : valueFromConfig.Value;
                    value = ConvertStringToNativeType(valueType, valueAsString, defaultValue);
                    valueExists = value != null && value != ConfigProvider.Empty;
                }
                else
                {
                    if (throwsException)
                        throw new KeyNotFoundException($"The configuration key '{optionName}' does not exist.");
                }
            }

            return valueExists;
        }

        internal static bool GetEmbeddedResourceSetting<T>(out T value, string optionName, T defaultValue, bool throwsException = false, params Expression<Func<object, object>>[] configParameters)
        {
            var valueExists = false;
            value = defaultValue;

            var assembly = Assembly.GetExecutingAssembly();
            var rm = new ResourceManager("items", assembly)
            {
                IgnoreCase = true
            };
            try
            {

                var valueAsString = rm.GetString(optionName);
                value = ConvertStringToNativeType<T>(valueAsString, defaultValue);
                valueExists = value != null;
            }
            catch (MissingManifestResourceException)
            {
                // no value available
                if (throwsException)
                    throw new KeyNotFoundException($"The configuration key '{optionName}' does not exist.");
            }

            return valueExists;
        }

        internal static bool GetEmbeddedResourceSetting(out object value, Type valueType, string optionName, object defaultValue, bool throwsException = false, params Expression<Func<object, object>>[] configParameters)
        {
            var valueExists = false;
            value = defaultValue;

            var assembly = Assembly.GetExecutingAssembly();
            var rm = new ResourceManager("items", assembly)
            {
                IgnoreCase = true
            };

            try
            {
                var valueAsString = rm.GetString(optionName);
                value = ConvertStringToNativeType(valueType, valueAsString, defaultValue);
                valueExists = value != null && value != ConfigProvider.Empty;
            }
            catch (MissingManifestResourceException)
            {
                // no value available
                if (throwsException)
                    throw new KeyNotFoundException($"The configuration key '{optionName}' does not exist.");
            }

            return valueExists;
        }

        internal static bool GetJsonConfigSetting<T>(out T value, string optionName, T defaultValue, bool throwsException = false, params Expression<Func<object, object>>[] configParameters)
        {
            var exists = GetJsonConfigSetting(out object result, typeof(T), optionName, defaultValue, throwsException, configParameters);
            value = (T)result;
            return exists;
        }

        internal static bool GetJsonConfigSetting(out object value, Type valueType, string optionName, object defaultValue, bool throwsException = false, params Expression<Func<object, object>>[] configParameters)
        {
            var valueExists = false;
            value = defaultValue;

            var json = string.Empty;
            var filename = Path.GetFullPath(configParameters.GetExpressionValue("Filename"));
            var cacheKey = new ObjectCacheKey
            {
                Filename = filename,
                OptionName = optionName,
                Type = valueType
            };

            if (_cachedConfigurationValues.ContainsKey(cacheKey))
            {
                value = _cachedConfigurationValues[cacheKey];
                return value != null && value != ConfigProvider.Empty;
            }
            var cachedConfiguration = new CachedConfiguration();

            if (_cachedConfigurationFiles.ContainsKey(filename))
            {
                // use json from cache
                json = _cachedConfigurationFiles[filename].OriginalText;
            }
            else
            {
                // load json from file
                if (File.Exists(filename))
                {
                    LastResolvedConfigurationFilename = filename;
                    json = File.ReadAllText(filename);
                    cachedConfiguration = new CachedConfiguration { OriginalText = json };
                    _cachedConfigurationFiles.AddOrUpdate(filename, cachedConfiguration, (key, existingValue) => existingValue);
                }
                else if (throwsException)
                {
                    throw new ConfigurationMissingException($"Configuration file '{filename}' not found.");
                }
            }

            if (!string.IsNullOrEmpty(optionName))
            {
                if (cachedConfiguration.RootNode == null)
                {
                    // parse the json for the first time
                    var parser = new JsonParser();
                    // cache the parsed root node
                    cachedConfiguration.RootNode = parser.Parse(json);
                }

                var valueAsString = cachedConfiguration.RootNode.SelectValueByName(optionName);
                value = ConvertStringToNativeType(valueType, valueAsString, defaultValue);
                valueExists = value != null && value != ConfigProvider.Empty;
            }
            else
            {
                // mapping an entire object
                value = JsonSerializer.Deserialize(json, valueType);
                valueExists = value != null && value != ConfigProvider.Empty;
            }

            // cache the result
            _cachedConfigurationValues.AddOrUpdate(cacheKey, value, (key, existingValue) => existingValue);

            return valueExists;
        }

        internal static bool GetXmlConfigSetting<T>(out T value, string optionName, T defaultValue, bool throwsException = false, params Expression<Func<object, object>>[] configParameters)
        {
            var valueExists = GetXmlConfigSetting(out object result, typeof(T), optionName, defaultValue, throwsException, configParameters);
            value = (T)result;
            return valueExists;
        }

        internal static bool GetXmlConfigSetting(out object value, Type valueType, string optionName, object defaultValue, bool throwsException = false, params Expression<Func<object, object>>[] configParameters)
        {
            var valueExists = false;
            value = defaultValue;

            var xml = string.Empty;
            var filenameExpressionValue = configParameters.GetExpressionValue("Filename");
            var filename = Path.GetFullPath(filenameExpressionValue);
            var cacheKey = new ObjectCacheKey
            {
                Filename = filename,
                OptionName = optionName,
                Type = valueType
            };

            if (_cachedConfigurationValues.ContainsKey(cacheKey))
            {
                value = _cachedConfigurationValues[cacheKey];
                return value != null && value != ConfigProvider.Empty;
            }

            var cachedConfiguration = new CachedConfiguration();

            if (_cachedConfigurationFiles.ContainsKey(filename))
            {
                // use xml from cache
                xml = _cachedConfigurationFiles[filename].OriginalText;
            }
            else
            {
                // load xml from file
                if (File.Exists(filename))
                {
                    LastResolvedConfigurationFilename = filename;
                    xml = File.ReadAllText(filename);
                    if (string.IsNullOrEmpty(xml))
                    {
                        return false;
                    }
                    cachedConfiguration = new CachedConfiguration { OriginalText = xml };
                    _cachedConfigurationFiles.AddOrUpdate(filename, cachedConfiguration, (key, existingValue) => existingValue);
                }
                else if (throwsException)
                {
                    throw new ConfigurationMissingException($"Configuration file '{filename}' not found.");
                }
            }

            if (!string.IsNullOrEmpty(optionName) && !string.IsNullOrEmpty(xml))
            {
                if (cachedConfiguration.RootNode == null)
                {
                    // parse the xml for the first time
                    var parser = new XmlParser();
                    // cache the parsed root node
                    cachedConfiguration.RootNode = parser.Parse(xml);
                }
                var val = cachedConfiguration.RootNode?.SelectValueByName(optionName);
                if (val == null && cachedConfiguration.RootNode != null)
                {
                    // if no value is found, try selecting it from appSettings nodes
                    var node = cachedConfiguration.RootNode
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
                value = ConvertStringToNativeType(valueType, val, defaultValue);
                valueExists = value != null && value != ConfigProvider.Empty;
            }
            else if (!string.IsNullOrEmpty(xml))
            {
                // mapping an entire object
                value = XmlSerializer.Deserialize(xml, valueType);
                valueExists = value != null && value != ConfigProvider.Empty;
            }

            // cache the result
            _cachedConfigurationValues.AddOrUpdate(cacheKey, value, (key, existingValue) => existingValue);


            return valueExists;
        }

        private static string InternalGetConnectionString(string name)
        {
            var connectionStringKey = ConfigurationManager.ConnectionStrings[name];
            if (connectionStringKey != null)
                return connectionStringKey.ConnectionString;
            else
                return null;
        }

        /// <summary>
        /// Convert a string to a native type using parsing or casting.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        internal static object ConvertStringToNativeType(Type type, string value, object defaultValue)
        {
            var result = defaultValue;
            var isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);

            // support nullable types
            if (isNullable)
            {
                // if type is nullable and value is empty, return null (undefined)
                if (value == null)
                    return result;
                type = Nullable.GetUnderlyingType(type);
            }
            // support enums
            if (type.IsEnum && value != null)
                result = Convert.ChangeType(Enum.Parse(type, value), type);

            // support built-in types. Type checks are in order of most used types, small optimization
            if (type == typeof(string))
                result = (object)value;
            else if (type == typeof(bool))
            {
                if (value == null)
                    result = defaultValue;
                else
                {
                    var boolval =
                        value.Trim().Equals("true", StringComparison.InvariantCultureIgnoreCase) // case insensitive
                        || value.Trim().Equals("1"); // support integer to bool
                    result = boolval;
                }
            }
            else if (type == typeof(int))
            {
                if (int.TryParse(value, out var intval))
                    result = intval;
            }
            else if (type == typeof(double))
            {
                if (double.TryParse(value, out var dblval))
                    result = dblval;
            }
            else if (type == typeof(long))
            {
                if (long.TryParse(value, out var longval))
                    result = longval;
            }
            else if (type == typeof(ulong))
            {
                if (ulong.TryParse(value, out var ulongval))
                    result = ulongval;
            }
            else if (type == typeof(uint))
            {
                if (uint.TryParse(value, out var uintval))
                    result = uintval;
            }
            else if (type == typeof(short))
            {
                if (short.TryParse(value, out var shortval))
                    result = shortval;
            }
            else if (type == typeof(ushort))
            {
                if (ushort.TryParse(value, out var ushortval))
                    result = ushortval;
            }
            else if (type == typeof(byte))
            {
                if (byte.TryParse(value, out var byteval))
                    result = byteval;
            }
            else if (type == typeof(sbyte))
            {
                if (sbyte.TryParse(value, out var sbyteval))
                    result = sbyteval;
            }
            else if (type == typeof(float))
            {
                if (float.TryParse(value, out var fltval))
                    result = fltval;
            }
            else if (type == typeof(decimal))
            {
                if (decimal.TryParse(value, out var decval))
                    result = decval;
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                // generic list support, comma delimited values
                var genericArgumentType = type.GenericTypeArguments.First();
                var values = value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
                var genericListType = typeof(List<>).MakeGenericType(genericArgumentType);
                var instanceOf = Activator.CreateInstance(genericListType);

                foreach (var val in values)
                    genericListType.GetMethod("Add").Invoke(instanceOf, new object[] { ConvertStringToNativeType(genericArgumentType, val) });
                result = instanceOf;
            }
            return result;
        }



        /// <summary>
        /// Convert a string to a native type using parsing or casting.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        internal static T ConvertStringToNativeType<T>(string value, T defaultValue)
            => (T)ConvertStringToNativeType(typeof(T), value, defaultValue);

        /// <summary>
        /// Convert a string to a native type using parsing or casting.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static object ConvertStringToNativeType(Type type, string value) => ConvertStringToNativeType(type, value, null);

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
    }
}
