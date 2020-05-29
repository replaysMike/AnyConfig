namespace AnyConfig.Models
{
    internal class ConfigurationProviderResult
    {
        /// <summary>
        /// The requested configuration value
        /// </summary>
        public object Result { get; set; }

        /// <summary>
        /// True if a configuration file was resolved
        /// </summary>
        public bool ConfigurationFileResolved { get; set; }

        /// <summary>
        /// The configuration file resolved
        /// </summary>
        public string ConfigurationFile { get; set; }
    }
}
