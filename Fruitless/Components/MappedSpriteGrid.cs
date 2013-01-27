namespace Fruitless.Components {
    /// <summary>
    /// Lays out a grid of sprites, but certain cells can be empty (i.e. where an entity does not need to be made).
    /// Uses the provided map to determine whether or not to create a new entity at a specific cell.
    /// </summary>
    public class MappedSpriteGrid : SpriteGrid {
        string Map {
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

        bool CanCreateSpriteAtCell(int column, int row) {
            // note how the row coordinate is flipped - this is because cell 0,0 is drawn from lower-left corner, 
            // but in the Map string, it is actually on the last row.
            int spriteIndex = GetSpriteIndex(column, (Rows - 1) - row);

            if (spriteIndex < Map.Length) {
                return !Map[spriteIndex]
                    .Equals('0');
            }

            return false;
        }

        protected override Sprite CreateSpriteAtCell(int column, int row) {
            if (CanCreateSpriteAtCell(column, row)) {
                return base.CreateSpriteAtCell(column, row);
            }

            return null;
        }
    }
}
