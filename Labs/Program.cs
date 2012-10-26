using System;
using System.Linq;
using Fruitless;
using ComponentKit.Model;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using System.Drawing;
using Labs.Components;
using Fruitless.Components;
using ComponentKit;

namespace Labs {
    internal class Program : GameWindow {
        EditableGameContext _context;

        public Program()
            : base(480, 400, GraphicsMode.Default, "FRUITLESS LIVING") {
            WindowBorder = OpenTK.WindowBorder.Fixed;

            CursorVisible = true;

            VSync = VSyncMode.On;
            
            TargetRenderFrequency = 60;
            TargetUpdateFrequency = 60;
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            _context = new EditableGameContext(
                windowBoundsInPixels: ClientRectangle.Size);

            _context.Registry.Entered += OnEntityEntered;
            _context.Registry.Removed += OnEntityRemoved;
            
            Sprite sprite = new Sprite() {
                Layer = 1,

                Texture = Texture.FromFile("fruitless-logo.png")
            };

            Sprite frameSprite = new Sprite() {
                Layer = 2,
                Bounds = new Size(64, 64),

                Texture = Texture.FromFile("frame.png")
            };

            Sprite backgroundSprite = new Sprite() {
                Layer = 0,
                Repeats = true,
                Bounds = new Size(
                    (int)(_context.Bounds.Width * 0.75f), 
                    _context.Bounds.Height),

                Texture = Texture.FromFile("tile.png")
            };

            SpriteBatch spriteBatch = new SpriteBatch();
            {
                spriteBatch.Add(backgroundSprite);
                spriteBatch.Add(sprite);
                spriteBatch.Add(frameSprite);
            }

            Entity.Create("logo", sprite, new Bounce());
            Entity.Create("background", backgroundSprite);
            Entity.Create("batcher batcher batcher!", spriteBatch);
        }

        void OnEntityEntered(object sender, EntityEventArgs e) {

        }

        void OnEntityRemoved(object sender, EntityEventArgs e) {

        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);

            _context.Bounds = ClientRectangle.Size;
        }

        protected override void OnUpdateFrame(FrameEventArgs e) {
            base.OnUpdateFrame(e);

            if (Keyboard[Key.Escape]) {
                Exit();
            }

            _context.Refresh(e.Time);

            if (_context.IsOutOfSync) {
                _context.Synchronize();
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e) {
            base.OnRenderFrame(e);

            _context.Render();

            SwapBuffers();
        }

        [STAThread]
        internal static void Main() {
            using (Program game = new Program()) {
                game.Icon = new Icon("icon.ico");
                game.Run(60.0);
            }
        }
    }
}
