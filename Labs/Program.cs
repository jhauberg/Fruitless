using ComponentKit;
using ComponentKit.Model;
using Fruitless;
using Fruitless.Components;
using Labs.Components;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using System;
using System.Drawing;
using MathHelper = OpenTK.MathHelper;

namespace Labs {
    internal class IsPulledByGravity : TimelineComponent {
        [RequireComponent]
        HasVelocity _transform = null;
        
        public IsPulledByGravity() {
            GravityDirection = new Vector2(0, -9.81f);
        }

        public Vector2 GravityDirection {
            get;
            set;
        }

        public override void Advance(TimeSpan delta) {
            _transform.Velocity += GravityDirection;
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
            _transform.Position += _velocity * (float)delta.TotalSeconds;
        }
    }

    internal class DirectionEvenArgs : EventArgs {
        public DirectionEvenArgs(Vector2 direction) {
            Direction = direction;
        }

        public Vector2 Direction {
            get;
            private set;
        }
    }

    internal class AreaIntersectionEventArgs : DirectionEvenArgs {
        public AreaIntersectionEventArgs(Vector2 direction, AreaIntersectionSide side) 
            : base(direction) {
            Side = side;
        }
        public AreaIntersectionSide Side {
            get;
            private set;
        }
    }

    [Flags]
    public enum AreaIntersectionSide {
        None = 0,
        Top = 1,
        Right = 2,
        Bottom = 4,
        Left = 8
    }

    internal class Area : DependencyComponent {
        [RequireComponent]
        Transformable2D _transform = null;

        public Area() {
            Boundaries = Size.Empty;
        }

        public Transformable2D Transform {
            get {
                return _transform;
            }
        }

        public Size Boundaries {
            get;
            set;
        }
        
        public bool Contains(Vector2 position) {
            RectangleF area = new RectangleF(
                _transform.Position.X - Boundaries.Width / 2,
                _transform.Position.Y - Boundaries.Height / 2,
                Boundaries.Width,
                Boundaries.Height);

            return area.Contains(new PointF(position.X, position.Y));
        }
    }

    internal class AreaIntersectionTrigger : TimelineComponent {
        public event EventHandler<AreaIntersectionEventArgs> Entered;
        public event EventHandler<AreaIntersectionEventArgs> Left;

        Area _area;

        [ComponentOutlet]
        public Area Area {
            get {
                return _area;
            }
            set {
                if (_area != value) {
                    _area = value;
                }
            }
        }

        [RequireComponent]
        Transformable2D _transform = null;

        void OnEntered(Vector2 direction, AreaIntersectionSide side) {
            if (Entered != null) {
                Entered(this, new AreaIntersectionEventArgs(
                    direction, side));
            }
        }

        void OnLeft(Vector2 direction, AreaIntersectionSide side) {
            if (Left != null) {
                Left(this, new AreaIntersectionEventArgs(
                    direction, side));
            }
        }

        Vector2 _previousPosition = Vector2.Zero;

        bool _isCurrentlyContainedInArea;
        
        AreaIntersectionSide GetIntersectionSide(Vector2 position) {
            AreaIntersectionSide side = AreaIntersectionSide.None;

            if (_area != null) {
                float left = _area.Transform.Position.X - _area.Boundaries.Width / 2;
                float bottom = _area.Transform.Position.Y - _area.Boundaries.Height / 2;
                float right = left + _area.Boundaries.Width;
                float top = bottom + _area.Boundaries.Height;

                if (position.X < left) {
                    side = AreaIntersectionSide.Left;
                } else if (position.X > right) {
                    side = AreaIntersectionSide.Right;
                }

                if (position.Y < bottom) {
                    side |= AreaIntersectionSide.Bottom;
                } else if (position.Y > top) {
                    side |= AreaIntersectionSide.Top;
                }    
            }

            return side;
        }

        public override void Advance(TimeSpan delta) {
            base.Advance(delta);

            if (_area == null) {
                return;
            }

            Vector2 position = _transform.Position;
            Vector2 direction = Vector2.Normalize(_previousPosition - position);

            bool wasContainedInArea = _isCurrentlyContainedInArea;

            _isCurrentlyContainedInArea = _area.Contains(position);
            
            if (!wasContainedInArea && _isCurrentlyContainedInArea) {
                OnEntered(direction, GetIntersectionSide(position));
            } else if (wasContainedInArea && !_isCurrentlyContainedInArea) {
                OnLeft(direction, GetIntersectionSide(position));
            }
   
            _previousPosition = position;
        }
    }

