using System;
using System.Collections.Specialized;
using System.Xml;

namespace AnyConfig
{
    /// <summary>
    /// Base data protection provider
    /// </summary>
    public class ProtectedConfigurationProvider
    {
        protected NameValueCollection _configurationValues;
        protected bool UseMachineProtection { get; private set; } = true;

        /// <summary>
        /// Gets a brief, friendly description suitable for display in administrative tools or other user interfaces (UIs).
        /// </summary>
        public string Description { get; private set; }
        
        /// <summary>
        /// Gets the friendly name used to refer to the provider during configuration.
        /// </summary>
        public string Name { get; private set; }

        public ProtectedConfigurationProvider()
        {

        }

        /// <summary>
        /// Decrypts the passed XmlNode object from a configuration file.
        /// </summary>
        /// <param name="encryptedNode"></param>
        /// <returns></returns>
        public virtual XmlNode Decrypt(XmlNode encryptedNode)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Encrypts the passed XmlNode object from a configuration file.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public virtual XmlNode Encrypt(XmlNode node)
        {
            throw new NotImplementedException();
        }

        public virtual void Initialize(string name, NameValueCollection configurationValues)
        {
            Name = name;
            _configurationValues = configurationValues ?? new NameValueCollection();
            UseMachineProtection = GetBooleanValue(configurationValues, "useMachineProtection", true);
        }

        protected static bool GetBooleanValue(NameValueCollection configurationValues, string valueName, bool defaultValue)
        {
            if (configurationValues == null)
                return defaultValue;

            var s = configurationValues[valueName];
            if (s == null)
                return defaultValue;
            configurationValues.Remove(valueName);
            if (s == "true")
                return true;
            if (s == "false")
                return false;
            
            throw new Exception($"Config_invalid_boolean_attribute {valueName}");
        }
    }
}
