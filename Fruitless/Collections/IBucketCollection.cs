using System.Collections.Generic;

namespace Fruitless.Collections {
    public interface IBucketCollection<TKey, TValue> : IEnumerable<KeyValuePair<TKey, ICollection<TValue>>> {
        void Add(TKey key, TValue value);
        bool Remove(TKey key);
        bool Remove(TKey key, TValue value);

        ICollection<TValue> this[TKey key] { get; set; }
    }
}
