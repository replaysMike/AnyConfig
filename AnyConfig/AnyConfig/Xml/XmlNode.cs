using System;
using System.Collections.Generic;
using System.Linq;

namespace AnyConfig.Xml
{
    public class XmlNode : INode
    {
        private readonly XmlFormatter _xmlFormatter;

        /// <summary>
        /// The name of the node
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// CData string
        /// </summary>
        public string CData { get; set; }

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
        public List<INode> ChildNodes { get; set; } = new List<INode>();

        /// <summary>
        /// All child nodes inherited by this node
        /// </summary>
        public List<XmlAttribute> Attributes { get; set; } = new List<XmlAttribute>();

        /// <summary>
        /// The parent mode
        /// </summary>
        public INode ParentNode { get; set; }

        /// <summary>
        /// The XML declaration header
        /// </summary>
        public XmlNode DeclarationNode { get; set; }

        /// <summary>
        /// The value of the node
        /// </summary>
        public string Value => InnerText;

        /// <summary>
        /// The inner string that makes up the node without XML children
        /// </summary>
        public string InnerContent { get; set; }

        /// <summary>
        /// The inner string that makes up the node
        /// </summary>
        public string InnerText { get; set; }

        /// <summary>
        /// The outer string that makes up the node
        /// </summary>
        public string OuterText { get; set; }

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
        /// Get all values for an array
        /// </summary>
        public List<INode> ArrayNodes
        {
            get
            {
                return ChildNodes
                    .GroupBy(x => x.Name)
                    .Where(x => x.Count() > 1)
                    .SelectMany(x => x)
                    .ToList();
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
                if (string.IsNullOrEmpty(Name))
                    return string.Empty;

                var buildPath = string.Empty;
                buildPath += GetParentPath(buildPath, this);
                return buildPath;
            }
        }

        public XmlNode() : this(string.Empty)
        {
        }

        public XmlNode(string name)
        {
            _xmlFormatter = new XmlFormatter();
            Name = name;
        }

        /// <summary>
        /// Select a node by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public INode SelectNodeByName(string name) => SelectNodeByName(name, StringComparison.InvariantCulture);

        /// <summary>
        /// Select a node by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public INode SelectNodeByName(string name, StringComparison comparisonType)
        {
            var nodes = ChildNodes.SelectChildren(x => x.ChildNodes);
            var matches = nodes
                .Where(x => x.Name?.Equals(name, comparisonType) == true)
                .Select(x => x.As<XmlNode>());
            return matches.FirstOrDefault();
        }

        public INode SelectNodeByPath(string path)
            => SelectNodeByPath(path);

        public INode SelectNodeByPath(string path, StringComparison comparisonType)
        {
            var nodes = ChildNodes.SelectChildren(x => x.ChildNodes);
            var matches = nodes
                .Where(x => x.FullPath.Equals(path, comparisonType))
                .Select(x => x.As<XmlNode>());
            return matches
                .FirstOrDefault();
        }

        /// <summary>
        /// Select a child node's value by its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string SelectValueByName(string name) => SelectValueByName(name, StringComparison.InvariantCulture);

        /// <summary>
        /// Select a child node's value by its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string SelectValueByName(string name, StringComparison comparisonType)
        {
            var nodes = ChildNodes.SelectChildren(x => x.ChildNodes);
            var matches = nodes
                .Where(x => x.Name.Equals(name, comparisonType));
            return matches
                .Select(y => y.As<XmlNode>().Value)
                .FirstOrDefault();
        }

        /// <summary>
        /// Select a child node's value by its name
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string SelectValueByPath(string path) => SelectValueByPath(path, StringComparison.InvariantCulture);

        /// <summary>
        /// Select a child node's value by its name
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string SelectValueByPath(string path, StringComparison comparisonType)
        {
            var nodes = ChildNodes.SelectChildren(x => x.ChildNodes);
            var matches = nodes
                .Where(x => x.FullPath.Equals(path, comparisonType));
            return matches
                .Select(y => y.As<XmlNode>().Value)
                .FirstOrDefault();
        }

        /// <summary>
        /// Query child nodes
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public IEnumerable<INode> QueryNodes(Func<INode, bool> condition)
        {
            var nodes = ChildNodes?.SelectChildren(x => x.ChildNodes)
                ?.Select(x => x.As<XmlNode>());
            var matches = nodes?.Where(condition);
            return matches?
                .Select(y => y.As<XmlNode>());
        }

        /// <summary>
        /// Map a child node to another object
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="lambda"></param>
        /// <returns></returns>
        public TResult SelectChild<TResult>(Func<XmlNode, TResult> lambda)
        {
            return lambda(this);
        }

        /// <summary>
        /// Map children nodes to another object
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="lambda"></param>
        /// <returns></returns>
        public IEnumerable<TResult> SelectChildren<TResult>(Func<XmlNode, IEnumerable<TResult>> lambda)
        {
            return lambda(this);
        }

        public string ToXmlString()
        {
            return _xmlFormatter.ToXmlString(this);
        }

        /// <summary>
        /// Traverse the parent structure of the object and compute the full path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="node"></param>
        /// <param name="withArrayHints">True to show the array position</param>
        /// <returns></returns>
        private string GetParentPath(string path, INode node, bool withArrayHints = false)
        {
            if (node != null)
            {
                if (node.ArrayPosition.HasValue && withArrayHints)
                {
                    // show the array element position if requested
                    if (node.ParentNode != null)
                    {
                        if (node.ArrayPosition != null) // just in case
                            path = "[" + node.ArrayPosition.Value + "]" + path;
                        else
                            path = "[]" + path;
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

        /// <summary>
        /// Get INode as concrete type
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <returns></returns>
        public TNode As<TNode>()
            where TNode : INode
        {
            return (TNode)(INode)this;
        }

        public override string ToString() => string.Format("{2}{3}{0} {1}, {2} children.", string.IsNullOrEmpty(Name) ? "()" : Name, ChildNodes.Count, IsDefined ? "" : "!", IsValidated ? "" : "*");
    }
}
