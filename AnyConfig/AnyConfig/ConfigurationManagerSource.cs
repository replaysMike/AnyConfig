namespace AnyConfig
{
    /// <summary>
    /// The source configuration to load
    /// </summary>
    public enum ConfigurationManagerSource
    {
        /// <summary>
        /// Automatically determine the source
        /// </summary>
        Auto = 1,
        /// <summary>
        /// Load via Xml configuration
        /// </summary>
        Xml,
        /// <summary>
        /// Load via Json configuration
        /// </summary>
        Json
    }
}
