using System;
using System.Text;

namespace AnyConfig.Json
{
    /// <summary>
    /// Json Formatter class
    /// </summary>
    public class JsonFormatter
    {
        /// <summary>
        /// The Json formatting configuration
        /// </summary>
        public JsonFormatConfig FormatConfig { get; set; } = new JsonFormatConfig();

        /// <summary>
        /// Json Formatter
        /// </summary>
        public JsonFormatter()
        {

        }

        /// <summary>
        /// Json Formatter
        /// </summary>
        /// <param name="formatConfig"></param>
        public JsonFormatter(JsonFormatConfig formatConfig)
        {
            FormatConfig = formatConfig;
        }

        /// <summary>
        /// Re-format a Json string
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static string Prettify(string json)
        {
            var parser = new JsonParser();
            var node = parser.Parse(json);

            return new JsonFormatter().ToJsonString(node);
        }

        /// <summary>
        /// Re-format a Json string
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public string PrettifyJson(string json)
        {
            var parser = new JsonParser();
            var node = parser.Parse(json);

            return ToJsonString(node);
        }

        /// <summary>
        /// Convert Json Node to it's formatted Json string
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public string ToJsonString(JsonNode node)
        {
            var sb = new StringBuilder();

            return IterateNodes(node, sb, 0).ToString();
        }

        private StringBuilder IterateNodes(JsonNode node, StringBuilder sb, int depth)
        {
            if (node.ParentNode == null && depth == 0)
                sb.Append("{" + GetLineEnding());
            var childCount = 0;
            foreach (JsonNode childNode in node.ChildNodes)
            {
                childCount++;
                depth++;
                sb = Tabs(sb, depth);
                if (childNode.NodeType == JsonNodeType.Value)
                {
                    if(string.IsNullOrEmpty(childNode.Name))
                        sb.Append(FormatValue(childNode.ValueType, childNode.Value));
                    else
                        sb.Append($"\"{childNode.Name}\": {FormatValue(childNode.ValueType, childNode.Value)}");
                }
                else if (childNode.NodeType == JsonNodeType.Object)
                {
                    if(string.IsNullOrEmpty(childNode.Name))
                        sb.Append($"{{{GetLineEnding()}");
                    else
                        sb.Append($"\"{childNode.Name}\": {{{GetLineEnding()}");
                    sb = IterateNodes(childNode, sb, depth);
                    sb = Tabs(sb, depth);
                    sb.Append("}");
                }
                else if (childNode.NodeType == JsonNodeType.Array)
                {
                    sb.Append($"\"{childNode.Name}\": [{GetLineEnding()}");
                    sb = IterateNodes(childNode, sb, depth);
                    sb = Tabs(sb, depth);
                    sb.Append("]");
                }
                if (childCount < node.ChildNodes.Count)
                    sb.Append($",{GetLineEnding()}");
                else
                    sb.Append(GetLineEnding());

                depth--;
            }
            if (node.ParentNode == null && depth == 0)
                sb.Append($"}}{GetLineEnding()}");

            return sb;
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

        private string FormatValue(PrimitiveTypes dataType, string value)
        {
            if (dataType == PrimitiveTypes.String)
                return $"\"{value}\"";
            else if (dataType == PrimitiveTypes.Null)
                return "null";
            else
                return value;
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