    internal class Bounces : TimelineComponent {
        [RequireComponent]
        AreaIntersectionTrigger _walls = null;
 /*
        [RequireComponent]
        Transformable2D _transform = null;*/
        [RequireComponent]
        HasVelocity _velocity = null;

        public Bounces() {
            Friction = 0.98f;
        }

        protected override void OnAdded(ComponentStateEventArgs registrationArgs) {
            base.OnAdded(registrationArgs);

            _walls.Left += OnLeftBoundaries;
        }

        void OnLeftBoundaries(object sender, AreaIntersectionEventArgs e) {
            Vector2 velocity = _velocity.Velocity;

            if ((e.Side & AreaIntersectionSide.Bottom) == AreaIntersectionSide.Bottom) {
                velocity.Y = -velocity.Y * Friction;
            } 
            
            if ((e.Side & AreaIntersectionSide.Left) == AreaIntersectionSide.Left ||
                (e.Side & AreaIntersectionSide.Right) == AreaIntersectionSide.Right) {
                velocity.X = -velocity.X * Friction;
            }

            _velocity.Velocity = velocity;
        }
        
        /// <summary>
        /// Between 0-1. 0 will stop all movement when hitting boundaries, and 1 will make it lose no speed at all on collision. A high value between 0.92-0.98 usually works well.
        /// </summary>
        public float Friction {
            get;
            set;
        }
    }

    internal class Jumps : TimelineComponent {
        Vector2 _up = new Vector2(0, 1);
        Vector2 _down = new Vector2(0, -1);

        [RequireComponent]
        HasVelocity _velocity = null;

        private DateTime _timeStartedJumping;

        public Jumps() {
            Strength = 2000;
            JumpDuration = TimeSpan.FromSeconds(1);
        }

        public TimeSpan JumpDuration {
            get;
            set;
        }

        public Vector2 Direction {
            get {
                return _up;
            }
            set {
                if (_up != value) {
                    _up = value;
                    _down = _up * -1;
                }
            }
        }

        public float Strength {
            get;
            set;
        }

        KeyboardState _ksLast;

