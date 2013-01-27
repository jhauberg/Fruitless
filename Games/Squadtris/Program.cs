using ComponentKit;
using ComponentKit.Model;
using Fruitless;
using Fruitless.Components;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using System;
using System.Drawing;
using System.Reflection;

namespace Squadtris {
    internal class Program : GameWindow {
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

        IEntityRecord _world;
        IEntityRecord _dungeon;

        public Program()
            : base(320, 480, GraphicsMode.Default, "Squadtris") {
            Console.WriteLine("-------------------------------------------------------------------------------");
            Console.WriteLine(" E | NAME ");
            Console.WriteLine("------------------------------");

            WindowBorder = OpenTK.WindowBorder.Fixed;

            CursorVisible = true;

            VSync = VSyncMode.On;

            TargetRenderFrequency = 60;
            TargetUpdateFrequency = 60;

            Cursor.X = Mouse.X;
            Cursor.Y = Mouse.Y;
        }

        void CreateSharedEntities() {
            _world = Entity.Create(Entities.Shared.World, new Transformable2D());
            
            Entity.Create(Entities.Shared.Tasks, new TaskManager());
            Entity.Create(Entities.Shared.Sprites, new SpriteBatch());
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
                    Columns = 11,
                    Rows = 16
                };

            gridSettings.Layer = 0;

            Entity.Create(string.Format("{0}~floor", Entities.Game.Dungeon),
                new Transformable2D() {
                    Parent = _dungeon.GetComponent<Transformable2D>()
                },
                new SpriteGrid(gridSettings) {
                    Texture = Texture.FromFile("Content/Graphics/floor.png")
                }
            );

            string map =
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

            Entity.Create(string.Format("{0}~walls", Entities.Game.Dungeon),
                new Transformable2D() {
                    Parent = _dungeon.GetComponent<Transformable2D>()
                },
                new MappedSpriteGrid(gridSettings, map) {
                    Textures = new Texture[] { 
                        Texture.FromFile("Content/Graphics/wall.png"),       // #1
                        Texture.FromFile("Content/Graphics/wall-broken.png") // #2
                    }
                }
            );

            map =
                "00000000000" +
                "00000000000" +
                "00000000000" +
                "00000000000" +
                "00000000000" +
                "00000000000" +
                "00000000000" +
                "00000000000" +
                "00000000000" +
                "00000000000" +
                "00000000000" +
                "00000000000" +
                "00000000000" +
                "00001000000" +
                "00011100000" +
                "00000000000";

            Entity.Create(string.Format("{0}~squad", Entities.Game.Dungeon),
                new Transformable2D() {
                    Parent = _dungeon.GetComponent<Transformable2D>()
                },
                new MappedSpriteGrid(gridSettings, map) {
                    Texture = Texture.FromFile("Content/Graphics/unit.png")
                }
            );

            map =
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

            Entity.Create(string.Format("{0}~enemies", Entities.Game.Dungeon),
                new Transformable2D() {
                    Parent = _dungeon.GetComponent<Transformable2D>()
                },
                new MappedSpriteGrid(gridSettings, map) {
                    Texture = Texture.FromFile("Content/Graphics/enemy.png")
                }
            );
        }

        void CreateGameEntities() {
            SpriteBatch sprites = Entity.Find(Entities.Shared.Sprites)
                .GetComponent<SpriteBatch>();

            CreateTheDungeon(spriteBatch: sprites);
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

        void OnEntityEntered(object sender, EntityEventArgs e) {

        }

        void OnEntityRemoved(object sender, EntityEventArgs e) {

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
                System.Diagnostics.Debug.WriteLine("~");
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
