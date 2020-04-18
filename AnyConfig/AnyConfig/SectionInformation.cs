using System;

namespace AnyConfig
{
    public class SectionInformation
    {
        /// <summary>
        /// Gets or sets a value that indicates whether the associated configuration section will be saved even if it has not been modified.
        /// </summary>
        public bool ForceSave { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether a change in an external configuration include file requires an application restart.
        /// </summary>
        public bool RestartOnExternalChanges { get; set; }

        /// <summary>
        /// Gets a value that indicates whether the associated configuration section requires access permissions.
        /// </summary>
        public bool RequirePermission { get; set; }

        /// <summary>
        /// Gets the protected configuration provider for the associated configuration section.
        /// </summary>
        public ProtectedConfigurationProvider ProtectionProvider { get; private set; }

        /// <summary>
        /// Gets the name of the associated configuration section.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets a value that indicates whether the associated configuration section is protected.
        /// </summary>
        public bool IsProtected { get; private set; }

        /// <summary>
        /// Gets a value that indicates whether the associated configuration section is locked.
        /// </summary>
        public bool IsLocked { get; private set; }

        /// <summary>
        /// Gets a value that indicates whether the associated configuration section is declared in the configuration file.
        /// </summary>
        public bool IsDeclared { get; private set; }

        /// <summary>
        /// Gets a value that indicates whether the configuration section must be declared in the configuration file.
        /// </summary>
        public bool IsDeclarationRequired { get; private set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the settings that are specified in the associated configuration section are inherited by applications that reside in a subdirectory of the relevant application.
        /// </summary>
        public bool InheritInChildApplications { get; set; }

        /// <summary>
        /// Gets or sets the section class name.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the name of the include file in which the associated configuration section is defined, if such a file exists.
        /// </summary>
        public string ConfigSource { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the associated configuration section can be overridden by lower-level configuration files.
        /// </summary>
        public bool AllowOverride { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the configuration section allows the location attribute.
        /// </summary>
        public bool AllowLocation { get; set; }

        /// <summary>
        /// Gets the name of the associated configuration section.
        /// </summary>
        public string SectionName { get; private set; }

        public SectionInformation() { }

        public SectionInformation(string sectionName, string protectionProvider) : this(sectionName, typeof(object), protectionProvider)
        {

        }

        public SectionInformation(string sectionName, Type sectionType, string protectionProvider)
        {
            Name = sectionName;
            SectionName = sectionName;
            Type = sectionType.Name;

            if (!string.IsNullOrEmpty(protectionProvider))
            {
                IsProtected = true;
                switch (protectionProvider.ToLower())
                {
                    case "rsaprotectedconfigurationprovider":
                        ProtectionProvider = new RsaProtectedConfigurationProvider();
                        break;
                    case "dataprotectionconfigurationprovider":
                    case "dpapiprotectedconfigurationprovider":
                        ProtectionProvider = new DpapiProtectedConfigurationProvider();
                        break;
                    default:
                        throw new NotSupportedException("Custom data protection providers are not supported. Please choose one of the default protection providers: DataProtectionConfigurationProvider, RsaProtectedConfigurationProvider");
                }
                ProtectionProvider.Initialize(protectionProvider, null);
            }
        }
    }
}
