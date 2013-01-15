using OpenTK;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace Fruitless {
    public class OrthographicCamera : Camera {
        public static OrthographicCamera Main {
            get;
            private set;
        }

        Matrix4 _projection;
        Matrix4 _view;
        
        Size _boundsInPixels;

        float _zoom;

        public OrthographicCamera(Size boundsInPixels) {
            if (Main == null) {
                Main = this;
            }

            Bounds = boundsInPixels;
            Scale = 1.0f;
        }

        void Build() {
            _view = Matrix4.Identity;
            _projection = Matrix4.CreateOrthographic(
                Bounds.Width * (1.0f * Scale), 
                Bounds.Height * (1.0f * Scale), 
                -1f, 1f);
        }

        public override void Clear() {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.ClearColor(Background);
        }

        public Vector2 GetWorldPositionFromScreen(int x, int y) {
            float centerX = Bounds.Width / 2;
            float centerY = Bounds.Height / 2;

            float deltaX = x - centerX;
            float deltaY = centerY - y;

            return new Vector2(deltaX, deltaY) * Scale;
        }

        public Size Bounds {
            get {
                return _boundsInPixels;
            }
            set {
                if (_boundsInPixels != value) {
                    _boundsInPixels = value;

                    Build();
                }
            }
        }

        public float Scale {
            get {
                return _zoom;
            }
            set {
                if (_zoom != value) {
                    _zoom = value;

                    Build();
                }
            }
        }

        public override Matrix4 Projection {
            get {
                return _projection;
            }
        }

        public override Matrix4 View {
            get {
                return _view;
            }
        }
    }
}
