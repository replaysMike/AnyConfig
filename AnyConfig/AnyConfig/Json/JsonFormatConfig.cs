namespace AnyConfig.Json
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

    /// <summary>
    /// Json format configuration
    /// </summary>
    public class JsonFormatConfig
    {
        /// <summary>
        /// Specify the tabs output mode
        /// </summary>
        public TabMode TabMode { get; set; }

        /// <summary>
        /// Specify the tab size
        /// </summary>
        public int TabSize { get; set; }

        /// <summary>
        /// Specify the line endings style
        /// </summary>
        public LineEnding LineEnding { get; set; }

        /// <summary>
        /// Json format configuration
        /// </summary>
        public JsonFormatConfig() : this(TabMode.Spaces, LineEnding.Environment)
        {
        }

        /// <summary>
        /// Json format configuration
        /// </summary>
        /// <param name="tabMode"></param>
        /// <param name="lineEnding"></param>
        public JsonFormatConfig(TabMode tabMode, LineEnding lineEnding) : this(tabMode, lineEnding, 4)
        {

        }

        /// <summary>
        /// Json format configuration
        /// </summary>
        /// <param name="tabMode"></param>
        /// <param name="lineEnding"></param>
        /// <param name="tabSize"></param>
        public JsonFormatConfig(TabMode tabMode, LineEnding lineEnding, int tabSize)
        {
            TabMode = tabMode;
            TabSize = tabSize;
            LineEnding = lineEnding;
        }
    }
}
