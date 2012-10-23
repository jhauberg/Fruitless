using OpenTK.Graphics.OpenGL;

namespace Fruitless {
    public delegate void RenderState(bool enable);

    public static class RenderStates {
        public static void Sprite(bool enable) {
            if (enable) {
                GL.Enable(EnableCap.CullFace);
                GL.FrontFace(FrontFaceDirection.Cw);
                GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);

                GL.Enable(EnableCap.DepthTest); // necessary for proper ordering of sprites (across different textures!)
                GL.Enable(EnableCap.Texture2D);
                GL.Enable(EnableCap.Blend);

                GL.DepthFunc(DepthFunction.Lequal);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            } else {
                GL.Disable(EnableCap.DepthTest);
                GL.Disable(EnableCap.CullFace);
                GL.Disable(EnableCap.Texture2D);
                GL.Disable(EnableCap.Blend);
            }
        }
    }
}
