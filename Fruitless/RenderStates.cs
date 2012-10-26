using OpenTK.Graphics.OpenGL;

namespace Fruitless {
    public delegate void RenderState(bool enable);

    public static class RenderStates {
        public static void Sprite(bool enable) {
            if (enable) {
                GL.Enable(EnableCap.CullFace);
                GL.FrontFace(FrontFaceDirection.Cw);
                GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);

                GL.Enable(EnableCap.Texture2D);
                GL.Enable(EnableCap.Blend);

                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            } else {
                GL.Disable(EnableCap.CullFace);
                GL.Disable(EnableCap.Texture2D);
                GL.Disable(EnableCap.Blend);
            }
        }
    }
}
