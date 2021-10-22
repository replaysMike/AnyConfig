using System.Text;
using System.Text.RegularExpressions;

namespace AnyConfig
{
    internal static class NodeHelpers
    {
        
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

        /// <summary>
        /// Remap array positions from [x] to :x
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal static string RemapIConfigurationArrayPositionText(string key)
        {
            if (key.Contains("["))
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
    }
}
