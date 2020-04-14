namespace AnyConfig.Tests.Models
{
    public class SecurityConfiguration
    {
        /// <summary>
        /// Get the minimum password complexity allowed of a password
        /// </summary>
        public int MinimumPasswordComplexity { get; set; } = 3;

        /// <summary>
        /// Get the total size of the combined salt size for hashed tenant data
        /// </summary>
        public int CombinedTenantDataSaltSize { get; set; } = 96;

        /// <summary>
        /// Get the total size of the a single user password salt (1 of 3)
        /// </summary>
        public int IndividualUserPasswordSaltSize { get; set; } = 32;

        /// <summary>
        /// Get the total size of the combined salt size for user passwords
        /// </summary>
        public int CombinedUserPasswordSaltSize { get; set; } = 96;

        /// <summary>
        /// This is the master encryption key for database data. Should be 32 bytes (256 bit).
        /// </summary>
        public string MasterDataEncryptionKey { get; set; }

        /// <summary>
        /// The master data salt. All hashed data will use this salt. If the hashed data belongs to a tenant, then this salt will be combined with the tenant salt.
        /// </summary>
        public string MasterDataSalt { get; set; }

        /// <summary>
        /// The master user password salt. All hashed user passwords will use this salt. If the hashed password belongs to a tenant, then this salt will be combined with the tenant salt.
        /// </summary>
        public string MasterUserPasswordSalt { get; set; }
    }
}
