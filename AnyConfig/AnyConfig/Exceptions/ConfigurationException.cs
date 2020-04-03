using System;

namespace AnyConfig.Exceptions
{
    public class ConfigurationException : Exception
    {
        public ConfigurationException(string errorMessage) : base(errorMessage) { }
        public ConfigurationException(string errorMessage, Exception innerException) : base(errorMessage, innerException) { }
    }
}
