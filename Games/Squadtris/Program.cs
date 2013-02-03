using ComponentKit;
using ComponentKit.Model;
using Fruitless;
using Fruitless.Components;
using OpenTK;
using OpenTK.Input;
using Squadtris.Components;
using System;
using System.Drawing;

namespace Squadtris {
    internal class Program : DebuggableGameWindow {
        internal static class Cursor {
            public static int X {
                get;
                set;
            }

            public static int Y {
                get;
                set;
            }
        }

        DefaultGameContext _context;

        KeyboardState _ks;
        KeyboardState _ksLast;

        double _previousRenderTime;
        double _averageRenderTime;

        double _previousUpdateTime;
        double _averageUpdateTime;

        public Program()
            : base(320, 480, "Squadtris") {
            WindowBorder = OpenTK.WindowBorder.Fixed;
            
            CursorVisible = true;

            VSync = VSyncMode.On;

            TargetRenderFrequency = 60;
            TargetUpdateFrequency = 60;

            Cursor.X = Mouse.X;
            Cursor.Y = Mouse.Y;
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            _context = new DefaultGameContext(
                windowBoundsInPixels: ClientRectangle.Size);

            _context.Registry.Entered += OnEntityEntered;
            _context.Registry.Removed += OnEntityRemoved;

            CreateSharedEntities();
            CreateGameEntities();
        }

        IEntityRecord _world;
        IEntityRecord _dungeon;
        IEntityRecord _squadLeader;
        
        const int DungeonColumns = 11;
        const int DungeonRows = 16;

        void CreateSharedEntities() {
            _world = Entity.Create(Entities.Shared.World, new Transformable2D());

            Entity.Create(Entities.Shared.Tasks, new TaskManager());
            Entity.Create(Entities.Shared.Sprites, new SpriteBatch());
        }

        void CreateGameEntities() {
            SpriteBatch sprites = Entity.Find(Entities.Shared.Sprites)
                .GetComponent<SpriteBatch>();

            CreateTheDungeon(spriteBatch: sprites);
            CreateSquad(spriteBatch: sprites);
        }

        void CreateTheDungeon(SpriteBatch spriteBatch) {
            _dungeon = Entity.Create(Entities.Game.Dungeon,
                new Transformable2D() {
                    Position = new Vector2(
                        -(_context.Bounds.Width / 2),
                        -(_context.Bounds.Height / 2))
                });

            SpriteGridSettings gridSettings =
                new SpriteGridSettings() {
                    SpriteBatch = spriteBatch,
                    Columns = DungeonColumns,
                    Rows = DungeonRows
                };

            gridSettings.Layer = 0;

            Entity.Create(Entities.Game.DungeonFloor,
                new Transformable2D() {
                    Parent = _dungeon.GetComponent<Transformable2D>()
                },
                new SpriteGrid(gridSettings) {
                    Texture = Texture.FromFile("Content/Graphics/floor.png")
                }
            );

            string dungeonWallsMap =
                "11111011111" +
                "10000000001" +
                "10000000001" +
                "10000000001" +
                "10001000001" +
                "10001000001" +
                "10111110001" +
                "10001100001" +
                "10002000001" +
                "10000000001" +
                "10000000011" +
                "10000021111" +
                "10000000011" +
                "10000000001" +
                "10000000021" +
                "11000000011";

            gridSettings.Layer = 1;

            Entity.Create(Entities.Game.DungeonWalls,
                new Transformable2D() {
                    Parent = _dungeon.GetComponent<Transformable2D>()
                },
                new MappedSpriteGrid(gridSettings, dungeonWallsMap) {
                    Textures = new Texture[] { 
                        Texture.FromFile("Content/Graphics/wall.png"),       // #1
                        Texture.FromFile("Content/Graphics/wall-broken.png") // #2
                    }
                }
            );

            string dungeonEnemiesMap =
                "00000000000" +
                "00001111000" +
                "00000100000" +
                "00000000000" +
                "00010000000" +
                "00000010000" +
                "01000000000" +
                "01100010000" +
                "00000100000" +
                "00011100000" +
                "00000000000" +
                "00000000000" +
                "00000001100" +
                "00000000000" +
                "00000000000" +
                "00000000000";

            Entity.Create(Entities.Game.DungeonEnemies,
                new Transformable2D() {
                    Parent = _dungeon.GetComponent<Transformable2D>()
                },
                new MappedSpriteGrid(gridSettings, dungeonEnemiesMap) {
                    Texture = Texture.FromFile("Content/Graphics/enemy.png")
                }
            );
        }

