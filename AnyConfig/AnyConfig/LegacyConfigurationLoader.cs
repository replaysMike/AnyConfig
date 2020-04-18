using AnyConfig.Collections;
using AnyConfig.Exceptions;
using AnyConfig.Json;
using AnyConfig.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using TypeSupport;
using TypeSupport.Extensions;

namespace AnyConfig
{
    /// <summary>
    /// Loads a legacy configuration
    /// </summary>
    public class LegacyConfigurationLoader
    {
        private readonly Assembly _entryAssembly;

        public LegacyConfigurationLoader(Assembly entryAssembly)
        {
            _entryAssembly = entryAssembly;
        }

        public LegacyConfiguration LoadDotNetCoreLegacyConfiguration()
        {
            return LoadDotNetCoreLegacyConfiguration(string.Empty);
        }

        public LegacyConfiguration LoadDotNetCoreLegacyConfiguration(string filename)
        {
            var configFile = ResolveConfigFile(filename);
            return SerializeLegacyJsonConfiguration(configFile);
        }

        public LegacyConfiguration LoadDotNetFrameworkLegacyConfiguration()
        {
            return LoadDotNetFrameworkLegacyConfiguration(string.Empty);
        }

        public LegacyConfiguration LoadDotNetFrameworkLegacyConfiguration(string filename)
        {
            var configFile = ResolveConfigFile(filename);
            return SerializeLegacyXmlConfiguration(configFile);
        }

        private string ResolveConfigFile(string filename)
        {
            var configFile = filename;
            if (!string.IsNullOrEmpty(configFile))
            {
                // resolve provided configuration filename
                configFile = Path.GetFullPath(filename);
                if (!File.Exists(configFile))
                {
                    configFile = Path.Combine(Path.GetDirectoryName(_entryAssembly.Location), filename);
                    if (!File.Exists(configFile))
                        throw new FileNotFoundException($"Could not find configuration file '{filename}' in any search path.");
                }
            }
            else
            {
                // resolve configuration automatically
                var appDomain = AppDomain.CurrentDomain;
                var baseDirectory = appDomain.BaseDirectory;
                // find the configuration file
#if NETFRAMEWORK
            configFile = configFile ?? appDomain.SetupInformation.ConfigurationFile;
#else
                if (_entryAssembly == null) throw new ConfigurationMissingException("Entry assembly not registered.");
                configFile = configFile ?? Path.Combine(baseDirectory, $"{_entryAssembly.Location}.config");
#endif
                if (!File.Exists(configFile))
                {
                    configFile = Path.Combine(baseDirectory, "App.config");
                    if (!File.Exists(configFile))
                    {
                        configFile = Path.Combine(baseDirectory, "Web.config");
                        if (!File.Exists(configFile))
                            throw new ConfigurationException("Could not resolve any legacy configuration for this application.");
                    }
                }
            }
            return configFile;
        }

