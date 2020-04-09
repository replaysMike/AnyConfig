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

    /// <summary>
    /// Xml format configuration
    /// </summary>
    public class XmlFormatConfig
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
        /// Xml format configuration
        /// </summary>
        public XmlFormatConfig() : this(TabMode.Spaces, LineEnding.Environment)
        {
        }

        /// <summary>
        /// Xml format configuration
        /// </summary>
        /// <param name="tabMode"></param>
        /// <param name="lineEnding"></param>
        public XmlFormatConfig(TabMode tabMode, LineEnding lineEnding) : this(tabMode, lineEnding, 4)
        {

        }

        /// <summary>
        /// Xml format configuration
        /// </summary>
        /// <param name="tabMode"></param>
        /// <param name="lineEnding"></param>
        /// <param name="tabSize"></param>
        public XmlFormatConfig(TabMode tabMode, LineEnding lineEnding, int tabSize)
        {
            TabMode = tabMode;
            TabSize = tabSize;
            LineEnding = lineEnding;
        }
    }
}
