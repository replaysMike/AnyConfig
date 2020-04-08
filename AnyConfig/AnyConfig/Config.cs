using Microsoft.Extensions.Configuration;

namespace AnyConfig
{
    /// <summary>
    /// AnyConfig Configuration
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Get<T>()
        {
            return Resolve<T>();
        }

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settingName"></param>
        /// <returns></returns>
        public static T Get<T>(string settingName)
        {
            return Resolve<T>(settingName, default);
        }

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settingName">Name of setting</param>
        /// <param name="defaultValue">Default value if setting is not found</param>
        /// <returns></returns>
        public static T Get<T>(string settingName, T defaultValue)
        {
            return Resolve<T>(settingName, defaultValue);
        }

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName">Section name to retrieve</param>
        /// <returns></returns>
        public static T GetSection<T>(string sectionName)
        {
            return ResolveSection<T>(sectionName, default, true);
        }

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName">Section name to retrieve</param>
        /// <param name="defaultValue">Default value if setting is not found</param>
        /// <returns></returns>
        public static T GetSection<T>(string sectionName, T defaultValue)
        {
            return ResolveSection<T>(sectionName, defaultValue, false);
        }

        /// <summary>
        /// Get an IConfiguration
        /// </summary>
        /// <returns></returns>
        public static IConfigurationRoot GetConfiguration()
        {
            var resolver = CreateResolver();
            return resolver.GetConfiguration();
        }

        /// <summary>
        /// Get an IConfiguration from a specified file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static IConfigurationRoot GetConfiguration(string filename)
        {
            var resolver = CreateResolver();
            return resolver.GetConfiguration(filename);
        }

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetFromXml<T>()
        {
            var resolver = CreateResolver();
            return resolver.GetFromXml<T>();
        }

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename">Filename of xml configuration to load</param>
        /// <returns></returns>
        public static T GetFromXmlFile<T>(string filename)
        {
            var resolver = CreateResolver();
            return resolver.GetFromXmlFile<T>(filename);
        }

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename">Filename of xml configuration to load</param>
        /// <param name="sectionName">Section name to retrieve</param>
        /// <returns></returns>
        public static T GetFromXmlFile<T>(string filename, string sectionName)
        {
            var resolver = CreateResolver();
            return resolver.GetFromXmlFile<T>(filename, sectionName);
        }

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetFromJson<T>()
        {
            var resolver = CreateResolver();
            return resolver.GetFromJson<T>();
        }

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename">Filename of json configuration to load</param>
        /// <returns></returns>
        public static T GetFromJsonFile<T>(string filename)
        {
            var resolver = CreateResolver();
            return resolver.GetFromJsonFile<T>(filename);
        }

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename">Filename of json configuration to load</param>
        /// <param name="sectionName">Section name to retrieve</param>
        /// <returns></returns>
        public static T GetFromJsonFile<T>(string filename, string sectionName)
        {
            var resolver = CreateResolver();
            return resolver.GetFromJsonFile<T>(filename, sectionName);
        }

        private static T Resolve<T>()
        {
            var resolver = CreateResolver();
            return resolver.ResolveConfiguration<T>();
        }

        private static T Resolve<T>(string settingName, T defaultValue)
        {
            var resolver = CreateResolver();
            return resolver.ResolveConfiguration<T>(settingName, defaultValue);
        }

        private static T ResolveSection<T>(string sectionName, T defaultValue, bool throwsException = false)
        {
            var resolver = CreateResolver();
            return resolver.ResolveConfigurationSection<T>(sectionName, defaultValue, throwsException);
        }

        private static ConfigurationResolver CreateResolver() => new ConfigurationResolver();
    }
}
