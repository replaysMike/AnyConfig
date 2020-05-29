using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;

namespace AnyConfig
{
    /// <summary>
    /// AnyConfig Configuration
    /// </summary>
    public static class Config
    {
        public static string LastResolvedConfigurationFilename { get; internal set; }

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Get<T>() => Resolve<T>();

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assembly">Specify the assembly associated with the configuration</param>
        /// <returns></returns>
        public static T Get<T>(Assembly assembly) => Resolve<T>(assembly);

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <param name="settingName">Name of setting</param>
        /// <returns></returns>
        public static string Get(string settingName) => Resolve<string>(settingName, default);

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <param name="settingName">Name of setting</param>
        /// <param name="assembly">Specify the assembly associated with the configuration</param>
        /// <returns></returns>
        public static string Get(string settingName, Assembly assembly) => Resolve<string>(assembly, settingName, default);

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <param name="settingName">Name of setting</param>
        /// <param name="type">The type to bind to</param>
        /// <returns></returns>
        public static object Get(string settingName, Type type) => Resolve(settingName, type, default);

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <param name="settingName">Name of setting</param>
        /// <param name="type">The type to bind to</param>
        /// <param name="assembly">Specify the assembly associated with the configuration</param>
        /// <returns></returns>
        public static object Get(string settingName, Type type, Assembly assembly) => Resolve(assembly, settingName, type, default);

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settingName">Name of setting</param>
        /// <returns></returns>
        public static T Get<T>(string settingName) => Resolve<T>(settingName, default);

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settingName">Name of setting</param>
        /// <param name="assembly">Specify the assembly associated with the configuration</param>
        /// <returns></returns>
        public static T Get<T>(string settingName, Assembly assembly) => Resolve<T>(assembly, settingName, default);

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settingName">Name of setting</param>
        /// <param name="defaultValue">Default value if setting is not found</param>
        /// <returns></returns>
        public static string Get(string settingName, string defaultValue) => Resolve<string>(settingName, defaultValue);

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settingName">Name of setting</param>
        /// <param name="defaultValue">Default value if setting is not found</param>
        /// <param name="assembly">Specify the assembly associated with the configuration</param>
        /// <returns></returns>
        public static string Get(string settingName, string defaultValue, Assembly assembly) => Resolve<string>(assembly, settingName, defaultValue);

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settingName">Name of setting</param>
        /// <param name="defaultValue">Default value if setting is not found</param>
        /// <returns></returns>
        public static T Get<T>(string settingName, T defaultValue) => Resolve<T>(settingName, defaultValue);

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settingName">Name of setting</param>
        /// <param name="defaultValue">Default value if setting is not found</param>
        /// <param name="assembly">Specify the assembly associated with the configuration</param>
        /// <returns></returns>
        public static T Get<T>(string settingName, T defaultValue, Assembly assembly) => Resolve<T>(assembly, settingName, defaultValue);

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settingName">Name of setting</param>
        /// <param name="type">The type to bind to</param>
        /// <param name="defaultValue">Default value if setting is not found</param>
        /// <returns></returns>
        public static object Get(string settingName, Type type, object defaultValue) => Resolve(settingName, type, defaultValue);

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settingName">Name of setting</param>
        /// <param name="type">The type to bind to</param>
        /// <param name="defaultValue">Default value if setting is not found</param>
        /// <param name="assembly">Specify the assembly associated with the configuration</param>
        /// <returns></returns>
        public static object Get(string settingName, Type type, object defaultValue, Assembly assembly) => Resolve(assembly, settingName, type, defaultValue);

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName">Section name to retrieve</param>
        /// <returns></returns>
        public static T GetSection<T>(string sectionName) => ResolveSection<T>(sectionName, default, true);

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName">Section name to retrieve</param>
        /// <param name="assembly">Specify the assembly associated with the configuration</param>
        /// <returns></returns>
        public static T GetSection<T>(string sectionName, Assembly assembly) => ResolveSection<T>(assembly, sectionName, default, true);

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName">Section name to retrieve</param>
        /// <param name="type">The type to bind to</param>
        /// <returns></returns>
        public static object GetSection(string sectionName, Type type) => ResolveSection(sectionName, type, default, true);

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName">Section name to retrieve</param>
        /// <param name="type">The type to bind to</param>
        /// <param name="assembly">Specify the assembly associated with the configuration</param>
        /// <returns></returns>
        public static object GetSection(string sectionName, Type type, Assembly assembly) => ResolveSection(assembly, sectionName, type, default, true);

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName">Section name to retrieve</param>
        /// <param name="defaultValue">Default value if setting is not found</param>
        /// <returns></returns>
        public static T GetSection<T>(string sectionName, T defaultValue) => ResolveSection<T>(sectionName, defaultValue, false);

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName">Section name to retrieve</param>
        /// <param name="defaultValue">Default value if setting is not found</param>
        /// <param name="assembly">Specify the assembly associated with the configuration</param>
        /// <returns></returns>
        public static T GetSection<T>(string sectionName, T defaultValue, Assembly assembly) => ResolveSection<T>(assembly, sectionName, defaultValue, false);

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName">Section name to retrieve</param>
        /// <param name="type">The type to bind to</param>
        /// <param name="defaultValue">Default value if setting is not found</param>
        /// <returns></returns>
        public static object GetSection(string sectionName, Type type, object defaultValue) => ResolveSection(sectionName, type, defaultValue, false);


        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName">Section name to retrieve</param>
        /// <param name="type">The type to bind to</param>
        /// <param name="defaultValue">Default value if setting is not found</param>
        /// <param name="assembly">Specify the assembly associated with the configuration</param>
        /// <returns></returns>
        public static object GetSection(string sectionName, Type type, object defaultValue, Assembly assembly) => ResolveSection(assembly, sectionName, type, defaultValue, false);

