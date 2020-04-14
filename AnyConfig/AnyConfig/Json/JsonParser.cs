using AnyConfig.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace AnyConfig.Json
{
    /// <summary>
    /// Json v4 parser
    /// </summary>
    public class JsonParser : IJsonParser
    {
        /// <summary>
        /// The original string passed to the parser
        /// </summary>
        public string OriginalText { get; private set; }

        /// <summary>
        /// True if json is valid
        /// </summary>
        public bool IsValid
        {
            get { return IsValidJson(OriginalText); }
        }

        /// <summary>
        /// Create a Json V4 parser
        /// </summary>
        public JsonParser()
        {
        }

        /// <summary>
        /// True if a valid json is parsed
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static bool IsValidJson(string json)
        {
            try
            {
                var parser = new JsonParser();
                var node = parser.Parse(json);
                if (node != null)
                    return true;
            }
            catch (Exception)
            {
                // invalid json
            }

            return false;
        }

        public JsonNode Parse(string json)
        {
            if (string.IsNullOrEmpty(json))
                throw new ArgumentNullException(nameof(json));
            // Convert into a JSNode document
            OriginalText = EnsureRooted(json);

            // determine the type of root node: Object or Array

            var rootNode = GetNextBlock(OriginalText, 0, null);

            //Optionally, we could validate some things here

            return rootNode;
        }

        /// <summary>
        /// Ensure the json block is contained in an empty json block as per the standard { }
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private string EnsureRooted(string json)
        {
            if (!json.Trim().StartsWith("{"))
            {
                var sb = new StringBuilder();
                sb.Append($"{{{Environment.NewLine}");
                sb.Append(json);
                if (json.EndsWith(Environment.NewLine))
                    sb.Append($"}}{Environment.NewLine}");
                else
                    sb.Append($"{Environment.NewLine}}}{Environment.NewLine}");
                return sb.ToString();
            }

            return json;
        }

        /// <summary>
        /// (recursive) Parse a JSON block
        /// A block may consist of an object, a value type or an array of types.
        /// </summary>
        /// <param name="json"></param>
        /// <param name="pos"></param>
        /// <param name="parentBlock"></param>
        /// <returns></returns>
        private JsonNode GetNextBlock(string json, int pos, JsonNode parentBlock = null)
        {
            if (parentBlock == null)
                parentBlock = new JsonNode();
            var blockCount = 0;
            var quotesCount = 0;
            var quotesStart = -1;
            var quotesEnd = -1;
            var blockOpenPosition = 0;
            var fieldName = string.Empty;
            var isColonFound = false;

            // read until we find an open and close quote
            for (var i = pos; i < json.Length; i++)
            {
                // ignore whitespace
                if (IsWhitespace(json[i]) && quotesCount == 0) continue;
                if (json[i] == '"' && quotesCount == 0)
                {
                    // the start of a quoted string
                    quotesCount++;
                    quotesStart = i + 1;
                    continue;
                }
                else if (json[i] == '"' && quotesCount > 0)
                {
                    // the end of a quoted string
                    quotesCount--;
                    quotesEnd = i;
                    fieldName = json.Substring(quotesStart, quotesEnd - quotesStart);
                    continue;
                }
                // only look for these markers if we are not inside a quoted string
                else if (quotesCount == 0)
                {
                    if (json[i] == '{')
                    {
                        // process start of new object
                        if (parentBlock != null)
                            parentBlock.OpenPosition = i;
                        blockOpenPosition = i;
                        blockCount++;
                        continue;
                    }
                    else if (json[i] == '}')
                    {
                        // finalize end of current object
                        blockCount--;
                        if (blockCount <= 0)
                        {
                            if (parentBlock != null)
                                parentBlock.ClosePosition = i + 1;
                            break;
                        }
                    }
                    else if (!isColonFound && json[i] == '[')
                    {
                        // an array start element was found.
                        // we should only look for this in the root of the json data

                        parentBlock = ParseArrayBlock(json, i);
                        parentBlock.OpenPosition = i;
                        parentBlock.ValueType = PrimitiveTypes.Array;
                        // set the json contents
                        if (parentBlock.OpenPosition >= 0 && parentBlock.Length > 0)
                            parentBlock.OuterText = json.Substring(parentBlock.OpenPosition, parentBlock.Length).Trim();
                        i = parentBlock.ClosePosition - 1;
                        break;
                    }
                    else if (json[i] == ']')
                    {
                        // if we found the end of an array, break out as we are at the end
                        // of an array block, no need to seek more.
                        break;
                    }
                    else if (json[i] == ':')
                    {
                        // we've found the next block/value to process
                        isColonFound = true;
                    }
                }

                if (isColonFound)
                {
                    i += 1; // read past the colon char
                    isColonFound = false; // reset this bit

                    // what follows is either an array, object or value type.
                    // detect and determine what type of data needs to be parsed.
                    var blockType = DetectBlockType(json, i);
                    var currentBlock = new JsonNode();
                    currentBlock.OpenPosition = quotesStart;
                    switch (blockType)
                    {
                        case JsonNodeType.Value:
                            currentBlock = ParseValueBlock(json, i);
                            currentBlock.ValueType = DetectDataType(json, i);
                            currentBlock.OpenPosition = quotesStart - 1;
                            break;
                        case JsonNodeType.Array:
                            currentBlock = ParseArrayBlock(json, i);
                            currentBlock.ValueType = PrimitiveTypes.Array;
                            currentBlock.OpenPosition = quotesStart - 1;
                            break;
                        case JsonNodeType.Object:
                            currentBlock = ParseObjectBlock(json, i);
                            currentBlock.ValueType = PrimitiveTypes.Object;
                            currentBlock.OpenPosition = quotesStart - 1;
                            break;
                        default:
                            break;
                    }
                    currentBlock.Name = fieldName;

                    if (currentBlock.OpenPosition >= 0 && currentBlock.Length > 0)
                        currentBlock.OuterText = json.Substring(currentBlock.OpenPosition, currentBlock.Length).Trim();

                    if (parentBlock != null)
                    {
                        currentBlock.ParentNode = parentBlock;
                        parentBlock.ChildNodes.Add(currentBlock);
                    }

                    // skip the data we processed
                    i = currentBlock.ClosePosition - 1;
                }
            }

            if (string.IsNullOrEmpty(parentBlock.OuterText))
            {
                if (parentBlock != null && parentBlock.Length > 0)
                    parentBlock.OuterText = json.Substring(parentBlock.OpenPosition, parentBlock.Length).Trim();
                else
                {
                    throw new ParseException("Error: End of Block not found. Double check your json for formatting errors.");
                }
            }

            return parentBlock;
        }

        /// <summary>
        /// Determine what type of JSON block is next to parse
        /// </summary>
        /// <param name="json"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        private JsonNodeType DetectBlockType(string json, int pos)
        {
            var nodeType = JsonNodeType.Value;
            for (var i = pos; i < json.Length; i++)
            {
                // ignore whitespace
                if (IsWhitespace(json[i])) continue;
                if (json[i] == '[') return JsonNodeType.Array;
                if (json[i] == '{') return JsonNodeType.Object;
                if (json[i] == '"'
                        || IsNumeric(json, i)
                        || IsBoolean(json, i)
                        || IsNull(json, i))
                    return JsonNodeType.Value;
            }
            return nodeType;
        }

        /// <summary>
        /// Detect the data type of a value
        /// </summary>
        /// <param name="json"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        private PrimitiveTypes DetectDataType(string json, int pos)
        {
            var dataType = PrimitiveTypes.Null;
            for (var i = pos; i < json.Length; i++)
            {
                // ignore whitespace
                if (IsWhitespace(json[i])) continue;
                if (json[i] == '"')
                {
                    dataType = PrimitiveTypes.String;
                    break;
                }
                else if (json[i] == '[')
                {
                    dataType = PrimitiveTypes.Array;
                    break;
                }
                else if (IsInteger(json, i))    // 1.0 or 1.9999 will fail, but 1 will pass. Json schema spec is weird.
                {
                    dataType = PrimitiveTypes.Integer;
                    break;
                }
                else if (IsNumeric(json, i))
                {
                    dataType = PrimitiveTypes.Number;
                    break;
                }
                else if (IsBoolean(json, i))
                {
                    dataType = PrimitiveTypes.Boolean;
                    break;
                }
                else if (IsNull(json, i))
                {
                    dataType = PrimitiveTypes.Null;
                    break;
                }
            }
            return dataType;
        }

        /// <summary>
        /// Determine if value is a number
        /// </summary>
        /// <param name="json"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        private bool IsNumeric(string json, int pos)
        {
            // read until end of block character found
            if (json == null) return false;
            var segment = ParseSegment(json, pos, out _, out _).Trim();
            var isNumeric = double.TryParse(segment, out _);
            return isNumeric;
        }

        /// <summary>
        /// Determine if value is an integer (whole value, and no decimal places)
        /// 1.0 is not an integer, neither is 1.9999.
        /// </summary>
        /// <param name="json"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        private bool IsInteger(string json, int pos)
        {
            // read until end of block character found
            if (json == null) return false;

            var segment = ParseSegment(json, pos, out _, out _).Trim();
            var n = Double.Epsilon;
            var isNumber = double.TryParse(segment, out n);
            var isWholeValue = ((Math.Abs(n % 1) < Double.Epsilon));
            var isInteger = isNumber && isWholeValue;

            // if number had a decimal place, it doesn't count. (thanks to weird Json data types!)
            NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;
            if (isNumber && isWholeValue && segment.IndexOf(nfi.NumberDecimalSeparator) >= 0)
                isInteger = false;

            return isInteger;
        }

        /// <summary>
        /// Determine if value is a boolean
        /// </summary>
        /// <param name="json"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        private bool IsBoolean(string json, int pos)
        {
            if (json == null) return false;

            var segment = ParseSegment(json, pos, out _, out _).Trim();
            if (segment.Equals("true", StringComparison.InvariantCultureIgnoreCase)
                || segment.Equals("false", StringComparison.InvariantCultureIgnoreCase))
                return true;
            return false;
        }

        /// <summary>
        /// Determin if value is null
        /// </summary>
        /// <param name="json"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        private bool IsNull(string json, int pos)
        {
            if (json == null) return true;

            var segment = ParseSegment(json, pos, out _, out _).Trim();
            if (segment.Equals("null", StringComparison.InvariantCultureIgnoreCase))
                return true;
            return false;
        }

        /// <summary>
        /// Parse the next item as a value type
        /// </summary>
        /// <param name="json"></param>
        /// <param name="pos"></param>
        /// <param name="cursorPosition"></param>
        /// <returns></returns>
        private string ParseSegment(string json, int pos, out int cursorPosition, out int openPosition)
        {
            var EOBC = ",{}[]";
            var segment = "";
            var quotesCount = 0;
            cursorPosition = pos;
            openPosition = pos;
            for (var i = pos; i < json.Length; i++)
            {
                if (IsWhitespace(json[i]) && quotesCount == 0) continue;
                if (EOBC.Contains(json[i]) && quotesCount == 0)
                {
                    cursorPosition = i;
                    break;
                }
                if (json[i] == '"' && quotesCount == 0)
                {
                    openPosition = i;
                    quotesCount++;
                }
                else if (json[i] == '"' && quotesCount > 0)
                {
                    quotesCount--;
                    segment += '"';
                    cursorPosition = i + 1;
                    break;
                }
                if (segment.Length == 0)
                    openPosition = i;
                segment += json[i];
                cursorPosition = i;
            }

            return segment.Trim();  // remove surrounding whitespace
        }

        /// <summary>
        /// Parse an object
        /// </summary>
        /// <param name="json"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        private JsonNode ParseObjectBlock(string json, int pos)
        {
            // todo: needs to be recursive
            var node = new JsonNode();
            node.NodeType = JsonNodeType.Object;
            node.ValueType = PrimitiveTypes.Object;
            node = GetNextBlock(json, pos, node);
            return node;
        }

        /// <summary>
        /// Parse an array
        /// </summary>
        /// <param name="json"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        private JsonNode ParseArrayBlock(string json, int pos)
        {
            // todo: needs to be recursive
            var node = new JsonNode();
            node.NodeType = JsonNodeType.Array;
            node.ValueType = PrimitiveTypes.Array;
            // arrays will show up as child objects
            // iterate the comma separated list of items
            var arrayElements = new List<JsonNode>();
            // find the start of the array. we should be right near it.
            var startOfArray = json.IndexOf("[", pos);
            node.OpenPosition = startOfArray;
            var closePosition = 0;
            for (var i = startOfArray + 1; i < json.Length; i++)
            {
                if (IsWhitespace(json[i]))
                    continue;
                if (json[i] == ',')
                    continue;
                if (json[i] == ']')
                {
                    // end of array
                    closePosition = i + 1;
                    break;
                }
                var blockType = DetectBlockType(json, i);

                if (blockType == JsonNodeType.Array || blockType == JsonNodeType.Object)
                {
                    var arraynode = GetNextBlock(json, i);
                    if (arraynode != null)
                    {
                        arraynode.ValueType = blockType == JsonNodeType.Object ? PrimitiveTypes.Object : PrimitiveTypes.Array;
                        arraynode.ParentNode = node;
                        arraynode.ArrayPosition = arrayElements.Count;
                        arrayElements.Add(arraynode);
                        i = arraynode.ClosePosition - 1;
                    }
                }
                else if (blockType == JsonNodeType.Value)
                {
                    var arraynode = ParseValueBlock(json, i);
                    if (arraynode != null)
                    {
                        arraynode.ValueType = DetectDataType(json, i);
                        arraynode.ParentNode = node;
                        arraynode.ArrayPosition = arrayElements.Count;
                        arrayElements.Add(arraynode);
                        i = arraynode.ClosePosition - 1;
                    }
                }
            }
            if (closePosition > 0)
                node.ClosePosition = closePosition;
            else
                node.ClosePosition = json.Length - 1;
            node.ChildNodes.AddRange(arrayElements);

            return node;
        }

        /// <summary>
        /// Parse a value
        /// </summary>
        /// <param name="json"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        private JsonNode ParseValueBlock(string json, int pos)
        {
            var node = new JsonNode();
            var cursorPosition = pos;
            node.NodeType = JsonNodeType.Value;
            var openPosition = pos;
            // read in a string, a number, a boolean or a null
            var segment = ParseSegment(json, pos, out cursorPosition, out openPosition);
            if (segment.Length > 0)
            {
                node.OpenPosition = openPosition;
                node.ClosePosition = openPosition + segment.Length;
                if (segment.Equals("null", StringComparison.InvariantCultureIgnoreCase))
                    node.Value = null;
                else
                    node.Value = StripQuotedString(segment);
            }
            else
                node = null;    // no segment found
            return node;
        }

        /// <summary>
        /// Determine if character is considered whitespace
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool IsWhitespace(char c)
        {
            if (c == ' ' || c == '\r' || c == '\n' || c == '\t')
                return true;

            return false;
        }

        /// <summary>
        /// Strip quotes from a string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string StripQuotedString(string str)
        {
            // todo: this is a blind strip. Should allow quoted strings (does JSON support that?)
            return str.Replace("\"", "");
        }
    }


}
