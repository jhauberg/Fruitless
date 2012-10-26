using OpenTK;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace Fruitless {
    public class OrthographicCamera : Camera {
        Matrix4 _projection;
        Matrix4 _view;
        
        Size _boundsInPixels;

        public OrthographicCamera(Size boundsInPixels) {
            Bounds = boundsInPixels;
        }

        void Build() {
            _projection = Matrix4.CreateOrthographic(Bounds.Width, Bounds.Height, -1f, 1f);
            _view = Matrix4.Identity;
        }

        public override void Clear() {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.ClearColor(Background);
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
