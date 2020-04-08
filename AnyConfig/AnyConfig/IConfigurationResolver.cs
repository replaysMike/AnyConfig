using AnyConfig.Models;
using Microsoft.Extensions.Configuration;
using System;

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
        /// Resolve a configuration for the current runtime platform with a specific setting name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settingName">Name of setting to load</param>
        /// <param name="defaultValue">Value to return if setting is not found</param>
        /// <param name="throwsException">True if exceptions should be thrown if data cannot be loaded</param>
        /// <returns></returns>
        T ResolveConfiguration<T>(string settingName, T defaultValue, bool throwsException = false);

        /// <summary>
        /// Resolve a configuration for the current runtime platform with a specific setting name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settingName">Name of setting to load</param>
        /// <param name="type">The type to return</param>
        /// <param name="defaultValue">Value to return if setting is not found</param>
        /// <param name="throwsException">True if exceptions should be thrown if data cannot be loaded</param>
        /// <returns></returns>
        object ResolveConfiguration(string settingName, Type type, object defaultValue, bool throwsException = false);

        /// <summary>
        /// Resolve a configuration for the current runtime platform
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName">Name of section to load</param>
        /// <param name="defaultValue">Value to return if setting is not found</param>
        /// <param name="throwsException">True if exceptions should be thrown if data cannot be loaded</param>
        /// <returns></returns>
        T ResolveConfigurationSection<T>(string sectionName, T defaultValue, bool throwsException = false);

        /// <summary>
        /// Resolve a configuration for the current runtime platform
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName">Name of section to load</param>
        /// <param name="type">The type to return</param>
        /// <param name="defaultValue">Value to return if setting is not found</param>
        /// <param name="throwsException">True if exceptions should be thrown if data cannot be loaded</param>
        /// <returns></returns>
        object ResolveConfigurationSection(string sectionName, Type type, object defaultValue, bool throwsException = false);

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
