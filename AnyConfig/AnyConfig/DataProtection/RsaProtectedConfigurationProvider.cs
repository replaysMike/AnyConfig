using System;
using System.Collections.Specialized;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace AnyConfig
{
    /// <summary>
    /// Data protection provider that uses the Rsa provider
    /// </summary>
    public class RsaProtectedConfigurationProvider : ProtectedConfigurationProvider
    {
        const string DefaultRsaKeyContainerName = "NetFrameworkConfigurationKey";

        private string _KeyName;
        private string _KeyContainerName;
        private string _CspProviderName;
        private bool _UseMachineContainer;
        private bool _UseOAEP;
        private string _KeyEntropy;

        public string KeyContainerName { get { return _KeyContainerName; } }
        public string CspProviderName { get { return _CspProviderName; } }
        public bool UseMachineContainer { get { return _UseMachineContainer; } }
        public bool UseOAEP { get { return _UseOAEP; } }

        public RsaProtectedConfigurationProvider()
        {

        }

        /// <summary>
        /// Decrypts the passed XmlNode object from a configuration file.
        /// </summary>
        /// <param name="encryptedNode"></param>
        /// <returns></returns>
        public override XmlNode Decrypt(XmlNode encryptedNode)
        {
            var xmlDocument = new XmlDocument();
            var rsa = GetCryptoServiceProvider(false, true);

            xmlDocument.PreserveWhitespace = true;
            xmlDocument.LoadXml(encryptedNode.OuterXml);
            var exml = new EncryptedXml(xmlDocument);
            exml.AddKeyNameMapping(_KeyName, rsa);
            exml.DecryptDocument();
            rsa.Clear();
            return xmlDocument.DocumentElement;
        }

        /// <summary>
        /// Encrypts the passed XmlNode object from a configuration file.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override XmlNode Encrypt(XmlNode node)
        {
            var rsa = GetCryptoServiceProvider(false, false);


            // Encrypt the node with the new key
            var xmlDocument = new XmlDocument
            {
                PreserveWhitespace = true
            };
            xmlDocument.LoadXml("<foo>" + node.OuterXml + "</foo>");
            var exml = new EncryptedXml(xmlDocument);
            var inputElement = xmlDocument.DocumentElement;

            // Create a new 3DES key
            var symAlg = new TripleDESCryptoServiceProvider();
            var rgbKey1 = GetRandomKey();
            symAlg.Key = rgbKey1;
            symAlg.Mode = CipherMode.ECB;
            symAlg.Padding = PaddingMode.PKCS7;
            var rgbOutput = exml.EncryptData(inputElement, symAlg, true);
            var ed = new EncryptedData
            {
                Type = EncryptedXml.XmlEncElementUrl,
                EncryptionMethod = new EncryptionMethod(EncryptedXml.XmlEncTripleDESUrl),
                KeyInfo = new KeyInfo()
            };

            var ek = new EncryptedKey
            {
                EncryptionMethod = new EncryptionMethod(EncryptedXml.XmlEncRSA15Url),
                KeyInfo = new KeyInfo(),
                CipherData = new CipherData()
            };
            ek.CipherData.CipherValue = EncryptedXml.EncryptKey(symAlg.Key, rsa, UseOAEP);
            var kin = new KeyInfoName
            {
                Value = _KeyName
            };
            ek.KeyInfo.AddClause(kin);
            var kek = new KeyInfoEncryptedKey(ek);
            ed.KeyInfo.AddClause(kek);
            ed.CipherData = new CipherData
            {
                CipherValue = rgbOutput
            };
            EncryptedXml.ReplaceElement(inputElement, ed, true);
            // Get node from the document 
            foreach (XmlNode node2 in xmlDocument.ChildNodes)
                if (node2.NodeType == XmlNodeType.Element)
                    foreach (XmlNode node3 in node2.ChildNodes) // node2 is the "foo" node 
                        if (node3.NodeType == XmlNodeType.Element)
                            return node3; // node3 is the "EncryptedData" node
            return null;
        }

        public void AddKey(int keySize, bool exportable)
        {
            var rsa = GetCryptoServiceProvider(exportable, false);
            rsa.KeySize = keySize;
            rsa.PersistKeyInCsp = true;
            rsa.Clear();
        }

        public void DeleteKey()
        {
            var rsa = GetCryptoServiceProvider(false, true);
            rsa.PersistKeyInCsp = false;
            rsa.Clear();
        }
        public void ImportKey(string xmlFileName, bool exportable)
        {
            var rsa = GetCryptoServiceProvider(exportable, false);
            rsa.FromXmlString(File.ReadAllText(xmlFileName));
            rsa.PersistKeyInCsp = true;
            rsa.Clear();
        }

        public void ExportKey(string xmlFileName, bool includePrivateParameters)
        {
            var rsa = GetCryptoServiceProvider(false, false);
            var xmlString = rsa.ToXmlString(includePrivateParameters);
            File.WriteAllText(xmlFileName, xmlString);
            rsa.Clear();
        }

        public override void Initialize(string name, NameValueCollection configurationValues)
        {
            base.Initialize(name, configurationValues);

            _KeyName = "Rsa Key";
            _KeyContainerName = _configurationValues["keyContainerName"];
            _configurationValues.Remove("keyContainerName");
            if (_KeyContainerName == null || _KeyContainerName.Length < 1)
                _KeyContainerName = DefaultRsaKeyContainerName;

            _CspProviderName = _configurationValues["cspProviderName"];
            _configurationValues.Remove("cspProviderName");
            _UseMachineContainer = GetBooleanValue(_configurationValues, "useMachineContainer", true);
            _UseOAEP = GetBooleanValue(_configurationValues, "useOAEP", false);
            if (_configurationValues.Count > 0)
                throw new Exception($"Unrecognized_initialization_value {_configurationValues.GetKey(0)}");
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

        private RSACryptoServiceProvider GetCryptoServiceProvider(bool exportable, bool keyMustExist)
        {
            try
            {
                var csp = new CspParameters
                {
                    KeyContainerName = KeyContainerName,
                    KeyNumber = 1,
                    ProviderType = 1 // Dev10 Bug #548719: Explicitly require "RSA Full (Signature and Key Exchange)"
                };

                if (CspProviderName != null && CspProviderName.Length > 0)
                    csp.ProviderName = CspProviderName;

                if (UseMachineContainer)
                    csp.Flags |= CspProviderFlags.UseMachineKeyStore;
                if (!exportable && !keyMustExist)
                    csp.Flags |= CspProviderFlags.UseNonExportableKey;
                if (keyMustExist)
                    csp.Flags |= CspProviderFlags.UseExistingKey;

                return new RSACryptoServiceProvider(csp);

            }
            catch
            {
                throw;
            }
        }

        private byte[] GetRandomKey()
        {
            var buf = new byte[24];
            (new RNGCryptoServiceProvider()).GetBytes(buf);
            return buf;
        }
    }
}
