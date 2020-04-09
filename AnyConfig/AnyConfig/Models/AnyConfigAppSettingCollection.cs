using AnyConfig.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AnyConfig.Models
{
    /// <summary>
    /// Stores AnyConfig grouped appsettings
    /// This class can be implicitly used as a StringValue to enable both [key]=>value and [group][key]=>value lookups
    /// </summary>
    public class AnyConfigAppSettingCollection : IEquatable<StringValue>, IEquatable<string>
    {
        private readonly ReadOnlyCollection<AnyConfigAppSettingPair> _values;

        public StringValue this[string key]
        {
            get
            {
                return _values.Where(x => x.Key.Equals(key))
                    .Select(x => x.Value)
                    .FirstOrDefault();
            }
        }

        public AnyConfigAppSettingCollection(List<AnyConfigAppSettingPair> values)
        {
            _values = values.AsReadOnly();

        }
        public AnyConfigAppSettingCollection(ReadOnlyCollection<AnyConfigAppSettingPair> values)
        {
            _values = new ReadOnlyCollection<AnyConfigAppSettingPair>(values);
        }

        public override int GetHashCode()
        {
            return _values.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (_values.Count == 0)
                return false;
            var firstValue = _values.FirstOrDefault().Value;
            if (ReferenceEquals(firstValue, null))
            {
                if (ReferenceEquals(obj, null))
                    return true;
                return false;
            }
            return firstValue.Equals(obj);
        }

        public bool Equals(StringValue other)
        {
            if (_values.Count == 0)
                return false;
            var firstValue = _values.FirstOrDefault().Value;
            if (ReferenceEquals(firstValue, null))
            {
                if (ReferenceEquals(other, null))
                    return true;
                return false;
            }
            return firstValue.Equals(other);
        }

        public bool Equals(string other)
        {
            if (_values.Count == 0)
                return false;
            var firstValue = _values.FirstOrDefault().Value;
            if (ReferenceEquals(firstValue, null))
            {
                if (ReferenceEquals(other, null))
                    return true;
                return false;
            }
            var isEqual = firstValue.Equals(other);
            return isEqual;
        }

        public static bool operator ==(AnyConfigAppSettingCollection left, StringValue right)
        {
            if (ReferenceEquals(left, null))
            {
                if (ReferenceEquals(right, null))
                    return true;
                return false;
            }
            return left.Equals(right);
        }

        public static bool operator !=(AnyConfigAppSettingCollection left, StringValue right)
        {
            return !(left == right);
        }

        public static bool operator ==(AnyConfigAppSettingCollection left, string right)
        {
            if (ReferenceEquals(left, null))
            {
                if (ReferenceEquals(right, null))
                    return true;
                return false;
            }
            return left.Equals(right);
        }

        public static bool operator !=(AnyConfigAppSettingCollection left, string right)
        {
            return !(left == right);
        }

        public static implicit operator StringValue(AnyConfigAppSettingCollection s) 
            => s._values.FirstOrDefault().Value;
        public static implicit operator AnyConfigAppSettingCollection(StringValue s) 
            => new AnyConfigAppSettingCollection(new List<AnyConfigAppSettingPair> { new AnyConfigAppSettingPair { Key = string.Empty, Value = s } });
        public static implicit operator string(AnyConfigAppSettingCollection s)
            => s._values.FirstOrDefault().Value;
        public static implicit operator AnyConfigAppSettingCollection(string s)
            => new AnyConfigAppSettingCollection(new List<AnyConfigAppSettingPair> { new AnyConfigAppSettingPair { Key = string.Empty, Value = s } });
    }
}
