using System;
using System.Collections.Generic;
using System.Text;

namespace AnyConfig
{
    public struct ObjectCacheKey
    {
        public string Filename { get; set; }
        public string OptionName { get; set; }
        public Type Type { get; set; }


        public override int GetHashCode()
            => Type.GetHashCode() ^ Filename.GetHashCode() ^ OptionName.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj is null || !(obj is ObjectCacheKey))
                return false;
            var typedObj = (ObjectCacheKey)obj;
            return Filename.Equals(typedObj.Filename)
                && Type.Equals(typedObj.Type)
                && OptionName.Equals(typedObj.OptionName);
        }

        public static bool operator ==(ObjectCacheKey a, ObjectCacheKey b) => a.Equals(b);
        public static bool operator !=(ObjectCacheKey a, ObjectCacheKey b) => !(a == b);
    }
}
