using System.Collections.Generic;

namespace AnyConfig.Models
{
    public class ConnectionStringSettingsCollection
    {
        private Dictionary<string, ConnectionStringSetting> _values = new Dictionary<string, ConnectionStringSetting>();

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
    }
}
