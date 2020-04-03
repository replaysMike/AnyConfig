using System;

namespace AnyConfig.Exceptions
{
    public class ConfigurationMissingException : Exception
    {
        public string PropertyName { get; }
        public Type PropertyType { get; }
        public ConfigurationMissingException(string errorMessage) : base(errorMessage) { }

        public ConfigurationMissingException(string propertyName, Type propertyType) : base($"Configuration is missing value for setting named '{propertyName}' of type '{propertyType.Name}'")
        {
            PropertyName = propertyName;
            PropertyType = propertyType;
        }
    }
}
