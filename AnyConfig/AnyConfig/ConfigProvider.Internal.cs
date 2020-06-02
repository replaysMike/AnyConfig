using AnyConfig.Exceptions;
using AnyConfig.Json;
using AnyConfig.Xml;
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
using System.Threading;
using TypeSupport.Extensions;

[assembly: InternalsVisibleTo("AnyConfig.Tests")]
namespace AnyConfig
{
    public static partial class ConfigProvider
    {
        private const string DotNetCoreSettingsFilename = "appsettings.json";
        private const string DotNetFrameworkSettingsFilename = "App.config";
        private static readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);
        private static readonly Dictionary<string, string> _cachedConfigurationFiles = new Dictionary<string, string>();

        private static bool InternalTryGet(out object value, Type valueType, string optionName, ConfigSource configSource, object defaultValue, bool throwsException = false, params Expression<Func<object, object>>[] configParameters)
        {
            var valueExists = false;
            value = defaultValue;

            switch (configSource)
            {
                case ConfigSource.WebConfig:
                    // Standard web.config file
                    valueExists = GetWebConfigSetting(out value, valueType, optionName, defaultValue, throwsException, configParameters);
                    break;
                case ConfigSource.ApplicationConfig:
                    // Standard app.config file
                    valueExists = GetWebConfigSetting(out value, valueType, optionName, defaultValue, throwsException, configParameters);
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

        private static bool InternalTryGet<T>(out T value, string optionName, ConfigSource configSource, T defaultValue, bool throwsException = false, params Expression<Func<object, object>>[] configParameters)
        {
            var valueExists = false;
            value = defaultValue;

            switch (configSource)
            {
                case ConfigSource.WebConfig:
                case ConfigSource.ApplicationConfig:
                    // Standard .net config file
                    valueExists = GetWebConfigSetting<T>(out value, optionName, defaultValue, throwsException, configParameters);
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

        internal static bool GetWebConfigSetting<T>(out T value, string optionName, string configSectionName, T defaultValue, bool throwsException = false)
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
                        var valueAsString = System.Environment.ExpandEnvironmentVariables(config[optionName]);
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

        internal static bool GetWebConfigSetting(out object value, Type valueType, string optionName, string configSectionName, object defaultValue, bool throwsException = false)
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
                        var valueAsString = System.Environment.ExpandEnvironmentVariables(config[optionName]);
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

        internal static bool GetWebConfigSetting<T>(out T value, string optionName, T defaultValue, bool throwsException = false, params Expression<Func<object, object>>[] configParameters)
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
                if (ConfigurationManager.AppSettings[optionName] != null)
                {
                    var valueAsString = System.Environment.ExpandEnvironmentVariables(ConfigurationManager.AppSettings[optionName]);
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

        internal static bool GetWebConfigSetting(out object value, Type valueType, string optionName, object defaultValue, bool throwsException = false, params Expression<Func<object, object>>[] configParameters)
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
                if (ConfigurationManager.AppSettings[optionName] != null)
                {
                    var valueAsString = System.Environment.ExpandEnvironmentVariables(ConfigurationManager.AppSettings[optionName]);
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
            _cacheLock.Wait();
            try
            {
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
            }
            finally
            {
                _cacheLock.Release();
            }
            if (!string.IsNullOrEmpty(optionName))
            {
                var parser = new JsonParser();
                var objects = parser.Parse(json);
                var valueAsString = objects.SelectValueByName(optionName);
                value = ConvertStringToNativeType(valueType, valueAsString, defaultValue);
                valueExists = value != null && value != ConfigProvider.Empty;
            }
            else
            {
                // mapping an entire object
                value = JsonSerializer.Deserialize(json, valueType);
                valueExists = value != null && value != ConfigProvider.Empty;
                return valueExists;
            }
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

            _cacheLock.Wait();
            try
            {
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
                        {
                            return false;
                        }
                        _cachedConfigurationFiles.Add(filename, xml);
                    }
                    else if (throwsException)
                    {
                        throw new ConfigurationMissingException($"Configuration file '{filename}' not found.");
                    }
                }
            }
            finally
            {
                _cacheLock.Release();
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
                value = ConvertStringToNativeType(valueType, val, defaultValue);
                valueExists = value != null && value != ConfigProvider.Empty;
            }
            else if (!string.IsNullOrEmpty(xml))
            {
                // mapping an entire object
                value = XmlSerializer.Deserialize(xml, valueType);
                valueExists = value != null && value != ConfigProvider.Empty;
            }

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

            // support built-in types
            if (type == typeof(string))
                result = (object)value;
            else if (type == typeof(bool))
            {
                if (value == null)
                    result = defaultValue;
                else
                {
                    var boolval = value.Trim().Equals("true", StringComparison.InvariantCultureIgnoreCase)
                        || value.Trim().Equals("1");
                    result = boolval;
                }
            }
            else if (type == typeof(float))
            {
                if (float.TryParse(value, out var fltval))
                    result = fltval;
            }
            else if (type == typeof(double))
            {
                if (double.TryParse(value, out var dblval))
                    result = dblval;
            }
            else if (type == typeof(decimal))
            {
                if (decimal.TryParse(value, out var decval))
                    result = decval;
            }
            else if (type == typeof(int))
            {
                if (int.TryParse(value, out var intval))
                    result = intval;
            }
            else if (type == typeof(byte))
            {
                if (byte.TryParse(value, out var byteval))
                    result = byteval;
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                // generic list
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