        /// <summary>
        /// Get an IConfiguration
        /// </summary>
        /// <returns></returns>
        public static IConfigurationRoot GetConfiguration()
        {
            var resolver = CreateResolver();
            var config = resolver.GetConfiguration();
            LastResolvedConfigurationFilename = resolver.LastResolvedConfigurationFilename;
            return config;
        }

        /// <summary>
        /// Get an IConfiguration
        /// </summary>
        /// <returns></returns>
        public static IConfigurationRoot GetConfiguration(Assembly assembly)
        {
            var resolver = CreateResolver(assembly);
            var config = resolver.GetConfiguration();
            LastResolvedConfigurationFilename = resolver.LastResolvedConfigurationFilename;
            return config;
        }

        /// <summary>
        /// Get an IConfiguration from a specified file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static IConfigurationRoot GetConfiguration(string filename)
        {
            var resolver = CreateResolver();
            var config = resolver.GetConfiguration(filename);
            LastResolvedConfigurationFilename = resolver.LastResolvedConfigurationFilename;
            return config;
        }

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetFromXml<T>()
        {
            var resolver = CreateResolver();
            var config = resolver.GetFromXml<T>();
            LastResolvedConfigurationFilename = resolver.LastResolvedConfigurationFilename;
            return config;
        }

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static object GetFromXml(string sectionName, Type type)
        {
            var resolver = CreateResolver();
            var config = resolver.LoadXmlConfiguration(null, type, sectionName: sectionName);
            LastResolvedConfigurationFilename = resolver.LastResolvedConfigurationFilename;
            return config;
        }

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetFromXml<T>(Assembly assembly)
        {
            var resolver = CreateResolver(assembly);
            var config = resolver.GetFromXml<T>();
            LastResolvedConfigurationFilename = resolver.LastResolvedConfigurationFilename;
            return config;
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
            var config = resolver.GetFromXmlFile<T>(filename);
            LastResolvedConfigurationFilename = resolver.LastResolvedConfigurationFilename;
            return config;
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
            var config = resolver.GetFromXmlFile<T>(filename, sectionName);
            LastResolvedConfigurationFilename = resolver.LastResolvedConfigurationFilename;
            return config;
        }

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename">Filename of xml configuration to load</param>
        /// <param name="sectionName">Section name to retrieve</param>
        /// <returns></returns>
        public static T GetFromXmlFile<T>(string filename, string settingName, T defaultValue)
        {
            var resolver = CreateResolver();
            var config = resolver.GetFromXmlFile<T>(filename, settingName, defaultValue);
            LastResolvedConfigurationFilename = resolver.LastResolvedConfigurationFilename;
            return config;
        }

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename">Filename of xml configuration to load</param>
        /// <param name="sectionName">Section name to retrieve</param>
        /// <returns></returns>
        public static T GetSettingFromXmlFile<T>(string filename, string settingName)
        {
            var resolver = CreateResolver();
            var config = resolver.GetSettingFromXmlFile<T>(filename, settingName);
            LastResolvedConfigurationFilename = resolver.LastResolvedConfigurationFilename;
            return config;
        }

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetFromJson<T>()
        {
            var resolver = CreateResolver();
            var config = resolver.GetFromJson<T>();
            LastResolvedConfigurationFilename = resolver.LastResolvedConfigurationFilename;
            return config;
        }

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetFromJson<T>(Assembly assembly)
        {
            var resolver = CreateResolver(assembly);
            var config = resolver.GetFromJson<T>();
            LastResolvedConfigurationFilename = resolver.LastResolvedConfigurationFilename;
            return config;
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
            var config = resolver.GetFromJsonFile<T>(filename);
            LastResolvedConfigurationFilename = resolver.LastResolvedConfigurationFilename;
            return config;
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
            var config = resolver.GetFromJsonFile<T>(filename, sectionName);
            LastResolvedConfigurationFilename = resolver.LastResolvedConfigurationFilename;
            return config;
        }

