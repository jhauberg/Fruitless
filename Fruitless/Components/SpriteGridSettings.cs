

namespace Fruitless.Components {
    /// <summary>
    /// Transport common SpriteGrid parameters easily.
    /// </summary>
    public struct SpriteGridSettings {
        public int Columns {
            get;
            set;
        }

        public int Rows {
            get;
            set;
        }

        public int Layer {
            get;
            set;
        }

        public Texture Texture {
            get;
            set;
        }

        public SpriteBatch SpriteBatch {
            get;
            set;
        }
    }
}
