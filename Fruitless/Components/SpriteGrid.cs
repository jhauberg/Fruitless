using ComponentKit.Model;
using OpenTK;

namespace Fruitless.Components {
    /// <summary>
    /// Lays out a grid of sprites. Does not attempt to do anything with the sprites once they have been created.
    /// 
    /// Note: Currently, if removed, it also removes all sprites it made. This might change.
    /// </summary>
    public class SpriteGrid : GameComponent {
        [RequireComponent]
        Transformable2D _transform = null;

        [ComponentOutlet]
        public SpriteBatch SpriteBatch {
            get;
            set;
        }

        public SpriteGrid() {

        }

        public SpriteGrid(SpriteGridSettings settings) {
            Columns = settings.Columns;
            Rows = settings.Rows;
            Texture = settings.Texture;
            SpriteBatch = settings.SpriteBatch;
            Layer = settings.Layer;
        }

        public int Columns {
            get;
            set;
        }

        public int Rows {
            get;
            set;
        }

        int Layer {
            get;
            set;
        }

        public Texture Texture {
            get;
            set;
        }

        Sprite[] _sprites;

        protected override void OnRemoved(ComponentStateEventArgs registrationArgs) {
            base.OnRemoved(registrationArgs);

            for (int i = 0; i < _sprites.Length; i++) {
                Sprite sprite = _sprites[i];

                if (sprite == null) {
                    continue;
                }

                if (sprite.Record != null) {
                    sprite.Record.Drop();
                }

                if (SpriteBatch != null) {
                    SpriteBatch.Remove(sprite);
                }

                _sprites[i] = null;
            }
        }

        protected int GetSpriteIndex(int column, int row) {
            return row * Columns + column;
        }

        string GetSpriteEntityName(int column, int row) {
            return string.Format("{0}~sprite#{1},{2}",
                Record.Name, column, row);
        }

        void Clear() {
            if (_sprites != null) {
                foreach (Sprite sprite in _sprites) {
                    sprite.Remove();

                    if (SpriteBatch != null) {
                        SpriteBatch.Remove(sprite);
                    }
                }
            }
        }

        protected virtual Sprite CreateSpriteAtCell(int column, int row) {
            Sprite sprite = new Sprite();

            Entity.Create(GetSpriteEntityName(column, row),
                sprite);

            sprite.Transform.Parent = _transform;
            sprite.Texture = Texture;
            sprite.Layer = Layer;

            return sprite;
        }

        protected virtual Vector2 GetPositionOfCell(int column, int row) {
            return new Vector2(
                column * Texture.Width,
                row * Texture.Height);
        }

        void Build() {
            _sprites = new Sprite[Columns * Rows];

            for (int row = 0; row < Rows; row++) {
                for (int column = 0; column < Columns; column++) {
                    Sprite sprite = CreateSpriteAtCell(column, row);

                    if (sprite == null) {
                        continue;
                    }

                    _sprites[GetSpriteIndex(column, row)] = sprite;

                    if (SpriteBatch != null) {
                        SpriteBatch.Add(sprite);
                    }
                }
            }
        }

        public override void Reset() {
            Clear();
            Build();
            Layout();
        }

        public void Layout() {
            if (_sprites == null ||
                _sprites.Length != Columns * Rows) {
                Reset();

                return;
            }

            for (int row = 0; row < Rows; row++) {
                for (int column = 0; column < Columns; column++) {
                    Sprite sprite = _sprites[GetSpriteIndex(column, row)];

                    if (sprite == null ||
                        sprite.Transform == null ||
                        sprite.Texture == null) {
                        continue;
                    }

                    sprite.Transform.Position = GetPositionOfCell(column, row);
                }
            }
        }

        public Sprite this[int column, int row] {
            get {
                if (_sprites == null ||
                    column < 0 || row < 0 ||
                    column > Columns || row > Rows) {
                    return null;
                }

                return _sprites[GetSpriteIndex(column, row)];
            }
        }
    }
}
