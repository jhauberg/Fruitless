using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace Fruitless {
    public class PerspectiveCamera : Camera {
        // no
        public override void Clear() {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(Background);
        }
    }

    public abstract class Camera : ICamera {
        public static OrthographicCamera CreateOrthographic(Size boundsInPixels) {
            return new OrthographicCamera(boundsInPixels);
        }

        public static PerspectiveCamera CreatePerspective() {
            return new PerspectiveCamera();
        }

        public abstract void Clear();

        public virtual OpenTK.Graphics.Color4 Background {
            get;
            set;
        }

        public virtual Matrix4 View {
            get {
                return Matrix4.Identity;
            }
        }

        public virtual Matrix4 Projection {
            get {
                return Matrix4.Identity;
            }
        }
    }
}
