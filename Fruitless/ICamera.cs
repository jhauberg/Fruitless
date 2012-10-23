using OpenTK;
using OpenTK.Graphics;

namespace Fruitless {
    public interface ICamera {
        Color4 Background { get; set; }

        Matrix4 View { get; }
        Matrix4 Projection { get; }

        void Clear();
    }
}
