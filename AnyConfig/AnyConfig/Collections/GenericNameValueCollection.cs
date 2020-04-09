using System;
using System.Collections.Specialized;

namespace AnyConfig.Collections
{
    /// <summary>
    /// Represents a NameValueCollection that supports conversions
    /// </summary>
    public class GenericNameValueCollection : NameValueCollection
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
                return new StringValue(base[name]);
            }
            set
            {
                base[name] = value.Value;
            }
        }

        /// <summary>
        /// Gets the entry at the specified index of the GenericNameValueCollection.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public new StringValue this[int index]
        {
            get
            {
                return new StringValue(base[index]);
            }
        }

        /// <summary>
        /// Gets all the keys in the GenericNameValueCollection.
        /// </summary>
        public new StringValue[] AllKeys
        {
            get
            {
                return Array.ConvertAll(base.AllKeys, b => new StringValue(b));
            }
        }
        
        public GenericNameValueCollection()
        {
        }

        public GenericNameValueCollection(GenericNameValueCollection collection) : base(collection)
        {
        }

        /// <summary>
        /// Gets the values at the specified index of the GenericNameValueCollection combined into one comma-separated list.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public new StringValue Get(int index) => this[index];

        /// <summary>
        /// Gets the values associated with the specified key from the GenericNameValueCollection combined into one comma-separated list.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public new StringValue Get(string name) => this[name];

        /// <summary>
        /// Gets the values at the specified index of the GenericNameValueCollection.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public new StringValue[] GetValues(int index) => Array.ConvertAll(base.GetValues(index), b => new StringValue(b));

        /// <summary>
        /// Gets the values associated with the specified key from the GenericNameValueCollection.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public new StringValue[] GetValues(string name) => Array.ConvertAll(base.GetValues(name), b => new StringValue(b));
    }
}
