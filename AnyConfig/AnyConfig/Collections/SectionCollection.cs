using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AnyConfig.Collections
{
    public class SectionCollection<T> : SectionCollection, IEnumerable<T>
        where T : IKeyable
    {
        private readonly List<T> _values;

        public new T this[int index]
        {
            get
            {
                return _values[index];
            }
            set
            {
                _values[index] = value;
            }
        }

        public new T this[string key]
        {
            get
            {
                return _values.FirstOrDefault(x => x.Key.Equals(key));
            }
            set
            {
                var existingValue = _values.FirstOrDefault(x => x.Key.Equals(key));
                existingValue?.Set(value);
            }
        }

        public new int Count => _values.Count;

        public SectionCollection()
        {
            _values = new List<T>();
        }
        
        public SectionCollection(List<T> values)
        {
            _values = values;
        }

        public SectionCollection(IReadOnlyList<T> values)
        {
            _values = new List<T>(values);
        }

        /// <summary>
        /// Gets the values associated with the specified key from the GenericNameValueCollection combined into one comma-separated list.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public new T Get(int index) => this[index];

        /// <summary>
        /// Gets the values associated with the specified key from the GenericNameValueCollection combined into one comma-separated list.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public void Add(T item) => _values.Add(item);

        public new IEnumerator<T> GetEnumerator() => _values.GetEnumerator();
    }

    public class SectionCollection : IEnumerable
    {
        private readonly List<IKeyable> _values;

        /// <summary>
        /// Section information
        /// </summary>
        public SectionInformation SectionInformation { get; set; }

        public object this[int index]
        {
            get
            {
                return _values[index];
            }
            set
            {
                _values[index].Set(value);
            }
        }

        public object this[string key]
        {
            get
            {
                return _values.FirstOrDefault(x => x.Key.Equals(key));
            }
            set
            {
                var existingValue = _values.FirstOrDefault(x => x.Key.Equals(key));
                existingValue?.Set(value);
            }
        }

        public int Count => _values.Count;

        public SectionCollection()
        {
            _values = new List<IKeyable>();
        }

        public SectionCollection(List<IKeyable> values)
        {
            _values = values;
        }

        public SectionCollection(IReadOnlyList<IKeyable> values)
        {
            _values = new List<IKeyable>(values);
        }

        /// <summary>
        /// Gets the values associated with the specified key from the GenericNameValueCollection combined into one comma-separated list.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object Get(int index) => this[index];

        /// <summary>
        /// Gets the values associated with the specified key from the GenericNameValueCollection combined into one comma-separated list.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public void Add(IKeyable item) => _values.Add(item);

        public IEnumerator<IKeyable> GetEnumerator() => _values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();
    }
}
