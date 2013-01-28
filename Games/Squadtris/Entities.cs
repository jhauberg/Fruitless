namespace Squadtris {
    internal sealed class Entities {
        public static class Shared {
            public const string World = "~";
            public const string Tasks = "tasks";
            public const string Sprites = "sprites";
        }

        public static class Game {
            public const string Dungeon = "dungeon";
            public const string DungeonFloor = Dungeon + "~floor";
            public const string DungeonWalls = Dungeon + "~walls";
            public const string DungeonEnemies = Dungeon + "~enemies";
            public const string Squad = "squad";
            public const string SquadUnits = Squad + "~units";
        }

        public static class Layers {
            public const int Back = 0;
            public const int Floor = 1;
            public const int Ceiling = 2;
        }
    }
}
