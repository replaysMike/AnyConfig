using System;

namespace AnyConfig
{
    /// <summary>
    /// Specify the name of the legacy configuration setting name (App.config, Web.config)
    /// </summary>
    public class LegacyConfigurationNameAttribute : Attribute
    {
        /// <summary>
        /// The name of the legacy configuration setting
        /// </summary>
        public string SettingName { get; }

        /// <summary>
        /// A value to always prepend to its children
        /// </summary>
        public string PrependChildrenName { get; }

        /// <summary>
        /// True to map an objects children
        /// </summary>
        public bool ChildrenMapped { get; }

        /// <summary>
        /// True if property is required to be configured
        /// </summary>
        public bool IsRequired { get; }

        /// <summary>
        /// Specify the name of the legacy configuration setting name (App.config, Web.config)
        /// </summary>
        /// <param name="settingName">The name of the legacy configuration setting</param>
        /// <param name="prependChildrenName">A value to always prepend to its children</param>
        /// <param name="childrenMapped">True to map an objects children</param>
        /// <param name="isRequired">True if property is required to be configured</param>
        public LegacyConfigurationNameAttribute(string settingName = null, string prependChildrenName = null, bool childrenMapped = false, bool isRequired = false)
        {
            SettingName = settingName;
            PrependChildrenName = prependChildrenName;
            ChildrenMapped = childrenMapped;
            IsRequired = isRequired;
        }
    }
}
