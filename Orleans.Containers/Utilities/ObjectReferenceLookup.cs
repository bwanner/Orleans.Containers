using System.Runtime.CompilerServices;

namespace Orleans.Collections.Utilities
{
    public class ObjectReferenceLookup<TKey, TValue> where TValue : class where TKey : class
    {
        private readonly ConditionalWeakTable<TKey, TValue> _table;

        public ObjectReferenceLookup()
        {
            _table = new ConditionalWeakTable<TKey, TValue>();
        }

        public bool ContainsKey(TKey key)
        {
            TValue outValue;
            return _table.TryGetValue(key, out outValue);
        }

        public TValue GetValue(TKey key, TValue defaultNotFoundValue = default(TValue))
        {
            TValue outValue = null;
            _table.TryGetValue(key, out outValue);

            if(outValue != null)
                return outValue;
            
            return defaultNotFoundValue;
        }

        public void Add(TKey key, TValue value)
        {
            _table.Add(key, value);
        }

    }
}