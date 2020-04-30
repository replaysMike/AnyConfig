using AnyConfig.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnyConfig
{
    /// <summary>
    /// AnyConfig configuration section
    /// </summary>
    public class ConfigurationSection : IConfigurationSection
    {
        private JsonNode _jsonNode;

        public string this[string key]
        {
            get
            {
                return _jsonNode?.SelectValueByName(key);
            }
            set
            {
                // nothing to set
            }
        }

        /// <summary>
        /// The name of the ConfigurationSection
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// The path of the ConfigurationSection
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// The value of the entry
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The original text of the ConfigurationSection
        /// </summary>
        public string RawText { get; set; }

        public ConfigurationSection(string path, string key, string value, string rawText)
        {
            Path = FormatPath(path);
            Key = key;
            Value = value;
            RawText = rawText;
            if (!string.IsNullOrEmpty(rawText))
                ParseConfigurationSection(rawText);
        }

        private void ParseConfigurationSection(string json)
        {
            var parser = new JsonParser();
            _jsonNode = parser.Parse(json);
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            if (_jsonNode == null)
                return new List<IConfigurationSection>();
            // if the root node is an object, get the node inside it
            if (_jsonNode.NodeType == JsonNodeType.Object)
            {
                var parentNode = _jsonNode.SelectNodeByName(Key);
                switch (parentNode.NodeType)
                {
                    case JsonNodeType.Array:
                        return parentNode.ArrayValues
                            .Select(x => new ConfigurationSection(parentNode.FullPath, parentNode.Name, x, parentNode.OuterText))
                            .OrderBy(x => x.Key);
                    case JsonNodeType.Object:
                    default:
                        return parentNode.ChildNodes
                            .Select(x => new ConfigurationSection(x.FullPath, x.Name, x.Value, x.OuterText))
                            .OrderBy(x => x.Key);
                }
            }
            return _jsonNode.ChildNodes
                .Select(x => new ConfigurationSection(x.FullPath, x.Name, x.Value, x.OuterText))
                .OrderBy(x => x.Key);
        }

        public IChangeToken GetReloadToken()
        {
            throw new NotImplementedException();
        }

        public IConfigurationSection GetSection(string key)
        {
            var node = _jsonNode.SelectNodeByName(key);
            if (node != null)
            {
                switch (node.NodeType)
                {
                    case JsonNodeType.Value:
                        return new ConfigurationSection(node.FullPath, node.Name, node.Value, node.OuterText);
                    case JsonNodeType.Array:
                        return new ConfigurationSection(node.FullPath, node.Name, node.OuterText, node.OuterText);
                    case JsonNodeType.Object:
                    default:
                        return new ConfigurationSection(node.FullPath, node.Name, node.OuterText, node.OuterText);
                }
            }

            // always return a configuration section
            return new ConfigurationSection(key, key, null, null);
        }

        internal string FormatPath(string path)
        {
            var newPath = path.Replace("/", ":");
            if (newPath.StartsWith(":"))
                newPath = newPath.Substring(1);

            return newPath;
        }

        public override string ToString()
        {
            return Key;
        }
    }
}
