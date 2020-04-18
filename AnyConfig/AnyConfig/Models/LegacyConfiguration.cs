namespace AnyConfig.Models
{
    /// <summary>
    /// Legacy configuration used by ConfigurationManager
    /// </summary>
    public class LegacyConfiguration
    {
        /// <summary>
        /// Legacy configuration
        /// </summary>
        public Configuration Configuration { get; set; } = new Configuration();

        /// <summary>
        /// Filename of configuration loaded
        /// </summary>
        public string Filename { get; set; }
    }
}
