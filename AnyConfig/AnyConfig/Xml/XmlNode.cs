using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnyConfig.Xml
{
    public class XmlNode
    {
        private XmlFormatter _xmlFormatter;

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
        public List<XmlNode> ChildNodes { get; set; } = new List<XmlNode>();

        /// <summary>
        /// All child nodes inherited by this node
        /// </summary>
        public List<XmlAttribute> Attributes { get; set; } = new List<XmlAttribute>();

        /// <summary>
        /// The parent mode
        /// </summary>
        public XmlNode ParentNode { get; set; }

        /// <summary>
        /// The XML declaration header
        /// </summary>
        public XmlNode DeclarationNode { get; set; }

        /// <summary>
        /// The value of the node
        /// </summary>
        public string Value { get; set; }

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
        public List<XmlNode> ArrayValues
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
        public XmlNode SelectNodeByName(string name)
        {
            return ChildNodes
                .Where(x => x.Name == name)
                .FirstOrDefault();
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
        private string GetParentPath(string path, XmlNode node, bool withArrayHints = false)
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
                    else
                    {

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

        public override string ToString() => string.Format("{2}{3}{0} {1}, {2} children.", string.IsNullOrEmpty(Name) ? "()" : Name, ChildNodes.Count, IsDefined ? "" : "!", IsValidated ? "" : "*");
    }
}
