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
    public class LegacyConfigurationLoader
    {
        private Assembly _entryAssembly;
        public LegacyConfigurationLoader(Assembly entryAssembly)
        {
            _entryAssembly = entryAssembly;
        }

        public LegacyConfiguration LoadDotNetCoreLegacyConfiguration(string filename = null)
        {
            var configFile = ResolveConfigFile(filename);
            return SerializeLegacyJsonConfiguration(configFile);
        }

        public LegacyConfiguration LoadDotNetFrameworkLegacyConfiguration(string filename = null)
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
            var legacyConfiguration = new LegacyConfiguration();

            try
            {
                var json = File.ReadAllText(filename);
                var appSettings = new List<Models.AppSettingPair>();
                var connectionStrings = new List<Models.ConnectionStringPair>();
                var configSections = new List<Models.ConfigSectionPair>();
                var jsonParser = new JsonParserV4();
                var nodes = jsonParser.Parse(json);
                foreach (var node in nodes.ChildNodes)
                {
                    if (node.Name.Equals("AppSettings", StringComparison.InvariantCultureIgnoreCase))
                    {
                        foreach (var appSetting in node.ChildNodes)
                        {
                            appSettings.Add(new AppSettingPair { Key = appSetting.Name, Value = appSetting.Value });
                        }
                    }
                    else if (node.Name.Equals("ConnectionStrings", StringComparison.InvariantCultureIgnoreCase))
                    {
                        foreach (var connectionStringEntry in node.ChildNodes)
                        {
                            var propertyList = new Dictionary<string, string>();
                            foreach (var connectionString in connectionStringEntry.ChildNodes)
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
                    else if(node.ValueType == PrimitiveTypes.Object)
                    {
                        configSections.Add(new ConfigSectionPair { 
                            Name = node.Name, 
                            Configuration = node.Json,
                            Type = "RequiresJsonSerialization",
                            TypeValue = typeof(RequiresJsonSerialization)
                        });
                    }
                }
                legacyConfiguration.Configuration.AppSettings = appSettings;
                legacyConfiguration.Configuration.ConnectionStrings = connectionStrings;
                legacyConfiguration.Configuration.ConfigSections = configSections;
            }
            catch (Exception)
            {
                // failed to deserialize
            }

            return legacyConfiguration;
        }

        private LegacyConfiguration SerializeLegacyXmlConfiguration(string filename)
        {
            var config = new LegacyConfiguration();
            var doc = new XmlDocument();
            doc.Load(filename);

            var configurationNode = doc.SelectSingleNode("configuration");
            // load custom config section declarations
            var configSectionNodes = configurationNode.SelectSingleNode("configSections");
            foreach (XmlNode configSectionNode in configSectionNodes.SelectNodes("section"))
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


            foreach (XmlNode xmlNode in configurationNode.ChildNodes)
            {
                config = ProcessNode(xmlNode, config);
            }

            return config;
        }

        private LegacyConfiguration ProcessNode(XmlNode node, LegacyConfiguration config)
        {
            switch (node.Name.ToLower())
            {
                case "connectionstrings":
                    foreach (XmlNode connectionStringNode in node.SelectNodes("add"))
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
                    break;
                case "appsettings":
                    foreach (XmlNode appSettingsNode in node.SelectNodes("add"))
                    {
                        config.Configuration.AppSettings.Add(new AppSettingPair
                        {
                            Key = appSettingsNode.Attributes.GetNamedItem("key")?.InnerText,
                            Value = appSettingsNode.Attributes.GetNamedItem("value")?.InnerText,
                        });
                    }
                    break;
                case "anyconfig":
                    // custom anyconfig root appsettings. They are stored as groups with a single element
                    foreach (XmlNode appSettingsNode in node.SelectNodes("add"))
                    {
                        var groupSettings = new List<AnyConfigAppSettingPair>();
                        config.Configuration.AnyConfigGroups.Add(new AnyConfigGroup
                        {
                            GroupName = appSettingsNode.Attributes.GetNamedItem("key")?.InnerText,
                            Settings = new List<AnyConfigAppSettingPair> { new AnyConfigAppSettingPair() { Key = appSettingsNode.Attributes.GetNamedItem("key")?.InnerText, Value = appSettingsNode.Attributes.GetNamedItem("value")?.InnerText } },
                        });
                    }

                    // custom anyconfig grouped appsettings
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
                    break;
                default:
                    // if its a declared configsection, serialize the type
                    var configSectionDeclaration = config.Configuration.ConfigSections.FirstOrDefault(x => x.Name == node.Name);
                    if (configSectionDeclaration != null)
                    {
                        try
                        {
                            if (configSectionDeclaration.TypeValue != null)
                            {
                                var configSectionHandler = new ObjectFactory().CreateEmptyObject(configSectionDeclaration.TypeValue);
                                var methods = configSectionHandler.GetMethods(MethodOptions.All);
                                var createMethod = methods.FirstOrDefault(x => x.Name == "System.Configuration.IConfigurationSectionHandler.Create");
                                if (createMethod == null)
                                    throw new ConfigurationException($"The configSection named '{configSectionDeclaration.Name}' does not implement IConfigurationSectionHandler.");
                                try
                                {
                                    // invoke the section handler
                                    var configSection = createMethod.MethodInfo.Invoke(configSectionHandler, new object[] { null, null, node });
                                    configSectionDeclaration.Configuration = configSection;
                                }
                                catch (Exception ex)
                                {
                                    throw new ConfigurationException($"Unable to obtain configuration for configSection named '{configSectionDeclaration.Name}'.", ex);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new ConfigurationException($"Unable to configure configSection named '{configSectionDeclaration.Name}'. See the inner exception for more details.", ex);
                        }
                    }
                    break;
            }
            return config;
        }

        private Type ResolveType(string typeName)
        {
            return Type.GetType(typeName, true, true);
        }
    }
}
