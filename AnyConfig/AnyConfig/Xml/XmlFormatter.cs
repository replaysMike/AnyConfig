using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnyConfig.Xml
{
    public enum TabMode
    {
        Spaces,
        Tabs
    }
    public enum LineEnding
    {
        CR,
        LF,
        CRLF,
        Environment
    }
    public class XmlFormatConfig
    {
        public TabMode TabMode { get; set; }
        public int TabSize { get; set; }
        public LineEnding LineEnding { get; set; }
        public XmlFormatConfig()
            : this(TabMode.Spaces, LineEnding.Environment, 4)
        {
        }
        public XmlFormatConfig(TabMode tabMode, LineEnding lineEnding, int tabSize = 4)
        {
            TabMode = tabMode;
            TabSize = tabSize;
            LineEnding = lineEnding;
        }
    }

    /// <summary>
    /// Xml Formatter class
    /// </summary>
    public class XmlFormatter
    {
        public XmlFormatConfig FormatConfig { get; set; }
        public XmlFormatter()
            : this(new XmlFormatConfig())
        {

        }

        public XmlFormatter(XmlFormatConfig formatConfig)
        {
            FormatConfig = formatConfig;
        }

        /// <summary>
        /// Re-format a Xml string
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static string Prettify(string xml)
        {
            var parser = new XmlParser();
            var node = parser.Parse(xml);

            return new XmlFormatter().ToXmlString(node);
        }

        /// <summary>
        /// Re-format a Xml string
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public string PrettifyXml(string xml)
        {
            var parser = new XmlParser();
            var node = parser.Parse(xml);

            return ToXmlString(node);
        }

        /// <summary>
        /// Convert XmlNode to it's formatted Xml string
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public string ToXmlString(XmlNode node)
        {
            var sb = new StringBuilder();

            return IterateNodes(node, sb, 0).ToString();
        }

        private StringBuilder IterateNodes(XmlNode node, StringBuilder sb, int depth)
        {
            var childCount = 0;
            sb = Tabs(sb, depth);
            if (node.DeclarationNode != null)
                sb.Append($"{node.DeclarationNode.OuterText}{GetLineEnding()}");
            sb.Append($"<{node.Name}{GetAttributes(node)}>{GetLineEnding()}");
            if (!string.IsNullOrEmpty(node.InnerContent))
            {
                sb = Tabs(sb, depth + 1);
                // display inner text of element without any children
                sb.Append($"{node.InnerContent}{GetLineEnding()}");
            }
            foreach (var childNode in node.ChildNodes)
            {
                childCount++;
                depth++;
                sb = IterateNodes(childNode, sb, depth);
                depth--;
            }
            sb = Tabs(sb, depth);
            sb.Append($"</{node.Name}>{GetLineEnding()}");

            return sb;
        }

        private string GetAttributes(XmlNode node)
        {
            if (node.Attributes.Any())
            {
                return $" {string.Join(" ", node.Attributes.Select(x => $@"{x.Name}=""{x.Value}"""))}";
            }
            return string.Empty;
        }

        private string GetLineEnding()
        {
            switch (FormatConfig.LineEnding)
            {
                case LineEnding.CR:
                    return "\r";
                case LineEnding.LF:
                    return "\n";
                case LineEnding.CRLF:
                    return "\r\n";
                case LineEnding.Environment:
                    return Environment.NewLine;
                default:
                    return Environment.NewLine;
            }
        }

        private StringBuilder Tabs(StringBuilder sb, int depth)
        {
            if (FormatConfig.TabMode == TabMode.Spaces)
            {
                for (var i = 0; i < depth * FormatConfig.TabSize; i++)
                    sb.Append(" ");
            }
            else if (FormatConfig.TabMode == TabMode.Tabs)
            {
                for (var i = 0; i < depth; i++)
                    sb.Append("\t");
            }
            return sb;
        }
    }
}
