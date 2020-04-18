using AnyConfig.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AnyConfig.Collections
{
    /// <summary>
    /// Collection of connection strings
    /// </summary>
    public class ConnectionStringSettingsCollection
    {
        private readonly Dictionary<string, ConnectionStringSetting> _values = new Dictionary<string, ConnectionStringSetting>();

        /// <summary>
        /// Section information
        /// </summary>
        public SectionInformation SectionInformation { get; set; }

        public ConnectionStringSetting this[string key]
        {
            get
            {
                if (_values.ContainsKey(key))
                    return _values[key];
                return null;
            }
            set
            {
                if (_values.ContainsKey(key))
                    _values[key] = value;
            }
        }

        public ConnectionStringSettingsCollection(Dictionary<string, ConnectionStringSetting> values)
        {
            _values = values;

        }
        public ConnectionStringSettingsCollection(ReadOnlyDictionary<string, ConnectionStringSetting> values)
        {
            _values = new Dictionary<string, ConnectionStringSetting>(values);
        }
    }
}
