namespace AnyConfig
{
    /// <summary>
    /// Config storage locations
    /// </summary>
    public enum ConfigSource
    {
        /// <summary>
        /// the ASP.Net WebConfig
        /// </summary>
        WebConfig,
        /// <summary>
        /// the .Net Application Config
        /// </summary>
        ApplicationConfig,
        /// <summary>
        /// An embedded DLL location
        /// </summary>
        EmbeddedResource,
        /// <summary>
        /// The windows registry
        /// </summary>
        Registry,
        /// <summary>
        /// Custom object, such as a static class
        /// </summary>
        Custom,
        /// <summary>
        /// Json configuration file
        /// </summary>
        JsonFile,
        /// <summary>
        /// Xml configuration file
        /// </summary>
        XmlFile
    }
}
