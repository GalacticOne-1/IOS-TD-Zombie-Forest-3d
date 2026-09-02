using System.Collections.Generic;

namespace Galactic1.Code.GameDatabase.Registries
{
    /// <summary>
    /// Generic immutable runtime registry.
    /// O(1) lookup.
    /// </summary>
    public abstract class RegistryBase<TKey, TValue>
    {
        protected readonly Dictionary<TKey, TValue> map;
        
        public IReadOnlyDictionary<TKey, TValue> All => map;
        public int Count => map.Count;
        

        protected RegistryBase()
        {
            map = new Dictionary<TKey, TValue>();
        }

        public TValue Get(TKey key)
        {
            return map[key];
        }

        public bool TryGet(TKey key, out TValue value)
        {
            if (key == null)
            {
                value = default;
                return false;
            }

            return map.TryGetValue(key, out value);
        }

        public bool Contains(TKey key)
        {
            return map.ContainsKey(key);
        }

    }
}