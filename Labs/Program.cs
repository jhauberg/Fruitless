using System;
using Fruitless;
using ComponentKit.Model;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using System.Drawing;
using Labs.Components;
using Fruitless.Components;

namespace Labs {
    internal class Program : GameWindow {
        DefaultGameContext _context;

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

            _context = new DefaultGameContext(
                windowBoundsInPixels: ClientRectangle.Size);

            Sprite sprite = new Sprite() {
                Texture = Texture.FromFile("fruitless-logo.png")
            };

            Sprite backgroundSprite = new Sprite() {
                Repeats = true,
                Layer = -1,
                Bounds = new Size(_context.Bounds.Width, _context.Bounds.Height),

                Texture = Texture.FromFile("tile.png")
            };

            SpriteBatch spriteBatch = new SpriteBatch();
            {
                spriteBatch.Add(sprite);
                spriteBatch.Add(backgroundSprite);
            }

            Entity.Create("logo", sprite, new Bounce());
            Entity.Create("background", backgroundSprite);
            Entity.Create("batcher batcher batcher!", spriteBatch);
            
            SpriteBatch editorSpriteBatch = new SpriteBatch();
            {
                
            }

            Entity.Create("editor-spritebatch", editorSpriteBatch);
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
