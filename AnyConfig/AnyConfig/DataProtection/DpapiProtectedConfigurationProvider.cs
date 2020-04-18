using System;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace AnyConfig
{
    /// <summary>
    /// Data protection provider that uses the Windows DPAPI provider
    /// </summary>
    public class DpapiProtectedConfigurationProvider : ProtectedConfigurationProvider
    {
        private string _KeyEntropy;

        public DpapiProtectedConfigurationProvider()
        {

        }

        /// <summary>
        /// Decrypts the passed XmlNode object from a configuration file.
        /// </summary>
        /// <param name="encryptedNode"></param>
        /// <returns></returns>
        public override XmlNode Decrypt(XmlNode encryptedNode)
        {
            if (encryptedNode.NodeType != XmlNodeType.Element || encryptedNode.Name != "EncryptedData")
                throw new Exception("DPAPI_bad_data");

            var cipherNode = TraverseToChild(encryptedNode, "CipherData", false);
            if (cipherNode == null)
                throw new Exception("DPAPI_bad_data");

            var cipherValue = TraverseToChild(cipherNode, "CipherValue", true);
            if (cipherValue == null)
                throw new Exception("DPAPI_bad_data");

            var encText = cipherValue.InnerText;
            if (encText == null)
                throw new Exception("DPAPI_bad_data");

            var decText = DecryptText(encText);
            var xmlDocument = new XmlDocument();
            xmlDocument.PreserveWhitespace = true;
            xmlDocument.LoadXml(decText);
            return xmlDocument.DocumentElement;
        }

        /// <summary>
        /// Encrypts the passed XmlNode object from a configuration file.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override XmlNode Encrypt(XmlNode node)
        {
            var text = node.OuterXml;
            var encText = EncryptText(text);
            var pre = @"<encrypteddata><cipherdata><ciphervalue>";
            var post = @"</ciphervalue></cipherdata></encrypteddata>";
            var xmlText = pre + encText + post;

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.PreserveWhitespace = true;
            xmlDocument.LoadXml(xmlText);
            return xmlDocument.DocumentElement;
        }

        public override void Initialize(string name, NameValueCollection configurationValues)
        {
            base.Initialize(name, configurationValues);

            _KeyEntropy = _configurationValues["keyEntropy"];
            if (_KeyEntropy != null)
            {
                _configurationValues.Remove("keyEntropy");
            }
            else
            {
                _KeyEntropy = string.Empty;
            }
            if (_configurationValues.Count > 0)
                throw new Exception($"Unrecognized_initialization_value {configurationValues.GetKey(0)}");
        }

        private static XmlNode TraverseToChild(XmlNode node, string name, bool onlyChild)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element)
                    continue;

                if (child.Name == name)
                    return child; // found it! 

                if (onlyChild)
                    return null;
            }

            return null;
        }

        private string EncryptText(string clearText)
        {
            var optionalEntropy = Encoding.Unicode.GetBytes(_KeyEntropy);
            var data = Encoding.Unicode.GetBytes(clearText);
            var encryptedBytes = ProtectedData.Protect(data, optionalEntropy, UseMachineProtection ? DataProtectionScope.LocalMachine : DataProtectionScope.CurrentUser);
            var encryptedText = Convert.ToBase64String(encryptedBytes);
            return encryptedText;
        }

        private string DecryptText(string encText)
        {
            var optionalEntropy = Encoding.Unicode.GetBytes(_KeyEntropy);
            var data = Convert.FromBase64String(encText);
            var decryptedBytes = ProtectedData.Unprotect(data, optionalEntropy, UseMachineProtection ? DataProtectionScope.LocalMachine : DataProtectionScope.CurrentUser);
            var decryptedText = Encoding.Unicode.GetString(decryptedBytes);
            return decryptedText;
        }
    }
}
