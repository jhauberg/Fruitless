using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ComponentKit.Model;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Fruitless.Components {
    // http://gamedev.stackexchange.com/questions/21220/how-exactly-does-xnas-spritebatch-work
    public class SpriteBatch : RenderComponent {
        static readonly string VertexShaderSource =
            @"
			#version 110
            
			uniform mat4 u_mvp;
            
			attribute vec3 a_position;
			attribute vec2 a_textureCoord;
            attribute vec4 a_tintColor;
            
			varying vec2 v_textureCoord;
            varying vec4 v_tintColor;

			void main() {
				gl_Position = u_mvp * vec4(a_position, 1);
                
				v_textureCoord = a_textureCoord;
                v_tintColor = a_tintColor;
			}
			";

        static readonly string FragmentShaderSource =
            @"	
			#version 110
            
			varying vec2 v_textureCoord;
	    	varying vec4 v_tintColor;

			uniform sampler2D s_texture;
            
			void main() {
                gl_FragColor = texture2D(s_texture, v_textureCoord) * v_tintColor;
			}
			";

        int shaderProgramHandle, vertexShaderHandle, fragmentShaderHandle;

        int a_positionHandle;
        int a_textureCoordHandle;
        int a_tintHandle;

        int u_mvpHandle;
        int s_textureHandle;

        uint _vbo;
        VertexPositionColorTexture[] _verticess;

        List<Sprite> _sprites =
            new List<Sprite>();

        [RequireComponent]
        Transformable2D _transform = null;

        public Transformable2D Transform {
            get {
                return _transform;
            }
        }

        public SpriteBatch() {
            RenderState = RenderStates.Sprite;

            Layer = 0;
            LayerDepth = 0;
            IsTransparent = false;
        }

        public override void Reset() {
            CreateShaderPrograms();
            CreateBufferObjects();
        }

        public void Add(Sprite sprite) {
            if (sprite.Texture.TextureID == -1) {
                throw new ArgumentException("TextureID");
            }

            if (!_sprites.Contains(sprite)) {
                _sprites.Add(sprite);

                CreateBufferObjects();
            }
        }

        public void Remove(Sprite sprite) {
            if (_sprites.Contains(sprite)) {
                if (_sprites.Remove(sprite)) {
                    CreateBufferObjects();
                }
            }
        }

        private void Build(ICamera camera) {
            _sprites.Sort();

            for (int i = 0; i < _sprites.Count; i++) {
                Sprite sprite = _sprites[i];

                if (!sprite.IsDirty &&
                    !sprite.Transform.IsInvalidated) {
                    continue;
                }

                Matrix4 world = sprite.Transform == null ?
                    Matrix4.Identity :
                    sprite.Transform.World;

                Matrix4 modelView = camera.View * world;

                float halfWidth = sprite.Bounds.Width / 2;
                float halfHeight = sprite.Bounds.Height / 2;

                float horizontalOffset = sprite.Bounds.Width * -sprite.Anchor.X;
                float verticalOffset = sprite.Bounds.Height * -sprite.Anchor.Y;

                Vector3 br = new Vector3(horizontalOffset + halfWidth, verticalOffset - halfHeight, sprite.Layer * 0.001f);
                Vector3 tl = new Vector3(horizontalOffset - halfWidth, verticalOffset + halfHeight, sprite.Layer * 0.001f);
                Vector3 bl = new Vector3(horizontalOffset - halfWidth, verticalOffset - halfHeight, sprite.Layer * 0.001f);
                Vector3 tr = new Vector3(horizontalOffset + halfWidth, verticalOffset + halfHeight, sprite.Layer * 0.001f);

                RectangleF rect = new RectangleF(
                    sprite.Frame.X,
                    sprite.Frame.Y,
                    sprite.Frame.Width * 2,
                    sprite.Frame.Height * 2);

                float uvScaleX = 1;
                float uvScaleY = 1;

                GL.BindTexture(TextureTarget.Texture2D, sprite.Texture.TextureID);
                {
                    // this needs to occur every time, because when using the same texture one sprite may want to repeat while another does not.
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                        sprite.Repeats ?
                            (int)TextureWrapMode.Repeat :
                            (int)TextureWrapMode.ClampToEdge);

                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                        sprite.Repeats ?
                            (int)TextureWrapMode.Repeat :
                            (int)TextureWrapMode.ClampToEdge);

                    if (sprite.Repeats) {
                        uvScaleX = (float)sprite.Bounds.Width / (float)sprite.Frame.Size.Width;
                        uvScaleY = (float)sprite.Bounds.Height / (float)sprite.Frame.Size.Height;
                    }
                }
                GL.BindTexture(TextureTarget.Texture2D, 0);

                Vector2 textl = new Vector2(
                    ((2 * rect.X + 1) / (2 * sprite.Texture.Width)) * uvScaleX,
                    ((2 * rect.Y + 1) / (2 * sprite.Texture.Height)) * uvScaleY);
                Vector2 texbr = new Vector2(
                    ((2 * rect.X + 1 + rect.Width - 2) / (2 * sprite.Texture.Width)) * uvScaleX,
                    ((2 * rect.Y + 1 + rect.Height - 2) / (2 * sprite.Texture.Height)) * uvScaleY);

                Vector2 texbl = new Vector2(textl.X, texbr.Y);
                Vector2 textr = new Vector2(texbr.X, textl.Y);

                int offset = i * 2 * 3;
                    
                Vector4 tint = new Vector4(
                    sprite.TintColor.R,
                    sprite.TintColor.G,
                    sprite.TintColor.B,
                    sprite.TintColor.A);
                    
                // first triangle (cw)
                int v0 = offset + 0;
                int v1 = offset + 1;
                int v2 = offset + 2;

                _verticess[v0].Position = Vector3.Transform(tl, modelView);
                _verticess[v1].Position = Vector3.Transform(br, modelView);
                _verticess[v2].Position = Vector3.Transform(bl, modelView);

                _verticess[v0].TextureCoordinate = textl;
                _verticess[v1].TextureCoordinate = texbr;
                _verticess[v2].TextureCoordinate = texbl;

                _verticess[v0].Color = tint;
                _verticess[v1].Color = tint;
                _verticess[v2].Color = tint;
                    
                // second triangle (cw)
                v0 = offset + 3;
                v1 = offset + 4;
                v2 = offset + 5;

                _verticess[v0].Position = Vector3.Transform(tl, modelView);
                _verticess[v1].Position = Vector3.Transform(tr, modelView);
                _verticess[v2].Position = Vector3.Transform(br, modelView);

                _verticess[v0].TextureCoordinate = textl;
                _verticess[v1].TextureCoordinate = textr;
                _verticess[v2].TextureCoordinate = texbr;

                _verticess[v0].Color = tint;
                _verticess[v1].Color = tint;
                _verticess[v2].Color = tint;
            }
        }

        public override void Render(ICamera camera) {
            Build(camera);
            
            GL.UseProgram(shaderProgramHandle);
            {
                Matrix4 world = Transform.World;
                Matrix4 mvp = world * camera.View * camera.Projection;

                GL.UniformMatrix4(u_mvpHandle, false, ref mvp);

                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
                {
                    GL.BufferSubData(BufferTarget.ArrayBuffer,
                        IntPtr.Zero,
                        new IntPtr(_verticess.Length * VertexPositionColorTexture.SizeInBytes),
                        _verticess);
               
                    if (_sprites.Count > 0) {
                        int startingOffset = 0;
                        Texture currentTexture = null, oldTexture = null;

                        for (int i = startingOffset; i < _sprites.Count; i++) {
                            Sprite sprite = _sprites[i];

                            currentTexture = sprite.Texture;

                            if (currentTexture != oldTexture) {
                                if (i > startingOffset) {
                                    RenderSprites(oldTexture,
                                        startingOffset,
                                        (i + 1) - startingOffset);
                                }

                                oldTexture = currentTexture;
                                startingOffset = i;
                            }
                        }

                        RenderSprites(currentTexture, 
                            startingOffset, 
                            _sprites.Count - startingOffset);
                    }
                }
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
            GL.UseProgram(0);
        }

        void RenderSprites(Texture texture, int fromIndex, int amount) {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture.TextureID);
            GL.Uniform1(s_textureHandle, 0);
            {
                GL.EnableVertexAttribArray(a_positionHandle);
                GL.EnableVertexAttribArray(a_tintHandle);
                GL.EnableVertexAttribArray(a_textureCoordHandle);
                {
                    GL.VertexAttribPointer(a_positionHandle, 3, VertexAttribPointerType.Float, false, VertexPositionColorTexture.SizeInBytes, 0);
                    GL.VertexAttribPointer(a_tintHandle, 4, VertexAttribPointerType.Float, true, VertexPositionColorTexture.SizeInBytes, 1 * Vector3.SizeInBytes);
                    GL.VertexAttribPointer(a_textureCoordHandle, 2, VertexAttribPointerType.Float, true, VertexPositionColorTexture.SizeInBytes, (1 * Vector3.SizeInBytes) + (1 * Vector4.SizeInBytes));

                    GL.DrawArrays(BeginMode.Triangles, 
                        fromIndex * 2 * 3, 
                        amount * 2 * 3);
                }
                GL.DisableVertexAttribArray(a_positionHandle);
                GL.DisableVertexAttribArray(a_tintHandle);
                GL.DisableVertexAttribArray(a_textureCoordHandle);
            }
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        void CreateShaderPrograms() {
            shaderProgramHandle = GL.CreateProgram();

            vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);

            GL.ShaderSource(vertexShaderHandle, VertexShaderSource);
            GL.ShaderSource(fragmentShaderHandle, FragmentShaderSource);

            GL.CompileShader(vertexShaderHandle);
            GL.CompileShader(fragmentShaderHandle);

            Console.WriteLine(GL.GetShaderInfoLog(vertexShaderHandle));
            Console.WriteLine(GL.GetShaderInfoLog(fragmentShaderHandle));

            GL.AttachShader(shaderProgramHandle, vertexShaderHandle);
            GL.AttachShader(shaderProgramHandle, fragmentShaderHandle);

            GL.LinkProgram(shaderProgramHandle);

            Console.WriteLine(GL.GetProgramInfoLog(shaderProgramHandle));

            a_positionHandle = GL.GetAttribLocation(shaderProgramHandle, "a_position");
            a_textureCoordHandle = GL.GetAttribLocation(shaderProgramHandle, "a_textureCoord");
            a_tintHandle = GL.GetAttribLocation(shaderProgramHandle, "a_tintColor");

            u_mvpHandle = GL.GetUniformLocation(shaderProgramHandle, "u_mvp");
            s_textureHandle = GL.GetUniformLocation(shaderProgramHandle, "s_texture");
        }

        void CreateBufferObjects() {
            DeleteBufferObjects();

            _verticess = new VertexPositionColorTexture[_sprites.Count * 2 * 3];

            GL.GenBuffers(1, out _vbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            {
                GL.BufferData(BufferTarget.ArrayBuffer,
                    new IntPtr(_verticess.Length * VertexPositionColorTexture.SizeInBytes),
                    _verticess,
                    BufferUsageHint.DynamicDraw);
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        void DeleteBufferObjects() {
            GL.DeleteBuffer(_vbo);
        }
    }
}
