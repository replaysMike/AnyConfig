﻿using AnyConfig.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AnyConfig
{
    /// <summary>
    /// AnyConfig configuration section
    /// </summary>
    public class ConfigurationSection : IConfigurationSection
    {
        private INode _node;

        public string this[string key]
        {
            get
            {
                return _node?.SelectValueByName(key, StringComparison.InvariantCultureIgnoreCase);
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

        public ConfigurationSection(string path, string key, string value, INode rootNode)
        {
            Path = ConvertToSectionPath(path);
            Key = key;
            Value = value;
            _node = rootNode;
        }

        /// <summary>
        /// Get the text that makes up the section
        /// </summary>
        /// <returns></returns>
        public string GetNodeStructuredText()
        {
            if (_node == null)
                return string.Empty;
            var path = ConvertToJsonPath(Path);
            var innerNode = _node.SelectNodeByPath(path);
            return innerNode?.OuterText;
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            if (_node == null)
                return new List<IConfigurationSection>();
            var _jsonNode = _node as JsonNode;
            // if the root node is an object, get the node inside it
            if (_jsonNode.NodeType == JsonNodeType.Object)
            {
                var path = ConvertToJsonPath(Path);
                var parentNode = _node.SelectNodeByPath(path, StringComparison.InvariantCultureIgnoreCase) as JsonNode;
                if (parentNode == null)
                    throw new InvalidOperationException($"Could not find configuration node in path '{path}'");
                switch (parentNode.NodeType)
                {
                    case JsonNodeType.Array:
                        return parentNode.ArrayNodes
                            .Select(x => new ConfigurationSection(x.FullPathWithArrayHints, x.ArrayPosition.ToString(), x.Value, _node))
                            .OrderBy(x => x.Key);
                    case JsonNodeType.Object:
                    default:
                        return parentNode.ChildNodes
                            .Select(x => new ConfigurationSection(x.FullPathWithArrayHints, x.Name, x.Value, _node))
                            .OrderBy(x => x.Key);
                }
            }
            return _jsonNode.ChildNodes
                .Select(x => new ConfigurationSection(x.FullPathWithArrayHints, x.Name, x.Value, _node))
                .OrderBy(x => x.Key);
        }

        IEnumerable<IConfigurationSection> IConfiguration.GetChildren() => GetChildren();

        public IChangeToken GetReloadToken() => new ConfigurationReloadToken();

        IChangeToken IConfiguration.GetReloadToken() => GetReloadToken();

        public IConfigurationSection GetSection(string key)
        {
            var path = ConvertToJsonPath(Path);
            var parentNode = _node?.SelectNodeByPath(path, StringComparison.InvariantCultureIgnoreCase) as JsonNode;
            if (parentNode != null)
            {
                var node = parentNode.SelectNodeByName(key, StringComparison.InvariantCultureIgnoreCase) as JsonNode;
                if (node != null)
                {
                    switch (node.NodeType)
                    {
                        case JsonNodeType.Value:
                            return new ConfigurationSection(node.FullPathWithArrayHints, node.Name, node.Value, _node);
                        case JsonNodeType.Array:
                            return new ConfigurationSection(node.FullPathWithArrayHints, node.Name, node.Value, _node);
                        case JsonNodeType.Object:
                        default:
                            return new ConfigurationSection(node.FullPathWithArrayHints, node.Name, node.Value, _node);
                    }
                }
            }

            // always return a configuration section
            return new ConfigurationSection(key, key, null, null);
        }

        IConfigurationSection IConfiguration.GetSection(string key) => GetSection(key);

        internal string ConvertToJsonPath(string path)
        {
            var newPath = path.Replace(":", "/");
            if (!newPath.StartsWith("/"))
                newPath = "/" + newPath;
            return NodeHelpers.RemapINodeArrayPositionText(newPath);
        }

        internal string ConvertToSectionPath(string path)
        {
            var newPath = path.Replace("/", ":");
            if (newPath.StartsWith(":"))
                newPath = newPath.Substring(1);

            return NodeHelpers.RemapIConfigurationArrayPositionText(newPath);
        }

        public override string ToString() => Key;
    }
}
