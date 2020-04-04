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
            var type = typeof(T);
            return (T)Convert.ChangeType(value, type);
        }
    }
}
