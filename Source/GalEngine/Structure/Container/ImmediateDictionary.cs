using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    public class ImmediateDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TValue> mPool;

        public int Count => mPool.Count;

        public Dictionary<TKey, TValue>.ValueCollection Values => mPool.Values;
        public Dictionary<TKey, TValue>.KeyCollection Keys => mPool.Keys;

        public TValue this[TKey key]
        {
            get => mPool[key];
            set => mPool[key] = value;
        }
        
        public ImmediateDictionary()
        {
            mPool = new Dictionary<TKey, TValue>();
        }

        public virtual void Add(TKey key, TValue value) =>
            mPool.Add(key, value);

        public virtual void Remove(TKey key) =>
            mPool.Remove(key);

        public virtual void Clear() =>
            mPool.Clear();

        public virtual bool ContainsKey(TKey key) =>
            mPool.ContainsKey(key);

        public virtual bool ContainsValue(TValue value) =>
            mPool.ContainsValue(value);

        public virtual Dictionary<TKey, TValue>.Enumerator GetEnumerator() =>
            mPool.GetEnumerator();
    }
}
