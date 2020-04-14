namespace AnyConfig.Json
{
    /// <summary>
    /// Json parser configuration
    /// </summary>
    public class JsonParserConfig
    {
        /// <summary>
        /// True to process raw json backslashes as single backslashes, otherwise they will be kept as is. Default: true
        /// </summary>
        public bool DecodeBackslash { get; set; } = true;
    }
}
