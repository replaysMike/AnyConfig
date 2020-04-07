﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace AnyConfig.Json
{
    /// <summary>
    /// A JSON node element
    /// </summary>
    public class JsonNode
    {
        private JsonFormatter _jsonFormatter;
        
        /// <summary>
        /// The name of the node
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// The type of node
        /// </summary>
        public JsonNodeType NodeType { get; set; }
        
        /// <summary>
        /// The position in the raw string this node begins
        /// </summary>
        public int OpenPosition { get; set; }
        
        /// <summary>
        /// The position in the raw string this node ends
        /// </summary>
        public int ClosePosition { get; set; }
        
        /// <summary>
        /// The raw length of the string this node occupies
        /// </summary>
        public int Length => ClosePosition - OpenPosition;
        
        /// <summary>
        /// All child nodes inherited by this node
        /// </summary>
        public List<JsonNode> ChildNodes { get; set; }
        
        /// <summary>
        /// The parent mode
        /// </summary>
        public JsonNode ParentNode { get; set; }
        
        /// <summary>
        /// The value of the node
        /// </summary>
        public string Value { get; set; }
        
        /// <summary>
        /// The data type of the value
        /// </summary>
        public PrimitiveTypes ValueType { get; set; }
        
        /// <summary>
        /// The raw string that makes up the node
        /// </summary>
        public string OriginalText { get; set; }
        
        /// <summary>
        /// True if this node has been validated
        /// </summary>
        public bool IsValidated { get; set; }

        /// <summary>
        /// True if this node has been defined
        /// </summary>
        public bool IsDefined { get; set; }

        /// <summary>
        /// If a child of an array, indicates the array element position it belongs in.
        /// </summary>
        public int? ArrayPosition { get; set; }

        /// <summary>
        /// If this node had an external ref, store it here
        /// </summary>
        public string ExternalPathLocation { get; set; }

        /// <summary>
        /// For array types, get all values for the array
        /// </summary>
        public List<string> ArrayValues
        {
            get
            {
                if (NodeType == JsonNodeType.Array)
                {
                    return ChildNodes.Select(x => x.Value).ToList();
                }

                return null;
            }
        }

        public string FullPathWithArrayHints
        {
            get
            {
                var buildPath = string.Empty;
                buildPath += GetParentPath(buildPath, this, true);
                return buildPath;
            }
        }

        public string FullPath
        {
            get
            {
                var buildPath = string.Empty;
                buildPath += GetParentPath(buildPath, this);
                return buildPath;
            }
        }

        public JsonNode() : this(string.Empty, JsonNodeType.Object)
        {
        }

        public JsonNode(string name) : this(name, JsonNodeType.Object)
        {
        }

        public JsonNode(JsonNodeType nodeType)
            : this("", nodeType)
        {
        }

        public JsonNode(string name, JsonNodeType nodeType)
        {
            _jsonFormatter = new JsonFormatter();
            Name = name;
            NodeType = nodeType;
            ChildNodes = new List<JsonNode>();
        }

        /// <summary>
        /// Select a node by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public JsonNode SelectNodeByName(string name)
        {
            return ChildNodes.Where(x => x.Name == name).FirstOrDefault();
        }

        /// <summary>
        /// Select a child node's value by its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string SelectValueByName(string name)
        {
            return ChildNodes.Where(x => x.Name == name).Select(y => y.Value).FirstOrDefault();
        }

        /// <summary>
        /// Map a child node to another object
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="lambda"></param>
        /// <returns></returns>
        public TResult SelectChild<TResult>(Func<JsonNode, TResult> lambda)
        {
            return lambda(this);
        }

        /// <summary>
        /// Map children nodes to another object
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="lambda"></param>
        /// <returns></returns>
        public IEnumerable<TResult> SelectChildren<TResult>(Func<JsonNode, IEnumerable<TResult>> lambda)
        {
            return lambda(this);
        } 

        public string ToJsonString()
        {
            return _jsonFormatter.ToJsonString(this);
        }

        /// <summary>
        /// Traverse the parent structure of the object and compute the full path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="node"></param>
        /// <param name="withArrayHints">True to show the array position</param>
        /// <returns></returns>
        private string GetParentPath(string path, JsonNode node, bool withArrayHints = false)
        {
            if (node != null)
            {
                if (string.IsNullOrEmpty(node.Name))
                {
                    // show the array element position if requested
                    if (withArrayHints)
                    {
                        if (node.ParentNode != null && node.ParentNode.ValueType == PrimitiveTypes.Array)
                        {
                            if (node.ArrayPosition != null) // just in case
                                path = "[" + node.ArrayPosition.Value + "]" + path;
                            else
                                path = "[]" + path;
                        }
                        else
                        {

                        }
                    }
                }
                else
                {
                    path = "/" + node.Name + path;
                }
                if (node.ParentNode != null)
                    path = GetParentPath(path, node.ParentNode, withArrayHints);
            }
            return path;
        }

        public override string ToString() => string.Format("{3}{4}{0} {1}, {2} children.", string.IsNullOrEmpty(Name) ? "()" : Name, NodeType.ToString(), ChildNodes.Count, IsDefined ? "" : "!", IsValidated ? "" : "*");
    }
}
