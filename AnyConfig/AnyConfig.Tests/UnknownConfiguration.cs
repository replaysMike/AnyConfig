using System;

namespace AnyConfig.Tests
{
    public class UnknownConfiguration : IEquatable<UnknownConfiguration>
    {
        public string Name { get; set; }
        public UnknownConfiguration(string name)
        {
            Name = name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;
            var typedObj = obj as UnknownConfiguration;
            return typedObj.Name == Name;
        }

        public bool Equals(UnknownConfiguration other)
        {
            if (other is null)
                return false;
            return other.Name == Name;
        }
    }
}
