namespace AnyConfig
{
    /// <summary>
    /// Used to specify which configuration file is to be represented by the Configuration object.
    /// </summary>
    public enum ConfigurationUserLevel
    {
        /// <summary>
        /// Gets the System.Configuration.Configuration that applies to all users.
        /// </summary>
        None = 0,
        /// <summary>
        /// Gets the roaming System.Configuration.Configuration that applies to the current user.
        /// </summary>
        PerUserRoaming = 10,
        /// <summary>
        /// Gets the local System.Configuration.Configuration that applies to the current user.
        /// </summary>
        PerUserRoamingAndLocal = 20
    }
}
