using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnyConfig
{
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
        /// <param name="sectionName">Section name to retrieve</param>
        /// <returns></returns>
        public static T Get<T>(string sectionName)
        {
            return Resolve<T>(sectionName);
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

        private static T Resolve<T>(string sectionName)
        {
            var resolver = CreateResolver();
            return resolver.ResolveConfiguration<T>(sectionName);
        }

        private static ConfigurationResolver CreateResolver() => new ConfigurationResolver();
    }
}
