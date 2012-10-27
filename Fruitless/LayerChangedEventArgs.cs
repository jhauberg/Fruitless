using System;

namespace Fruitless {
    public class LayerChangedEventArgs : EventArgs {
        public LayerChangedEventArgs(int previous, int current) {
            Previous = previous;
            Current = current;
        }

        public int Previous {
            get;
            private set;
        }

        public int Current {
            get;
            private set;
        }
    }
}