        /// <summary>
        /// Get a specific configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename">Filename of json configuration to load</param>
        /// <param name="sectionName">Section name to retrieve</param>
        /// <returns></returns>
        public static T GetFromJsonFile<T>(string filename, string settingName, T defaultValue)
        {
            var resolver = CreateResolver();
            var config = resolver.GetFromJsonFile<T>(filename, settingName, defaultValue);
            LastResolvedConfigurationFilename = resolver.LastResolvedConfigurationFilename;
            return config;
        }

        private static T Resolve<T>()
        {
            var resolver = CreateResolver();
            var config = resolver.ResolveConfiguration<T>();
            LastResolvedConfigurationFilename = resolver.LastResolvedConfigurationFilename;
            return config;
        }

        private static T Resolve<T>(Assembly assembly)
        {
            var resolver = CreateResolver(assembly);
            var config = resolver.ResolveConfiguration<T>();
            LastResolvedConfigurationFilename = resolver.LastResolvedConfigurationFilename;
            return config;
        }

        private static T Resolve<T>(string settingName, T defaultValue)
        {
            var resolver = CreateResolver();
            var config = resolver.ResolveConfiguration<T>(settingName, defaultValue);
            LastResolvedConfigurationFilename = resolver.LastResolvedConfigurationFilename;
            return config;
        }

        private static T Resolve<T>(Assembly assembly, string settingName, T defaultValue)
        {
            var resolver = CreateResolver(assembly);
            var config = resolver.ResolveConfiguration<T>(settingName, defaultValue);
            LastResolvedConfigurationFilename = resolver.LastResolvedConfigurationFilename;
            return config;
        }

        private static object Resolve(string settingName, Type type, object defaultValue)
        {
            var resolver = CreateResolver();
            var config = resolver.ResolveConfiguration(settingName, type, defaultValue);
            LastResolvedConfigurationFilename = resolver.LastResolvedConfigurationFilename;
            return config;
        }

        private static object Resolve(Assembly assembly, string settingName, Type type, object defaultValue)
        {
            var resolver = CreateResolver();
            var config = resolver.ResolveConfiguration(settingName, type, defaultValue);
            LastResolvedConfigurationFilename = resolver.LastResolvedConfigurationFilename;
            return config;
        }

        private static T ResolveSection<T>(string sectionName, T defaultValue, bool throwsException)
        {
            var resolver = CreateResolver();
            var config = resolver.ResolveConfigurationSection<T>(sectionName, defaultValue, throwsException);
            LastResolvedConfigurationFilename = resolver.LastResolvedConfigurationFilename;
            return config;
        }

        private static T ResolveSection<T>(Assembly assembly, string sectionName, T defaultValue, bool throwsException)
        {
            var resolver = CreateResolver(assembly);
            var config = resolver.ResolveConfigurationSection<T>(sectionName, defaultValue, throwsException);
            LastResolvedConfigurationFilename = resolver.LastResolvedConfigurationFilename;
            return config;
        }

        private static object ResolveSection(string sectionName, Type type, object defaultValue, bool throwsException)
        {
            var resolver = CreateResolver();
            var config = resolver.ResolveConfigurationSection(sectionName, type, defaultValue, throwsException);
            LastResolvedConfigurationFilename = resolver.LastResolvedConfigurationFilename;
            return config;
        }

        private static object ResolveSection(Assembly assembly, string sectionName, Type type, object defaultValue, bool throwsException)
        {
            var resolver = CreateResolver(assembly);
            var config = resolver.ResolveConfigurationSection(sectionName, type, defaultValue, throwsException);
            LastResolvedConfigurationFilename = resolver.LastResolvedConfigurationFilename;
            return config;
        }

        private static ConfigurationResolver CreateResolver() => new ConfigurationResolver();
        private static ConfigurationResolver CreateResolver(Assembly assembly) => new ConfigurationResolver(assembly);
    }
}
