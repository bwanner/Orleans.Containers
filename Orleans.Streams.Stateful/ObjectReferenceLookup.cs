using System.Runtime.CompilerServices;

namespace Orleans.Streams.Stateful
{
    /// <summary>
    /// Attaches values to an object reference.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class ObjectReferenceLookup<TKey, TValue> where TValue : class where TKey : class
    {
        private readonly ConditionalWeakTable<TKey, TValue> _table;

        /// <summary>
        /// Construct a new ObjectReferenceLookup.
        /// </summary>
        public ObjectReferenceLookup()
        {
            _table = new ConditionalWeakTable<TKey, TValue>();
        }

        /// <summary>
        /// Check if lookup contains a value for a key.
        /// </summary>
        /// <param name="key">Key to lookup.</param>
        /// <returns></returns>
        public bool ContainsKey(TKey key)
        {
            TValue outValue;
            return _table.TryGetValue(key, out outValue);
        }

        /// <summary>
        /// Get value for a given key or a default value.
        /// </summary>
        /// <param name="key">Key to use.</param>
        /// <param name="defaultNotFoundValue">Default value if key does not exist.</param>
        /// <returns>Value attached to key or default value.</returns>
        public TValue GetValue(TKey key, TValue defaultNotFoundValue = default(TValue))
        {
            TValue outValue = null;
            _table.TryGetValue(key, out outValue);

            if (outValue != null)
                return outValue;

            return defaultNotFoundValue;
        }

        /// <summary>
        /// Attach a value to a new key.
        /// </summary>
        /// <param name="key">Key to add.</param>
        /// <param name="value">Value to attach.</param>
        public void Add(TKey key, TValue value)
        {
            _table.Add(key, value);
        }

        public void OverrideValue(TKey key, TValue value)
        {
            _table.Remove(key);
            Add(key, value);
        }
    }
}