        public override void Advance(TimeSpan delta) {
            base.Advance(delta);

            float elapsed = (float)delta.TotalSeconds;

            KeyboardState ks = Keyboard.GetState();
            
            if (ks.IsKeyDown(Key.W)) {
                if (_ksLast.IsKeyUp(Key.W)) {
                    _timeStartedJumping = DateTime.Now;
                }

                TimeSpan timeSinceStartedJumping = DateTime.Now - _timeStartedJumping;

                if (timeSinceStartedJumping < JumpDuration) {
                    _velocity.Velocity += (_up * Strength) * elapsed;
                }
            } else if (ks.IsKeyDown(Key.S)) {
                _velocity.Velocity += (_down * Strength) * elapsed;
            }

            _ksLast = ks;
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
            
            Sprite logoCactusSprite = new Sprite() {
                Layer = 1,
                Size = new Size(48, 48),
                TextureSourceRectangle = new Rectangle(Point.Empty, new Size(48, 48)),

                Texture = Texture.FromFile("fruitless-logo.png")
            };

            Sprite logoNameSprite = new Sprite() {
                Layer = 1,
                Size = new Size(184, 48),
                TextureSourceRectangle = new Rectangle(new Point(56, 0), new Size(184, 48)),

                Texture = Texture.FromFile("fruitless-logo.png")
            };

            Sprite backgroundSprite = new Sprite() {
                Repeats = true,
                Size = new Size(
                    _context.Bounds.Width, 
                    _context.Bounds.Height),

                Texture = Texture.FromFile("tile.png")
            };

            Sprite rectangleSprite = new Sprite()
            {
                TintColor = Color4.Red,
                Size = new Size(16, 16),
                Texture = Texture.FromFile("tile.png")
            };

            SpriteBatch spriteBatch = new SpriteBatch();
            {
                spriteBatch.Add(backgroundSprite);
                spriteBatch.Add(logoCactusSprite);
                spriteBatch.Add(logoNameSprite);

                // something is very wrong here :(((
                //spriteBatch.Add(rectangleSprite);
            }

            Entity.Create("background", backgroundSprite);
            Entity.Create("logo", new Bounce() { Speed = 0.005f });
            Entity.Create("logo-cactus", logoCactusSprite, new Jumps() { JumpDuration = TimeSpan.FromSeconds(0.5) }, new IsPulledByGravity());
            Entity.Create("logo-name", logoNameSprite);
            Entity.Create(rectangleSprite);
            Entity.Create("batch", spriteBatch);
            
            logoCactusSprite.Transform.Parent = Entity.Find("logo").GetComponent<Transformable2D>();
            logoCactusSprite.Transform.Position = new Vector2(-100, 0);
            logoNameSprite.Transform.Parent = Entity.Find("logo").GetComponent<Transformable2D>();
            logoNameSprite.Transform.Position = new Vector2(24, 0);
            logoNameSprite.Transform.Scale = new Vector2(0.8f, 0.8f);
            logoNameSprite.Transform.Rotation = MathHelper.DegreesToRadians(10);

            rectangleSprite.Transform.Position = new Vector2(-100, 0);
      /*
            SpriteFontDescription font = new SpriteFontDescription() {
                Characters = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}“ ",
                CharacterSize = new Size(8, 8),
                TextureSourceRectangle = new Rectangle(new Point(0, 16), new Size(128, 176)),

                Texture = Texture.FromFile("ass_font_tran.png")
            };

            Text text = new Text() {
                FontDescription = font,
                Content = "hold nu kaeft mand :D"
            };

            TextBatch textBatch = new TextBatch() {
                Layer = 1
            };

            textBatch.AddText(text);

            Entity.Create("text", text);
            // at this point 'text' will automatically add an entity for each character in the string
            // i.e. 'text~h', 'text~o', 'text~l', etc...
            Entity.Create("text batch", textBatch);
            */
            /*
            Text text = new Text() {
                Layer = 3,
                Content = "hold nu kaeft mand :D",
                Size = new Size(128, 176),
                TextureSourceRectangle = new Rectangle(new Point(0, 16), new Size(128, 176)),

                Texture = Texture.FromFile("ass_font_tran.png")
            };

            spriteBatch.Add(text);

            Entity.Create("text-sheet", text);
            */

            IEntityRecord walls = Entity.Create("walls", 
                new Area() { 
                    Boundaries = _context.Bounds 
                }
            );

            Entity.Find("logo-cactus").Add(new AreaIntersectionTrigger() {
                Area = walls.GetComponent<Area>()
            });

            Entity.Find("logo-cactus").Add(new Bounces() {
                Friction = 0.25f
            });

            Random r = new Random();

            for (int i = 3; i < 13; i++) {
                Sprite s = new Sprite() {
                    Layer = i,

                    Size = new Size(240 / 2, 48 / 2),

                    Texture = Texture.FromFile("fruitless-logo.png")
                };

                spriteBatch.Add(s);

                Entity.Create("" + i,
                    s,
                    new Spin() {
                        Speed = 10.0f * (float)r.NextDouble()
                    },
                    new HasVelocity() {
                        Velocity = new Vector2(
                            (r.Next(0, 450) * (r.Next(0, 2) > 0 ? 1 : -1)) * (float)r.NextDouble(),
                            (r.Next(2, 700)) * (float)r.NextDouble())
                    },
                    new IsPulledByGravity(),
                    new AreaIntersectionTrigger() {
                        Area = walls.GetComponent<Area>()
                    },
                    new Bounces());
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

        double _previousRenderTime;
        double _averageRenderTime;

        double _previousUpdateTime;
        double _averageUpdateTime;

        KeyboardState _ks;
        KeyboardState _ksLast;

        bool KeyWasReleased(Key key) {
            return _ksLast[key] && !_ks[key];
        }

        protected override void OnUpdateFrame(FrameEventArgs e) {
            base.OnUpdateFrame(e);

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
            //System.Diagnostics.Debug.WriteLine(String.Format("update: {0:0.0000}ms", _averageUpdateTime));
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

            //System.Diagnostics.Debug.WriteLine(String.Format("render: {0:0.0000}ms", _averageRenderTime));
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
