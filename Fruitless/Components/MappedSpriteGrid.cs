namespace Fruitless.Components {
    /// <summary>
    /// Lays out a grid of sprites, but certain cells can be empty (i.e. where an entity does not need to be made).
    /// Uses the provided map to determine whether or not to create a new entity at a specific cell.
    /// </summary>
    public class MappedSpriteGrid : SpriteGrid {
        const int NotFound = -1;

        string Map {
            get;
            set;
        }

        /// <summary>
        /// When set, these textures are prioritized over base.Texture.
        /// </summary>
        public Texture[] Textures {
            get;
            set;
        }

        public MappedSpriteGrid(string map) {
            Map = map;
        }

        public MappedSpriteGrid(SpriteGridSettings settings, string map) :
            base(settings) {
            Map = map;
        }

        int GetTextureIndexAtCell(int column, int row) {
            int spriteIndex = GetSpriteIndex(column, (Rows - 1) - row);

            if (spriteIndex < Map.Length) {
                return int.Parse("" + Map[spriteIndex]) - 1;
            }

            return NotFound;
        }

        bool CanCreateSpriteAtCell(int column, int row, out int textureIndex) {
            textureIndex = NotFound;

            if (string.IsNullOrEmpty(Map)) {
                return false;
            }

            // note how the row coordinate is flipped - this is because cell 0,0 is drawn from lower-left corner, 
            // but in the Map string, it is actually on the last row.
            int spriteIndex = GetSpriteIndex(column, (Rows - 1) - row);

            if (spriteIndex < Map.Length) {
                textureIndex = int.Parse("" + Map[spriteIndex]) - 1;

                return textureIndex != NotFound;
            }

            return false;
        }

        protected override Sprite CreateSpriteAtCell(int column, int row) {
            int textureIndex = -1;

            if (CanCreateSpriteAtCell(column, row, out textureIndex)) {
                Sprite sprite = base.CreateSpriteAtCell(column, row);

                if (sprite.Texture == null || (Textures != null && textureIndex < Textures.Length)) {
                    sprite.Texture = Textures[textureIndex];
                }

                return sprite;
            }

            return null;
        }
    }
}
