namespace AnyConfig.Tests.Models
{
    /// <summary>
    /// Certificate load type
    /// </summary>
    public enum LoadCertificateType
    {
        /// <summary>
        /// Use the built in developer certificate
        /// </summary>
        Embedded,
        /// <summary>
        /// Use a certificate from the certificate store
        /// </summary>
        Store,
        /// <summary>
        /// Use a certificate located in the file system
        /// </summary>
        File
    }
}
