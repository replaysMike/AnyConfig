using AnyConfig.Models;
using Microsoft.Extensions.Configuration;

namespace AnyConfig
{
    /// <summary>
    /// Multi-framework configuration resolver
    /// </summary>
    public interface IConfigurationResolver
    {
        /// <summary>
        /// The detected runtime framework
        /// </summary>
        RuntimeInfo DetectedRuntime { get; }

        /// <summary>
        /// Resolve a configuration for the current runtime platform
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T ResolveConfiguration<T>();

        /// <summary>
        /// Resolve a configuration for the current runtime platform
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T ResolveConfiguration<T>(string sectionName);

        /// <summary>
        /// Resolve a legacy configuration
        /// </summary>
        /// <returns></returns>
        LegacyConfiguration ResolveLegacyConfigurationFromXml();

        /// <summary>
        /// Resolve a legacy configuration
        /// </summary>
        /// <param name="filename">Filename of xml configuration to load</param>
        /// <returns></returns>
        LegacyConfiguration ResolveLegacyConfigurationFromXml(string filename);

        /// <summary>
        /// Resolve a legacy configuration
        /// </summary>
        /// <returns></returns>
        LegacyConfiguration ResolveLegacyConfigurationFromJson();

        /// <summary>
        /// Resolve a configuration for the current runtime platform
        /// </summary>
        /// <returns></returns>
        LegacyConfiguration ResolveLegacyConfigurationFromJson(string filename);

        /// <summary>
        /// Get configuration as an object from Xml configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        T GetFromXml<T>(string sectionName = null);

        /// <summary>
        /// Get configuration as an object from Xml configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        T GetFromXmlFile<T>(string filename, string sectionName = null);

        /// <summary>
        /// Get configuration as an object from Json configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        T GetFromJson<T>(string sectionName = null);

        /// <summary>
        /// Get configuration as an object from Json configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        T GetFromJsonFile<T>(string filename, string sectionName = null);

        /// <summary>
        /// Get an IConfiguration
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        IConfigurationRoot GetConfiguration(string filename = null, string sectionName = null);
    }
}
