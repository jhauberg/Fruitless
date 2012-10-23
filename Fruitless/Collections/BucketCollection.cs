using System.Collections.Generic;
using System.Collections;

namespace Fruitless.Collections {
    /// <summary>
    /// Represents a collection of unique buckets, each able to contain values.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    internal class BucketCollection<TKey, TValue> : IBucketCollection<TKey, TValue> {
        Dictionary<TKey, ICollection<TValue>> _buckets = new Dictionary<TKey, ICollection<TValue>>();

        public void Add(TKey key, TValue value) {
            ICollection<TValue> bucket = null;

            if (_buckets.ContainsKey(key)) {
                bucket = _buckets[key];
            }

            if (bucket == null) {
                bucket = new List<TValue>();

                _buckets[key] = bucket;
            }

            if (bucket != null) {
                bucket.Add(value);
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item) {
            Add(item.Key, item.Value);
        }

        public bool Remove(TKey key) {
            if (_buckets.ContainsKey(key)) {
                return _buckets.Remove(key);
            }

            return false;
        }

        public bool Remove(TKey key, TValue value) {
            ICollection<TValue> bucket = null;

            if (_buckets.ContainsKey(key)) {
                bucket = _buckets[key];
            }

            if (bucket != null) {
                return bucket.Remove(value);
            }

            return false;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) {
            return Remove(item.Key, item.Value);
        }

        public ICollection<TValue> this[TKey key] {
            get {
                return _buckets[key];
            }
            set {
                _buckets[key] = value;
            }
        }

        public void Clear() {
            _buckets.Clear();
        }

        public int Count {
            get {
                return _buckets.Count;
            }
        }

        public bool IsReadOnly {
            get {
                return false;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _buckets.GetEnumerator();
        }

        IEnumerator<KeyValuePair<TKey, ICollection<TValue>>> IEnumerable<KeyValuePair<TKey, ICollection<TValue>>>.GetEnumerator() {
            return _buckets.GetEnumerator();
        }
    }
}
