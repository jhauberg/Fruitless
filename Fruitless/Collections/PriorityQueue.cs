using System.Collections.Generic;
using System.Linq;

namespace Fruitless.Collections {
    internal class PriorityQueue<TP, TV> {
        SortedDictionary<TP, Queue<TV>> list = 
            new SortedDictionary<TP, Queue<TV>>();

        public void Enqueue(TP priority, TV value) {
            Queue<TV> q = null;

            if (!list.TryGetValue(priority, out q)) {
                q = new Queue<TV>();

                list.Add(priority, q);
            }

            q.Enqueue(value);
        }

        public TV Dequeue() {
            var pair = list.First();
            var v = pair.Value.Dequeue();

            if (pair.Value.Count == 0) {
                list.Remove(pair.Key);
            }

            return v;
        }

        public bool IsEmpty {
            get {
                return !list.Any();
            }
        }
    }
}
