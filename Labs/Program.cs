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
    internal class IsPulledByGravity : TimelineComponent {
        [RequireComponent]
        HasVelocity _transform = null;

        Vector2 _gravity = new Vector2(0, -9.81f);

        public override void Advance(TimeSpan delta) {
            float elapsed = (float)delta.TotalSeconds;

            Vector2 force = _gravity * elapsed;

            _transform.Velocity += force;
        }
    }

    internal class HasVelocity : TimelineComponent {
        [RequireComponent]
        Transformable2D _transform = null;

        Vector2 _velocity = Vector2.Zero;

        public Vector2 Velocity {
            get {
                return _velocity;
            }
            set {
                if (_velocity != value) {
                    _velocity = value;
                }
            }
        }

        public override void Advance(TimeSpan delta) {
            _transform.Position += _velocity;

            if (_transform.Position.Y < -200) {
                _velocity.Y = -_velocity.Y * 0.99f;
            }

            if (_transform.Position.X < -240 || _transform.Position.X > 240) {
                _velocity.X = -_velocity.X * 0.99f;
            }
        }
    }

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
                Size = new Size(64, 64),

                Texture = Texture.FromFile("frame.png")
            };

            Sprite backgroundSprite = new Sprite() {
                Layer = 0,
                Repeats = true,
                Size = new Size(
                    _context.Bounds.Width, 
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

            Random r = new Random();

            for (int i = 3; i < 1003; i++) {
                Sprite s = new Sprite() {
                    Layer = i,

                    Size = new Size(240 / 2, 48 / 2),

                    Texture = Texture.FromFile("fruitless-logo.png")
                };

                spriteBatch.Add(s);

                Entity.Create("" + i, 
                    s, 
                    new Spin() { 
                        Speed = 0.0075f * (float)r.NextDouble() 
                    }, 
                    new HasVelocity() {
                        Velocity = new Vector2(
                            (r.Next(0, 8) * (r.Next(0, 2) > 0 ? 1 : -1)) * (float)r.NextDouble(),
                            (r.Next(2, 14)) * (float)r.NextDouble())
                    },
                    new IsPulledByGravity());
            }
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