        private LegacyConfiguration SerializeLegacyJsonConfiguration(string filename)
        {
            if (!File.Exists(filename))
                throw new ConfigurationException($"Could not load Json configuration at '{filename}'");
            var legacyConfiguration = new LegacyConfiguration();
            legacyConfiguration.Filename = filename;

            try
            {
                var json = File.ReadAllText(filename);
                var appSettings = new List<Models.AppSettingPair>();
                var connectionStrings = new List<Models.ConnectionStringPair>();
                var configSections = new List<Models.ConfigSectionPair>();
                var jsonParser = new JsonParser();
                var nodes = jsonParser.Parse(json);
                foreach (JsonNode node in nodes.ChildNodes)
                {
                    if (node.Name.Equals("AppSettings", StringComparison.InvariantCultureIgnoreCase))
                    {
                        foreach (JsonNode appSetting in node.ChildNodes)
                        {
                            appSettings.Add(new AppSettingPair { Key = appSetting.Name, Value = appSetting.Value });
                        }
                    }
                    else if (node.Name.Equals("ConnectionStrings", StringComparison.InvariantCultureIgnoreCase))
                    {
                        foreach (var connectionStringEntry in node.ChildNodes)
                        {
                            var propertyList = new Dictionary<string, string>();
                            foreach (JsonNode connectionString in connectionStringEntry.ChildNodes)
                            {
                                propertyList.Add(connectionString.Name, connectionString.Value);
                            }
                            connectionStrings.Add(new ConnectionStringPair
                            {
                                Name = propertyList.Where(x => x.Key.Equals("Name", StringComparison.InvariantCultureIgnoreCase)).Select(x => x.Value).FirstOrDefault(),
                                ConnectionStringSetting = new ConnectionStringSetting
                                {
                                    ConnectionString = propertyList.Where(x => x.Key.Equals("ConnectionString", StringComparison.InvariantCultureIgnoreCase)).Select(x => x.Value).FirstOrDefault(),
                                    Name = propertyList.Where(x => x.Key.Equals("Name", StringComparison.InvariantCultureIgnoreCase)).Select(x => x.Value).FirstOrDefault(),
                                    ProviderName = propertyList.Where(x => x.Key.Equals("ProviderName", StringComparison.InvariantCultureIgnoreCase)).Select(x => x.Value).FirstOrDefault()
                                }
                            });
                        }
                    }
                    else if (node.ValueType != PrimitiveTypes.Array && node.ValueType != PrimitiveTypes.Object)
                    {
                        // add unknown root elements to appsettings
                        if (!appSettings.Any(x => x.Key == node.Name))
                            appSettings.Add(new AppSettingPair { Key = node.Name, Value = node.Value });
                    }
                    else if (node.ValueType == PrimitiveTypes.Object)
                    {
                        configSections.Add(new ConfigSectionPair
                        {
                            Name = node.Name,
                            Configuration = node.OuterText,
                            Type = nameof(RequiresJsonSerialization),
                            TypeValue = typeof(RequiresJsonSerialization)
                        });
                    }
                }
                legacyConfiguration.Configuration.AppSettings = new SectionCollection<AppSettingPair>(appSettings);
                legacyConfiguration.Configuration.ConnectionStrings = new SectionCollection<ConnectionStringPair>(connectionStrings);
                legacyConfiguration.Configuration.ConfigSections = new SectionCollection<ConfigSectionPair>(configSections);
            }
            catch (Exception)
            {
                // failed to deserialize
            }

            return legacyConfiguration;
        }

        private LegacyConfiguration SerializeLegacyXmlConfiguration(string filename)
        {
            if (!File.Exists(filename))
                throw new ConfigurationException($"Could not load Xml configuration at '{filename}'");
            var config = new LegacyConfiguration();
            config.Filename = filename;
            var doc = new XmlDocument();
            doc.Load(filename);

            var configurationNode = doc.SelectSingleNode("configuration");
            if (configurationNode != null)
            {
                // load custom config section declarations
                var configSectionNodes = configurationNode.SelectSingleNode("configSections");
                if (configSectionNodes != null)
                {
                    var sections = configSectionNodes.SelectNodes("section");
                    if (sections != null)
                    {
                        foreach (XmlNode configSectionNode in sections)
                        {
                            var configSectionPair = new ConfigSectionPair
                            {
                                Name = configSectionNode.Attributes.GetNamedItem("name")?.InnerText,
                                Type = configSectionNode.Attributes.GetNamedItem("type")?.InnerText,
                            };
                            try
                            {
                                configSectionPair.TypeValue = ResolveType(configSectionNode.Attributes.GetNamedItem("type")?.InnerText);
                            }
                            catch (TypeLoadException)
                            {
                                // could not load this type
                            }
                            config.Configuration.ConfigSections.Add(configSectionPair);
                        }
                    }
                }

                if (configurationNode.ChildNodes != null)
                {
                    foreach (XmlNode xmlNode in configurationNode.ChildNodes)
                    {
                        config = ProcessNode(xmlNode, config);
                    }
                }
            }
            return config;
        }

