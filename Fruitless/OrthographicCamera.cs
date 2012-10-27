using OpenTK;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace Fruitless {
    public class OrthographicCamera : Camera {
        Matrix4 _projection;
        Matrix4 _view;
        
        Size _boundsInPixels;

        float _zoom;

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

        public OrthographicCamera(Size boundsInPixels) {
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
