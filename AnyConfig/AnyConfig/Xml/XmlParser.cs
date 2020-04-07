using AnyConfig.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AnyConfig.Xml
{
    public class XmlParser : IXmlParser
    {
        /// <summary>
        /// The original string passed to the parser
        /// </summary>
        public string OriginalText { get; private set; }

        /// <summary>
        /// True if xml is valid
        /// </summary>
        public bool IsValid
        {
            get { return IsValidXml(OriginalText); }
        }

        /// <summary>
        /// Create a Xml parser
        /// </summary>
        public XmlParser()
        {
        }

        /// <summary>
        /// True if a valid xml is parsed
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static bool IsValidXml(string xml)
        {
            try
            {
                var parser = new XmlParser();
                var node = parser.Parse(xml);
                if (node != null)
                    return true;
            }
            catch (Exception)
            {
                // invalid xml
            }

            return false;
        }

        public XmlNode Parse(string text)
        {
            OriginalText = text;

            // determine the type of root node: Object or Array
            var rootNode = GetNextNode(text, 0, null);

            //Optionally, we could validate some things here

            return rootNode;
        }

        /// <summary>
        /// (recursive) Parse a XML block
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="pos"></param>
        /// <param name="currentNode"></param>
        /// <returns></returns>
        private XmlNode GetNextNode(string xml, int pos, XmlNode currentNode = null)
        {
            if (currentNode == null)
                currentNode = new XmlNode();
            var startTagOpenPosition = -1;
            var startTagClosePosition = 0;
            var startTagText = string.Empty;

            var endTagOpenPosition = -1;
            var endTagOpenPositionNameStart = -1;
            var endTagClosePosition = 0;
            var endTagText = string.Empty;

            var declaration = string.Empty;
            var startTagName = string.Empty;
            var endTagName = string.Empty;
            var tagAttributes = string.Empty;
            var innerText = string.Empty;
            var innerContent = string.Empty;
            var outerText = string.Empty;

            var quotesCount = 0;
            var quotesStart = -1;
            var quotesEnd = -1;
            var isTagComplete = false;

            // read until we find an open and close quote
            for (var i = pos; i < xml.Length; i++)
            {
                if (IsComment(xml, i))
                {
                    i = DiscardComment(xml, i);
                    continue;
                }
                if (IsDeclaration(xml, i))
                {
                    // read in the xml declaration
                    currentNode.DeclarationNode = ReadDeclaration(xml, i, out var length);
                    i += length;
                    continue;
                }
                if (IsCData(xml, i))
                {
                    var cData = ReadCData(xml, i, out var length);
                    currentNode.CData = cData;
                    i += length;
                    continue;
                }
                if (IsWhitespace(xml, i))
                {
                    if (startTagOpenPosition >= 0 && startTagClosePosition == 0 && startTagName == string.Empty)
                    {
                        // tag name
                        startTagName = xml.Substring(startTagOpenPosition + 1, i - (startTagOpenPosition + 1));
                    }
                    // ignore whitespace
                    if (quotesCount == 0)
                        continue;
                }
                if (xml[i] == '"' && quotesCount == 0)
                {
                    // the start of a quoted string
                    quotesCount++;
                    quotesStart = i + 1;
                    continue;
                }
                else if (xml[i] == '"' && quotesCount > 0)
                {
                    // the end of a quoted string
                    quotesCount--;
                    quotesEnd = i;
                    continue;
                }
                // only look for these markers if we are not inside a quoted string
                else if (quotesCount == 0)
                {
                    if (i + 1 < xml.Length &&
                        xml[i] == '<' && xml[i + 1] != '/')
                    {
                        // start of a tag
                        if (startTagOpenPosition >= 0 && startTagClosePosition > 0 && endTagOpenPosition == -1 && endTagClosePosition == 0)
                        {
                            // parse next block
                            var childBlock = GetNextNode(xml, i);
                            childBlock.ParentNode = currentNode;
                            currentNode.ChildNodes.Add(childBlock);
                            i += childBlock.Length;
                        }
                        else
                        {
                            currentNode.OpenPosition = startTagOpenPosition = i;
                        }
                        continue;
                    }
                    else if (i + 1 < xml.Length
                        && xml[i] == '<' && xml[i + 1] == '/')
                    {
                        // end of a tag
                        endTagOpenPosition = i;
                        endTagOpenPositionNameStart = endTagOpenPosition + 2;
                        innerText = xml.Substring(startTagClosePosition + 1, endTagOpenPosition - (startTagClosePosition + 1));
                        innerContent = GetContent(innerText);
                        i++;
                        continue;
                    }
                    else if (xml[i] == '/' && xml[i + 1] == '>')
                    {
                        // inline end tag
                        // we have a start tag, but the end of the start tag hasn't completed.
                        endTagOpenPosition = startTagOpenPosition;
                        if (startTagName == string.Empty)
                        {
                            // tag name
                            startTagName = xml.Substring(startTagOpenPosition + 1, i - (startTagOpenPosition + 1));
                        }
                        endTagName = startTagName;
                        endTagClosePosition = 0;
                    }
                    else if (xml[i] == '>')
                    {
                        // end of tag part definition
                        if (startTagOpenPosition >= 0 && startTagClosePosition == 0)
                        {
                            // end of start tag
                            startTagClosePosition = i;
                            startTagText = xml.Substring(startTagOpenPosition, startTagClosePosition + 1 - startTagOpenPosition);
                            if (startTagName == string.Empty)
                            {
                                // tag name
                                startTagName = xml.Substring(startTagOpenPosition + 1, i - (startTagOpenPosition + 1));
                            }
                            // attributes
                            if (tagAttributes == string.Empty)
                            {
                                tagAttributes = xml.Substring(startTagOpenPosition + 1 + startTagName.Length, startTagClosePosition - (startTagOpenPosition + 1 + startTagName.Length));
                                currentNode.Attributes = ParseAttributes(tagAttributes, currentNode);
                            }
                        }
                        if (endTagOpenPosition >= 0 && endTagClosePosition == 0)
                        {
                            // end of end tag
                            endTagClosePosition = i;
                            currentNode.ClosePosition = endTagClosePosition = i;
                            if (string.IsNullOrEmpty(endTagName))
                                endTagName = xml.Substring(endTagOpenPositionNameStart, endTagClosePosition - endTagOpenPositionNameStart);
                            if (!endTagName.Equals(startTagName, StringComparison.InvariantCultureIgnoreCase))
                                throw new ParseException($"Error parsing XML. Expected end tag for '{startTagName}' but found '{endTagName}' instead. Please check for syntax errors.");
                            endTagText = xml.Substring(endTagOpenPosition, endTagClosePosition + 1 - endTagOpenPosition);
                            outerText = xml.Substring(startTagOpenPosition, endTagClosePosition + 1 - startTagOpenPosition);
                            isTagComplete = true;
                            break;
                        }
                    }
                }

                if (isTagComplete)
                {
                    i += 1; // read past the > char
                    isTagComplete = false; // reset this bit
                }
            }

            currentNode.Name = startTagName;
            currentNode.InnerContent = innerContent;
            currentNode.InnerText = innerText;
            currentNode.OuterText = outerText;

            var childArrays = currentNode.ChildNodes
                .GroupBy(x => x.Name)
                .Select(x => new { GroupName = x.Key, Count = x.Count() })
                .Where(x => x.Count > 1);
            if (childArrays.Any())
            {
                // populate their array position
                foreach (var arrayGroup in childArrays)
                {
                    var arrayPosition = 0;
                    foreach (var childNode in currentNode.ChildNodes.Where(x => x.Name.Equals(arrayGroup.GroupName)))
                    {
                        childNode.ArrayPosition = arrayPosition;
                        arrayPosition++;
                    }
                }
            }

            return currentNode;
        } // end GetNextBlock

        private List<XmlAttribute> ParseAttributes(string attributeText, XmlNode parentNode)
        {
            var attributes = new List<XmlAttribute>();
            // attributes are defined by key=value pairs seperated by whitespace
            var quotesCount = 0;
            var quotesStart = 0;
            var quotesEnd = 0;
            var attributeName = string.Empty;
            var attributeValue = string.Empty;
            var attributeNameStart = 0;
            var attributeNameEnd = 0;
            var attributeValueStart = 0;
            var attributeValueEnd = 0;
            for (var i = 0; i < attributeText.Length; i++)
            {
                if (i == attributeText.Length - 1)
                {
                    // value end
                    if (attributeNameEnd > 0 && attributeValueStart > 0)
                    {
                        attributeValueEnd = i;
                        attributeValue = attributeText.Substring(attributeValueStart, attributeValueEnd - attributeValueStart + 1);
                        attributeValue = StripEnclosingQuotes(attributeValue);
                        attributes.Add(new XmlAttribute
                        {
                            Name = attributeName,
                            Value = attributeValue,
                            ParentNode = parentNode,
                            Path = parentNode.FullPath
                        });
                    }
                    continue;
                }
                if (attributeText[i] == '"' && quotesCount == 0)
                {
                    // the start of a quoted string
                    quotesCount++;
                    quotesStart = i + 1;
                    continue;
                }
                else if (attributeText[i] == '"' && quotesCount > 0)
                {
                    // the end of a quoted string
                    quotesCount--;
                    quotesEnd = i;
                    continue;
                }
                if (IsWhitespace(attributeText, i) && quotesCount == 0)
                {
                    if (attributeNameEnd > 0 && attributeValueStart > 0)
                    {
                        attributeValueEnd = i;
                        attributeValue = attributeText.Substring(attributeValueStart, attributeValueEnd - attributeValueStart);
                        attributeValue = StripEnclosingQuotes(attributeValue);
                        attributes.Add(new XmlAttribute
                        {
                            Name = attributeName,
                            Value = attributeValue,
                            ParentNode = parentNode,
                            Path = parentNode.FullPath
                        });
                        // reset
                        attributeName = string.Empty;
                        attributeValue = string.Empty;
                        attributeNameStart = i;
                        attributeNameEnd = 0;
                        attributeValueStart = 0;
                        attributeValueEnd = 0;
                    }
                    continue;
                }
                if (attributeText[i] == '=' && attributeName == string.Empty)
                {
                    attributeNameEnd = i;
                    attributeValueStart = i + 1;
                    attributeName = attributeText.Substring(attributeNameStart, attributeNameEnd - attributeNameStart).Trim();
                }
            }

            return attributes;
        }

        public struct Range
        {
            public int Start { get; set; }
            public int End { get; set; }
        }

        private string GetContent(string xml)
        {
            if (xml.Contains("<"))
            {
                var matches = Regex.Matches(xml, @"[<][^>]*[>]", RegexOptions.Multiline | RegexOptions.Compiled);
                var removeRanges = new List<Range>();
                var skipTags = new List<Range>();
                if (matches.Count > 0)
                {
                    var content = new StringBuilder();
                    foreach (Match match in matches)
                    {
                        if (match.Success && match.Captures.Count > 0)
                        {
                            var capture = match.Captures[0];
                            var startPos = capture.Index;
                            // is it a comment? those are ok
                            if (xml[startPos] == '<' && xml[startPos + 1] == '!')
                                continue;
                            // is it already processed?
                            if (skipTags.Contains(new Range { Start = capture.Index, End = capture.Length }))
                                continue;
                            // get the name of the tag so we can look for its ending
                            var tagName = string.Empty;
                            for (var i = startPos + 1; i < startPos + capture.Length; i++)
                            {
                                if (xml[i] == ' ' || xml[i] == '>')
                                {
                                    tagName = xml.Substring(startPos + 1, i - (startPos + 1));
                                    skipTags.Add(new Range { Start = capture.Index, End = capture.Length });
                                    break;
                                }
                            }
                            if (!string.IsNullOrEmpty(tagName))
                            {
                                // find end tag
                                Match endTag = null;
                                foreach (Match endTagMatch in matches)
                                {
                                    if (endTagMatch.Value.Replace(" ", "").StartsWith($"</{tagName}>"))
                                    {
                                        endTag = endTagMatch;
                                        skipTags.Add(new Range { Start = endTag.Index, End = endTag.Length });
                                        break;
                                    }
                                }
                                if (endTag != null)
                                    removeRanges.Add(new Range { Start = startPos, End = endTag.Index + endTag.Length });
                                else
                                    removeRanges.Add(new Range { Start = startPos, End = xml.Length });
                            }

                        }
                    }
                    // walk the string and append non-tag content
                    for (var i = 0; i < xml.Length; i++)
                    {
                        if (removeRanges.Any(x => i >= x.Start && i <= x.End))
                            continue;
                        content.Append(xml[i]);
                    }
                    return content.ToString();
                }
            }
            return xml;
        }

        private string StripEnclosingQuotes(string str)
        {
            str = str.Trim();
            if (str[0] == '\"')
                str = str.Substring(1);
            if (str[str.Length - 1] == '\"')
                str = str.Substring(0, str.Length - 1);

            return str;
        }

        private XmlNode ReadDeclaration(string xml, int position, out int length)
        {
            var startPosition = position;
            length = 0;
            var declaration = new XmlNode();
            declaration.Name = "Declaration";
            var startTagOpenPosition = startPosition;
            var startTagClosePosition = 0;
            position += 6; // skip past <?xml 
            while (position < xml.Length)
            {
                if (position + 1 < xml.Length
                    && xml[position] == '?' && xml[position + 1] == '>')
                {
                    startTagClosePosition = position + 1;
                    length = position + 1;
                    break;
                }
                position++;
            }

            var tagAttributes = xml.Substring(startTagOpenPosition + 6, (startTagClosePosition - 1) - (startTagOpenPosition + 6));
            var attributes = ParseAttributes(tagAttributes, declaration);
            declaration.Attributes = attributes;
            declaration.OuterText = xml.Substring(startTagOpenPosition, startTagClosePosition + 1 - startTagOpenPosition);
            declaration.InnerText = tagAttributes;

            return declaration;
        }

        private int DiscardComment(string xml, int position)
        {
            // position is the start of the comment
            position += 4; // skip past <!--
            while (position < xml.Length)
            {
                if (position + 2 < xml.Length
                    && xml[position] == '-' && xml[position + 1] == '-' && xml[position + 2] == '>')
                {
                    return position + 2;
                }
                position++;
            }
            return xml.Length;
        }

        private string ReadCData(string xml, int position, out int length)
        {
            var startPosition = position;
            length = 0;
            // position is the start of the comment
            position += 8; // skip past <![CDATA[
            while (position < xml.Length)
            {
                if (position + 2 < xml.Length
                    && xml[position] == ']' && xml[position + 1] == ']' && xml[position + 2] == '>')
                {
                    length = position + 1;
                    // copy out the contents of CData excluding header info
                    var cData = xml.Substring(startPosition + 8, length - 3);
                }
                position++;
            }
            return string.Empty;
        }

        /// <summary>
        /// Determine if we are at the start of a declaration
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        private bool IsDeclaration(string xml, int position)
        {
            if (position + 5 < xml.Length
                && xml[position] == '<' && xml[position + 1] == '?' && xml[position + 2] == 'x' && xml[position + 3] == 'm' && xml[position + 4] == 'l' && xml[position + 5] == ' ')
                return true;
            return false;
        }

        /// <summary>
        /// Determine if we are at the start of a comment
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        private bool IsCData(string xml, int position)
        {
            if (position + 8 < xml.Length
                && xml[position] == '<' && xml[position + 1] == '!' && xml[position + 2] == '['
                && xml[position + 3] == 'C' && xml[position + 4] == 'D' && xml[position + 5] == 'A' && xml[position + 6] == 'T' && xml[position + 7] == 'A'
                && xml[position + 8] == '[')
                return true;
            return false;
        }

        /// <summary>
        /// Determine if we are at the start of a comment
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        private bool IsComment(string xml, int position)
        {
            if (position + 3 < xml.Length
                && xml[position] == '<' && xml[position + 1] == '!' && xml[position + 2] == '-' && xml[position + 3] == '-')
                return true;
            return false;
        }

        /// <summary>
        /// Determine if character is considered whitespace
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool IsWhitespace(string xml, int position)
        {
            var c = xml[position];
            if (c == ' ' || c == '\r' || c == '\n' || c == '\t')
                return true;

            return false;
        }
    }
}