        IEntityRecord CreateSquadUnit(string name, IEntityRecord parent, Vector2 formationPosition) {
            IEntityRecord squadUnit = Entity.CreateFromDefinition("squad-unit", String.Format("{0}~{1}", Entities.Game.Squad, name),
                new Transformable2D() {
                    Parent = parent.GetComponent<Transformable2D>(),
                    Position = formationPosition
                },
                new Sprite() {
                    Layer = 2,
                    Texture = Texture.FromFile("Content/Graphics/unit.png")
                });

            return squadUnit;
        }

        void CreateSquad(SpriteBatch spriteBatch) {
            Entity.Define("squad-unit",
                typeof(Health));

            Texture unitTexture = Texture.FromFile("Content/Graphics/unit.png");

            _squadLeader = CreateSquadUnit("leader", _dungeon, new Vector2(unitTexture.Width * (DungeonColumns / 2), 0));
            _squadLeader.Add(
                new SquadLeader() {
                    MovementInPixels = unitTexture.Width
                });

            IEntityRecord squadUnitLeft = CreateSquadUnit("left", _squadLeader, new Vector2(-unitTexture.Width, -unitTexture.Width));
            IEntityRecord squadUnitRight = CreateSquadUnit("right", _squadLeader, new Vector2(unitTexture.Width, -unitTexture.Width));
            IEntityRecord squadUnitMiddle = CreateSquadUnit("middle", _squadLeader, new Vector2(0, -unitTexture.Width));
            
            spriteBatch.Add(_squadLeader.GetComponent<Sprite>());
            spriteBatch.Add(squadUnitLeft.GetComponent<Sprite>());
            spriteBatch.Add(squadUnitRight.GetComponent<Sprite>());
            spriteBatch.Add(squadUnitMiddle.GetComponent<Sprite>());
        }

        void OnEntityEntered(object sender, EntityEventArgs e) {
            WriteInfo(String.Format("[+] {0}", e.Record));
        }

        void OnEntityRemoved(object sender, EntityEventArgs e) {
            WriteInfo(String.Format("[-] {0}", e.Record));
        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);

            _context.Bounds = ClientRectangle.Size;
        }

        bool KeyWasReleased(Key key) {
            return _ksLast[key] && !_ks[key];
        }

        protected override void OnUpdateFrame(FrameEventArgs e) {
            base.OnUpdateFrame(e);

            Cursor.X = Mouse.X;
            Cursor.Y = Mouse.Y;
            
            _ks = OpenTK.Input.Keyboard.GetState();

            if (KeyWasReleased(Key.Escape)) {
                Exit();
            }

            if (KeyWasReleased(Key.Tilde)) {
                ToggleConsole();
            }
  
            _context.Refresh(e.Time);

            if (_context.IsOutOfSync) {
                _context.Synchronize();
            }

            if (_previousUpdateTime > 0) {
                double weightRatio = 0.1;

                _averageUpdateTime = _averageUpdateTime * (1.0 - weightRatio) + _previousUpdateTime * weightRatio;
            }

            _previousUpdateTime = UpdateTime;

            _ksLast = _ks;
        }

        protected override void OnRenderFrame(FrameEventArgs e) {
            base.OnRenderFrame(e);

            _context.Render();

            SwapBuffers();

            if (_previousRenderTime > 0) {
                double weightRatio = 0.1;

                _averageRenderTime = _averageRenderTime * (1.0 - weightRatio) + _previousRenderTime * weightRatio;
            }

            _previousRenderTime = RenderTime;
        }

        [STAThread]
        internal static void Main() {
            using (Program game = new Program()) {
                game.Icon = new Icon("icon.ico");
                game.Run(game.TargetUpdateFrequency);
            }
        }
    }
}
