using System;

namespace AnyConfig.Collections
{
    /// <summary>
    /// Represents a string value that can be converted
    /// </summary>
    public struct StringValue : IEquatable<string>
    {
        public string Value { get; set; }

        public StringValue(string value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(Value, null))
            {
                if (ReferenceEquals(obj, null))
                    return true;
                return false;
            }
            return Value.Equals(obj);
        }

        public bool Equals(string str)
        {
            if (ReferenceEquals(Value, null))
            {
                if (ReferenceEquals(str, null))
                    return true;
                return false;
            }
            return Value.Equals(str);
        }

        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value;

        public static bool operator ==(StringValue left, object right)
        {
            if (ReferenceEquals(left, null))
            {
                if (ReferenceEquals(right, null))
                    return true;
                return false;
            }
            return left.Equals(right);
        }

        public static bool operator !=(StringValue left, object right)
        {
            return !(left == right);
        }

        public static implicit operator string(StringValue s) => s.Value;
        public static implicit operator StringValue(string s) => new StringValue(s);
    }
}
