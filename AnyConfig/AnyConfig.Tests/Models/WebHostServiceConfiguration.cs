using System.Collections.Generic;

namespace AnyConfig.Tests.Models
{
    public class WebHostServiceConfiguration
    {
        /// <summary>
        /// The server name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The server ip to bind to
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// The port number to host
        /// </summary>
        public int Port { get; set; } = 4433;

        /// <summary>
        /// True to enable server side caching
        /// </summary>
        public bool AllowCaching { get; set; }

        /// <summary>
        /// Maximum number of items to cache
        /// </summary>
        public int MaxCacheItems { get; set; } = 1024;

        /// <summary>
        /// The number of minutes to set the sliding expiration cache to (default: 30)
        /// </summary>
        public int CacheSlidingExpirationMinutes { get; set; } = 30;

        /// <summary>
        /// The number of minutes to set the absolute expiration to (default: 0)
        /// </summary>
        public int CacheAbsoluteExpirationMinutes { get; set; } = 0;

        /// <summary>
        /// The certificate type to use for the server
        /// </summary>
        public LoadCertificateType CertificateType { get; set; }

        /// <summary>
        /// The Issuer name of the certificate to load, for CertificateType=Store
        /// </summary>
        public string CertificateIssuerName { get; set; }

        /// <summary>
        /// The Friendly name of the certificate to load, for CertificateType=Store
        /// </summary>
        public string CertificateFriendlyName { get; set; }

        /// <summary>
        /// The full path to the certificate store location (Eg. LocalMachine\Root)
        /// </summary>
        public string CertificateStore { get; set; } = @"CurrentUser\Root";

        /// <summary>
        /// The origin to allow for Cors
        /// </summary>
        public string CorsAllowOrigin { get; set; }

        /// <summary>
        /// List of IP addresses authorized to use the service
        /// </summary>
        public List<string> AuthorizedIPs { get; set; }
    }
}
