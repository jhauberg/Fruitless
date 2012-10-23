using System;

namespace Fruitless {
    public class TextureChangedEventArgs : EventArgs {
        public TextureChangedEventArgs(Texture old, Texture current) {
            Previous = old;
            Current = current;
        }

        public Texture Previous {
            get;
            private set;
        }

        public Texture Current {
            get;
            private set;
        }
    }
}