        private LegacyConfiguration ProcessNode(XmlNode node, LegacyConfiguration config)
        {
            var configProtectionProvider = node.Attributes.GetNamedItem("configProtectionProvider");
            var sectionInformation = new SectionInformation(node.Name, configProtectionProvider?.Value);
            if (sectionInformation.IsProtected && node.ChildNodes.Count > 0)
                node = sectionInformation.ProtectionProvider.Decrypt(node.ChildNodes[0]);
            switch (node.Name.ToLower())
            {
                case "connectionstrings":

                    sectionInformation.Type = typeof(SectionCollection<ConnectionStringPair>).Name;
                    config.Configuration.ConnectionStrings.SectionInformation = sectionInformation;
                    var connectionstringsAddNodes = node.SelectNodes("add");
                    if (connectionstringsAddNodes != null)
                    {
                        foreach (XmlNode connectionStringNode in connectionstringsAddNodes)
                        {
                            config.Configuration.ConnectionStrings.Add(new ConnectionStringPair
                            {
                                Name = connectionStringNode.Attributes.GetNamedItem("name")?.InnerText,
                                ConnectionStringSetting = new ConnectionStringSetting
                                {
                                    Name = connectionStringNode.Attributes.GetNamedItem("name")?.InnerText,
                                    ConnectionString = connectionStringNode.Attributes.GetNamedItem("connectionString")?.InnerText,
                                    ProviderName = connectionStringNode.Attributes.GetNamedItem("providerName")?.InnerText
                                }
                            });
                        }
                    }
                    config.Configuration.ConfigSections.Add(new ConfigSectionPair
                    {
                        Name = "connectionStrings",
                        Configuration = config.Configuration.ConnectionStrings,
                        Type = config.Configuration.ConnectionStrings.GetType().Name,
                        TypeValue = config.Configuration.ConnectionStrings.GetType(),
                    });
                    break;
                case "appsettings":
                    sectionInformation.Type = typeof(SectionCollection<AppSettingPair>).Name;
                    config.Configuration.AppSettings.SectionInformation = sectionInformation;
                    var appsettingsAddNodes = node.SelectNodes("add");
                    if (appsettingsAddNodes != null)
                    {
                        foreach (XmlNode appSettingsNode in appsettingsAddNodes)
                        {
                            config.Configuration.AppSettings.Add(new AppSettingPair
                            {
                                Key = appSettingsNode.Attributes.GetNamedItem("key")?.InnerText,
                                Value = appSettingsNode.Attributes.GetNamedItem("value")?.InnerText,
                            });
                        }
                    }
                    config.Configuration.ConfigSections.Add(new ConfigSectionPair
                    {
                        Name = "appSettings",
                        Configuration = config.Configuration.AppSettings,
                        Type = config.Configuration.AppSettings.GetType().Name,
                        TypeValue = config.Configuration.AppSettings.GetType(),
                    });
                    break;
                case "anyconfig":
                    // custom anyconfig root appsettings. They are stored as groups with a single element
                    sectionInformation.Type = typeof(SectionCollection<AnyConfigGroup>).Name;
                    config.Configuration.AnyConfigGroups.SectionInformation = sectionInformation;
                    var anyconfigAddNodes = node.SelectNodes("add");
                    if (anyconfigAddNodes != null)
                    {
                        foreach (XmlNode appSettingsNode in anyconfigAddNodes)
                        {
                            var groupSettings = new List<AnyConfigAppSettingPair>();
                            config.Configuration.AnyConfigGroups.Add(new AnyConfigGroup
                            {
                                GroupName = appSettingsNode.Attributes.GetNamedItem("key")?.InnerText,
                                Settings = new List<AnyConfigAppSettingPair> { new AnyConfigAppSettingPair() { Key = appSettingsNode.Attributes.GetNamedItem("key")?.InnerText, Value = appSettingsNode.Attributes.GetNamedItem("value")?.InnerText } },
                            });
                        }
                    }

                    // custom anyconfig grouped appsettings
                    if (node.ChildNodes != null)
                    {
                        foreach (XmlNode anyConfigNode in node.ChildNodes)
                        {
                            var groupSettings = new List<AnyConfigAppSettingPair>();
                            foreach (XmlNode appSettingsNode in anyConfigNode.SelectNodes("add"))
                            {
                                groupSettings.Add(new AnyConfigAppSettingPair
                                {
                                    Key = appSettingsNode.Attributes.GetNamedItem("key")?.InnerText,
                                    Value = appSettingsNode.Attributes.GetNamedItem("value")?.InnerText,
                                });
                            }
                            if (groupSettings.Any())
                            {
                                config.Configuration.AnyConfigGroups.Add(new AnyConfigGroup
                                {
                                    GroupName = anyConfigNode.Name,
                                    Settings = groupSettings,
                                });
                            }
                        }
                    }
                    break;
                default:
                    // if its a declared configsection, serialize the type
                    var configSectionDeclaration = config.Configuration.ConfigSections.FirstOrDefault(x => x.Name == node.Name);
                    if (configSectionDeclaration != null)
                    {
                        config.Configuration.ConfigSections.SectionInformation = sectionInformation;
                        try
                        {
                            if (configSectionDeclaration.TypeValue != null)
                            {
                                sectionInformation.Type = configSectionDeclaration.TypeValue.Name;
                                if (IsConfigurationSectionHandler(configSectionDeclaration))
                                    ExecuteConfigurationSectionHandler(configSectionDeclaration, node);
                                else if (IsConfigurationSection(configSectionDeclaration))
                                    ConfigureConfigurationSection(configSectionDeclaration, node);
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new ConfigurationException($"Unable to configure configSection named '{configSectionDeclaration.Name}'. See the inner exception for more details.", ex);
                        }
                    }
                    else
                    {
                        // unknown config type, it needs to be deserialized
                        sectionInformation.Type = typeof(RequiresXmlSerialization).Name;
                        var configSection = new ConfigSectionPair
                        {
                            Configuration = node.OuterXml,
                            Name = node.Name,
                            Type = nameof(RequiresXmlSerialization),
                            TypeValue = typeof(RequiresXmlSerialization)
                        };
                        config.Configuration.ConfigSections.Add(configSection);
                    }
                    break;
            }
            return config;
        }

        private bool IsConfigurationSection(ConfigSectionPair configSection)
        {
            var extendedType = configSection.TypeValue.GetExtendedType();
            return extendedType.BaseTypes.Any(x => x.FullName.Equals("System.Configuration.ConfigurationSection"));
        }

        private void ConfigureConfigurationSection(ConfigSectionPair configSection, XmlNode node)
        {
            var configurationSection = new ObjectFactory().CreateEmptyObject(configSection.TypeValue);
            var methods = configurationSection.GetMethods(MethodOptions.All);
            var deserializeMethod = methods.FirstOrDefault(x => x.Name == "DeserializeSection");
            if (deserializeMethod == null)
                throw new ConfigurationException($"The configSection named '{configSection.Name}' does not implement ConfigurationSection.DeserializeSection.");
            try
            {
                // invoke the section xml deserialization handler
                using (var stream = new MemoryStream())
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write(node.OuterXml);
                        writer.Flush();
                        stream.Position = 0;
                        var xmlReader = XmlReader.Create(stream);
                        var handler = deserializeMethod.MethodInfo.Invoke(configurationSection, new object[] { xmlReader });
                        configSection.Configuration = configurationSection;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ConfigurationException($"Unable to obtain configuration for configSection named '{configSection.Name}'.", ex);
            }
        }

        private bool IsConfigurationSectionHandler(ConfigSectionPair configSection)
        {
            var configSectionHandler = new ObjectFactory().CreateEmptyObject(configSection.TypeValue);
            var methods = configSectionHandler.GetMethods(MethodOptions.All);
            var createMethod = methods.FirstOrDefault(x => x.Name.Equals("System.Configuration.IConfigurationSectionHandler.Create"));
            return createMethod != null;
        }

        private void ExecuteConfigurationSectionHandler(ConfigSectionPair configSection, XmlNode node)
        {
            var configSectionHandler = new ObjectFactory().CreateEmptyObject(configSection.TypeValue);
            var methods = configSectionHandler.GetMethods(MethodOptions.All);
            var createMethod = methods.FirstOrDefault(x => x.Name.Equals("System.Configuration.IConfigurationSectionHandler.Create"));
            if (createMethod == null)
                throw new ConfigurationException($"The configSection named '{configSection.Name}' does not implement IConfigurationSectionHandler.");
            try
            {
                // invoke the section handler
                var handler = createMethod.MethodInfo.Invoke(configSectionHandler, new object[] { null, null, node });
                configSection.Configuration = handler;
            }
            catch (Exception ex)
            {
                throw new ConfigurationException($"Unable to obtain configuration for configSection named '{configSection.Name}'.", ex);
            }
        }

        private Type ResolveType(string typeName)
        {
            return Type.GetType(typeName, true, true);
        }
    }
}
