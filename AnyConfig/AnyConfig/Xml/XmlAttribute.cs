namespace AnyConfig.Xml
{
    /// <summary>
    /// An Xml attribute
    /// </summary>
    public class XmlAttribute
    {
        /// <summary>
        /// Name of attribute
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Path to attribute
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Parent Xml node
        /// </summary>
        public XmlNode ParentNode { get; set; }

        /// <summary>
        /// Attribute value
        /// </summary>
        public string Value { get; set; }

        public override string ToString()
        {
            return $"{Name}=\"{Value}\"";
        }
    }
}
