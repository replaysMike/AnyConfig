using AnyConfig.Json;
using AnyConfig.Models;
using System;

namespace AnyConfig
{
    public static class ConfigurationManagerExtensions
    {
        /// <summary>
        /// Get value as a specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T As<T>(this object value)
        {
            if (value == null)
                return default;

            var type = typeof(T);
            var objectType = value.GetType();

            // handle any custom serialization requirements
            if (objectType == typeof(ConfigSectionPair))
            {
                var configSectionPair = value as ConfigSectionPair;
                if (configSectionPair.TypeValue == typeof(RequiresJsonSerialization))
                {
                    // serialize data first
                    value = JsonSerializer.Deserialize<T>(configSectionPair.Configuration.ToString());
                }
            }

            try
            {
                var typedValue = (T)value;
                return typedValue;
            }
            catch (Exception)
            {
                // unable to cast
            }

            // try converting the object
            return (T)Convert.ChangeType(value, type);
        }
    }
}
