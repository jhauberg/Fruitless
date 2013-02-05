using ComponentKit;
using ComponentKit.Model;
using Fruitless.Components;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using System;

namespace Fruitless {
    internal sealed class Entities {
        public static class Shared {
            public const string World = "~";
            public const string Tasks = "tasks";
            public const string Sprites = "sprites";
        }
    }

    public class DefaultGameWindow : GameWindow {
        public static class Cursor {
            public static int X {
                get;
                set;
            }

            public static int Y {
                get;
                set;
            }
        }

        public DefaultGameContext GameContext {
            get;
            private set;
        }

        KeyboardState _ks;
        KeyboardState _ksLast;

        double _previousRenderTime;
        double _averageRenderTime;

        double _previousUpdateTime;
        double _averageUpdateTime;

        public DefaultGameWindow(int width, int height, string title) :
            base(width, height, GraphicsMode.Default, title) {
            Cursor.X = Mouse.X;
            Cursor.Y = Mouse.Y;
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            GameContext = new DefaultGameContext(
                windowBoundsInPixels: ClientRectangle.Size);

            GameContext.Registry.Entered += OnEntityEntered;
            GameContext.Registry.Removed += OnEntityRemoved;

            CreateSharedEntities();
        }

        void CreateSharedEntities() {
            Entity.Create(Entities.Shared.World, new Transformable2D());
            Entity.Create(Entities.Shared.Tasks, new TaskManager());
            Entity.Create(Entities.Shared.Sprites, new SpriteBatch());
        }
        
        protected virtual void OnEntityEntered(object sender, EntityEventArgs e) {
            
        }

        protected virtual void OnEntityRemoved(object sender, EntityEventArgs e) {
            
        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);

            GameContext.Bounds = ClientRectangle.Size;
        }

        protected bool KeyWasPressed(Key key) {
            return _ks[key] && !_ksLast[key];
        }

        protected bool KeyWasReleased(Key key) {
            return _ksLast[key] && !_ks[key];
        }

        protected override void OnUpdateFrame(FrameEventArgs e) {
            base.OnUpdateFrame(e);
            
            Cursor.X = Mouse.X;
            Cursor.Y = Mouse.Y;

            _ksLast = _ks;
            _ks = OpenTK.Input.Keyboard.GetState();

            GameContext.Refresh(e.Time);

            if (GameContext.IsOutOfSync) {
                GameContext.Synchronize();
            }

            if (_previousUpdateTime > 0) {
                double weightRatio = 0.1;

                _averageUpdateTime = _averageUpdateTime * (1.0 - weightRatio) + _previousUpdateTime * weightRatio;
            }

            _previousUpdateTime = UpdateTime;
        }

        protected override void OnRenderFrame(FrameEventArgs e) {
            base.OnRenderFrame(e);

            GameContext.Render();

            SwapBuffers();

            if (_previousRenderTime > 0) {
                double weightRatio = 0.1;

                _averageRenderTime = _averageRenderTime * (1.0 - weightRatio) + _previousRenderTime * weightRatio;
            }

            _previousRenderTime = RenderTime;
        }
    }
}
