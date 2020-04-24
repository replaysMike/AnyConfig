using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace AnyConfig.Tests.Models
{
    [LegacyConfigurationName(prependChildrenName: "Authorization")]
    public class AuthorizationClientConfiguration
    {
        /// <summary>
        /// The endpoint of the Authorization Server
        /// </summary>
        public string Endpoint { get; set; } = "https://localhost:5433";

        /// <summary>
        /// The certificate configuration for authenticating
        /// </summary>
        [LegacyConfigurationName(childrenMapped: true)]
        public CertificateConfiguration CertificateConfiguration { get; set; }

        /// <summary>
        /// Enable caching of requests on the client side
        /// </summary>
        public bool UseClientSideCaching { get; set; } = true;

        /// <summary>
        /// Enable caching of requests on the client side
        /// </summary>
        public CachingProvider CachingProvider { get; set; } = CachingProvider.InMemory;

        /// <summary>
        /// Specify the redis configuration when using <seealso cref="CachingProvider.Redis"/>
        /// </summary>
        public string RedisConfiguration { get; set; }

        /// <summary>
        /// True to allow connecting to servers with untrusted certificates
        /// </summary>
        public bool AllowUntrustedCertificates { get; set; } = false;

        public AuthorizationClientConfiguration()
        {
            CertificateConfiguration = new CertificateConfiguration();
            //CertificateConfiguration.EmbeddedServerCertificate = new EmbeddedCertificate(EmbeddedCertificates.AuthServerCertificateName, EmbeddedCertificates.EmbeddedCertificatePassword);
            //CertificateConfiguration.EmbeddedClientCertificate = new EmbeddedCertificate(EmbeddedCertificates.AuthClientCertificateName, EmbeddedCertificates.EmbeddedCertificatePassword);
        }
    }

    /// <summary>
    /// Certificate configuration
    /// </summary>
    public class CertificateConfiguration
    {
        /// <summary>
        /// The type of certificate to load
        /// </summary>
        public LoadCertificateType CertificateType { get; set; } = LoadCertificateType.Embedded;

        /// <summary>
        /// The type of embedded certificate to load
        /// </summary>
        public EmbeddedCertificateType EmbeddedCertificateType { get; set; } = EmbeddedCertificateType.Client;

        /// <summary>
        /// The certificate store to retrieve the certificate from (Root, My)
        /// Default: Root
        /// </summary>
        public string Store { get; set; } = "Root";

        /// <summary>
        /// The certificate store to retrieve the certificate from (LocalMachine, CurrentUser)
        /// Default: CurrentUser
        /// </summary>
        public string StoreLocation { get; set; } = "CurrentUser";

        /// <summary>
        /// Load certificate by Issuer Name
        /// </summary>
        public string IssuerName { get; set; }

        /// <summary>
        /// Load certificate by Subject Name
        /// </summary>
        public string SubjectName { get; set; }

        /// <summary>
        /// Load certificate by Friendly Name
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Load certificate by Serial Number
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// Load certificate by Thumbprint
        /// </summary>
        public string Thumbprint { get; set; }

        /// <summary>
        /// The embedded server certificate
        /// </summary>
        public EmbeddedCertificate EmbeddedServerCertificate { get; set; }

        /// <summary>
        /// The embedded client certificate
        /// </summary>
        public EmbeddedCertificate EmbeddedClientCertificate { get; set; }
    }

    /// <summary>
    /// Embedded certificates come from an assembly rather than a certificate store. Should only be used on development environments
    /// </summary>
    public class EmbeddedCertificate
    {
        /// <summary>
        /// The assembly containing the embedded certificates
        /// </summary>
        //public static readonly Assembly EmbeddedCertificateAssembly = Assembly.GetAssembly(typeof(ITN.Standard.Certificates.EmbeddedCertificates));

        /// <summary>
        /// The assembly the embedded certificate is located in
        /// </summary>
        public Assembly Assembly { get; set; }

        /// <summary>
        /// Certificate embedded filename
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Certificate password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Create a new embedded certificate using the default certificates from <seealso cref="ITN.Standard.Certificates"/> assembly
        /// </summary>
        /// <param name="filename">Certificate embedded filename</param>
        /// <param name="password">Certificate password</param>
        public EmbeddedCertificate(string filename, string password) : this(Assembly.GetExecutingAssembly(), filename, password)
        {
        }

        /// <summary>
        /// Create a new embedded certificate
        /// </summary>
        /// <param name="assembly">The assembly the embedded certificate is located in</param>
        /// <param name="filename">Certificate embedded filename</param>
        /// <param name="password">Certificate password</param>
        public EmbeddedCertificate(Assembly assembly, string filename, string password)
        {
            Assembly = assembly;
            Filename = filename;
            Password = password;
        }

        /// <summary>
        /// Create a new embedded certificate
        /// </summary>
        /// <param name="assemblyName">The assembly the embedded certificate is located in. Usually 'ITN.Standard.Certificates'</param>
        /// <param name="filename">Certificate embedded filename</param>
        /// <param name="password">Certificate password</param>
        public EmbeddedCertificate(string assemblyName, string filename, string password)
        {
            try
            {
                Assembly = Assembly.Load(assemblyName);
            }
            catch (Exception ex)
            {
                // the quick fix: You need to add a reference to project ITN.Standard.Certificates from the application that is running
                throw new FileLoadException($"Could not locate assembly named '{assemblyName}' to load embedded certificates. Please ensure it is referenced by the application calling this library.", ex);
            }
            Filename = filename;
            Password = password;
        }
    }

    /// <summary>
    /// The type of embedded certificate to use
    /// </summary>
    public enum EmbeddedCertificateType
    {
        /// <summary>
        /// Use the embedded server certificate
        /// </summary>
        Server,
        /// <summary>
        /// Use the embedded client certificate
        /// </summary>
        Client
    }

    public enum CachingProvider
    {
        /// <summary>
        /// Use in-memory caching
        /// </summary>
        InMemory,
        /// <summary>
        /// Use Redis caching
        /// </summary>
        Redis
    }
}
