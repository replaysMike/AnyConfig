using System.Collections.Generic;

namespace AnyConfig.Collections
{
    /// <summary>
    /// Represents a NameValueCollection that supports conversions
    /// </summary>
    public class GenericNameValueCollectionFast : Dictionary<string, StringValue>
    {
        /// <summary>
        /// Gets or sets the entry with the specified key in the GenericNameValueCollection.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public new StringValue this[string name]
        {
            get
            {
                if (ContainsKey(name))
                    return base[name];
                return null;
            }
            set
            {
                base[name] = value.Value;
            }
        }

        /// <summary>
        /// Section information
        /// </summary>
        public SectionInformation SectionInformation { get; set; }

        public GenericNameValueCollectionFast()
        {
        }

        public GenericNameValueCollectionFast(GenericNameValueCollectionFast collection) : base(collection)
        {
        }

        /// <summary>
        /// Gets the values associated with the specified key from the GenericNameValueCollection combined into one comma-separated list.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public StringValue Get(string name) => this[name];
    }
}
