using ComponentKit;
using ComponentKit.Model;
using Fruitless;
using Fruitless.Components;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using System;
using System.Drawing;

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

        void CreateGameEntities() {
            SpriteBatch sprites = Entity.Find(Entities.Shared.Sprites)
                .GetComponent<SpriteBatch>();

            _dungeon = Entity.Create(Entities.Game.Dungeon);

            SpriteGridSettings gridSettings = 
                new SpriteGridSettings() {
                    SpriteBatch = sprites,
                    Columns = 11,
                    Rows = 16
                };

            gridSettings.Layer = 0;

            Entity.Create(string.Format("{0}~floor", Entities.Game.Dungeon),
                new Transformable2D() {
                    Position = new Vector2(
                        -(_context.Bounds.Width / 2),
                        -(_context.Bounds.Height / 2))
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
                "10001000001" +
                "10000000001" +
                "10000000011" +
                "10000011111" +
                "10000000011" +
                "10000000001" +
                "10000000001" +
                "11000000011";

            gridSettings.Layer = 1;
            
            // note how the grid components are not really required after the tiles have been laid out; entities could instead be marked with e.g. "wall",
            // although they are useful to keep around if needing to Layout() after the initial pass (maybe tiles get displaced as part of the game - because why not).
            Entity.Create(string.Format("{0}~walls", Entities.Game.Dungeon),
                new Transformable2D() {
                    Position = new Vector2(
                        -(_context.Bounds.Width / 2),
                        -(_context.Bounds.Height / 2))
                },
                new MappedSpriteGrid(gridSettings, map) {
                    Texture = Texture.FromFile("Content/Graphics/wall.png")
                }
            );
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            _context = new DefaultGameContext(
                windowBoundsInPixels: ClientRectangle.Size);

            _context.Registry.Entered += OnEntityEntered;
            _context.Registry.Removed += OnEntityRemoved;

            CreateSharedEntities();
            CreateGameEntities();
            /*
            Console.WriteLine();

            foreach (IEntityRecord entity in EntityRegistry.Current) {
                PrintEntity(entity, 0);
            }
            */
        }

        void PrintEntity(IEntityRecord entity, int level) {
            for (int i = 0; i < level; i++) {
                Console.Write(" ");
            }

            Console.Write(entity.ToString());
            Console.WriteLine();
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
