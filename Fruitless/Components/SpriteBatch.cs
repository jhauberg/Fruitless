using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ComponentKit.Model;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Fruitless.Components {
    // http://gamedev.stackexchange.com/questions/21220/how-exactly-does-xnas-spritebatch-work

    /// <summary>
    /// Provides an efficient way of rendering many sprites with as few draw calls as possible.
    /// </summary>
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

        int _shaderProgramHandle, _vertexShaderHandle, _fragmentShaderHandle;

        int a_positionHandle;
        int a_textureCoordHandle;
        int a_tintHandle;

        int u_mvpHandle;
        int s_textureHandle;

        uint _vbo;
        VertexPositionColorTexture[] _vertices;

        List<Sprite> _sprites = new List<Sprite>();

        [RequireComponent]
        Transformable2D _transform = null;

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
            if (!_sprites.Contains(sprite)) {
                _sprites.Add(sprite);
                
                sprite.LayerChanged += OnSpriteLayerChanged;
                sprite.Removed += OnSpriteRemoved;

                _sprites.Sort();

                CreateBufferObjects();
            }
        }

        void OnSpriteRemoved(object sender, ComponentStateEventArgs e) {
            Remove(sender as Sprite);
        }

        void OnSpriteLayerChanged(object sender, LayerChangedEventArgs e) {
            _sprites.Sort();

            CreateBufferObjects();
        }

        public void Remove(Sprite sprite) {
            if (_sprites.Contains(sprite)) {
                if (_sprites.Remove(sprite)) {
                    sprite.LayerChanged -= OnSpriteLayerChanged;
                    sprite.Removed -= OnSpriteRemoved;

                    CreateBufferObjects();
                }
            }
        }

        void Build(ICamera camera) {
            Build(camera, all: false);
        }

        void Build(ICamera camera, bool all) {
            foreach (Sprite sprite in _sprites) {
                if (all || sprite.Transform != null && sprite.Transform.WasInvalidated) {
                    TransformPositions(sprite, camera);
                }

                if (all || sprite.IsDirty) {
                    CalculateTextureCoordinates(sprite);

                    sprite.IsDirty = false;
                }
            }
        }

        void CreateBufferObjects() {
            DeleteBufferObjects();

            _vertices = new VertexPositionColorTexture[_sprites.Count * 2 * 3];

            // since we're being lazy and just create a brand new vertex array every time a single sprite is added,
            // we better make sure to also do all the calculations again... (otherwise, the first sprites added
            // may never be represented correctly in the vertexbuffer if they never become Dirty or Invalidated!)
            Build(camera: null, all: true);
            
            GL.GenBuffers(1, out _vbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            {
                GL.BufferData(BufferTarget.ArrayBuffer,
                    new IntPtr(_vertices.Length * VertexPositionColorTexture.SizeInBytes),
                    _vertices,
                    BufferUsageHint.DynamicDraw);
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        void DeleteBufferObjects() {
            GL.DeleteBuffer(_vbo);
        }

        void CreateShaderPrograms() {
            _shaderProgramHandle = GL.CreateProgram();

            _vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            _fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);

            GL.ShaderSource(_vertexShaderHandle, VertexShaderSource);
            GL.ShaderSource(_fragmentShaderHandle, FragmentShaderSource);

            GL.CompileShader(_vertexShaderHandle);
            GL.CompileShader(_fragmentShaderHandle);

            GL.AttachShader(_shaderProgramHandle, _vertexShaderHandle);
            GL.AttachShader(_shaderProgramHandle, _fragmentShaderHandle);

            GL.LinkProgram(_shaderProgramHandle);

            a_positionHandle = GL.GetAttribLocation(_shaderProgramHandle, "a_position");
            a_textureCoordHandle = GL.GetAttribLocation(_shaderProgramHandle, "a_textureCoord");
            a_tintHandle = GL.GetAttribLocation(_shaderProgramHandle, "a_tintColor");

            u_mvpHandle = GL.GetUniformLocation(_shaderProgramHandle, "u_mvp");
            s_textureHandle = GL.GetUniformLocation(_shaderProgramHandle, "s_texture");
        }

        void TransformPositions(Sprite sprite, ICamera camera) {
            Matrix4 world = sprite.Transform != null ? sprite.Transform.World : Matrix4.Identity;
            Matrix4 view = camera != null ? camera.View : Matrix4.Identity;

            Matrix4 modelView = view * world;

            float halfWidth = sprite.Size.Width / 2;
            float halfHeight = sprite.Size.Height / 2;

            float horizontalOffset = sprite.Size.Width * -sprite.Anchor.X;
            float verticalOffset = sprite.Size.Height * -sprite.Anchor.Y;

            Vector3 br = new Vector3(horizontalOffset + halfWidth, verticalOffset - halfHeight, 0);
            Vector3 tl = new Vector3(horizontalOffset - halfWidth, verticalOffset + halfHeight, 0);
            Vector3 bl = new Vector3(horizontalOffset - halfWidth, verticalOffset - halfHeight, 0);
            Vector3 tr = new Vector3(horizontalOffset + halfWidth, verticalOffset + halfHeight, 0);

            int offset = _sprites.IndexOf(sprite) * 2 * 3;

            _vertices[offset + 0].Position = Vector3.Transform(tl, modelView);
            _vertices[offset + 1].Position = Vector3.Transform(br, modelView);
            _vertices[offset + 2].Position = Vector3.Transform(bl, modelView);

            _vertices[offset + 3].Position = Vector3.Transform(tl, modelView);
            _vertices[offset + 4].Position = Vector3.Transform(tr, modelView);
            _vertices[offset + 5].Position = Vector3.Transform(br, modelView);
        }

        void CalculateTextureCoordinates(Sprite sprite) {
            RectangleF sourceFrame = new RectangleF(
                sprite.TextureSourceRectangle.X,
                sprite.TextureSourceRectangle.Y,
                sprite.TextureSourceRectangle.Width * 2,
                sprite.TextureSourceRectangle.Height * 2);

            Vector2 tl = Vector2.Zero;
            Vector2 br = Vector2.Zero;

            Vector2 bl = Vector2.Zero;
            Vector2 tr = Vector2.Zero;

            if (sprite.Texture != null) {
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
                        uvScaleX = (float)sprite.Size.Width / (float)sprite.TextureSourceRectangle.Size.Width;
                        uvScaleY = (float)sprite.Size.Height / (float)sprite.TextureSourceRectangle.Size.Height;
                    }
                }
                GL.BindTexture(TextureTarget.Texture2D, 0);

                int w = 2 * sprite.Texture.Width;
                int h = 2 * sprite.Texture.Height;

                tl = new Vector2(
                    ((2 * sourceFrame.X + 1) / w) * uvScaleX,
                    ((2 * sourceFrame.Y + 1) / h) * uvScaleY);
                br = new Vector2(
                    ((2 * sourceFrame.X + 1 + sourceFrame.Width - 2) / w) * uvScaleX,
                    ((2 * sourceFrame.Y + 1 + sourceFrame.Height - 2) / h) * uvScaleY);

                bl = new Vector2(tl.X, br.Y);
                tr = new Vector2(br.X, tl.Y);
            }

            int offset = _sprites.IndexOf(sprite) * 2 * 3;

            _vertices[offset + 0].TextureCoordinate = tl;
            _vertices[offset + 1].TextureCoordinate = br;
            _vertices[offset + 2].TextureCoordinate = bl;

            _vertices[offset + 3].TextureCoordinate = tl;
            _vertices[offset + 4].TextureCoordinate = tr;
            _vertices[offset + 5].TextureCoordinate = br;

            Vector4 tint = new Vector4(
                sprite.TintColor.R,
                sprite.TintColor.G,
                sprite.TintColor.B,
                sprite.TintColor.A);

            _vertices[offset].Color = tint;
            _vertices[offset + 1].Color = tint;
            _vertices[offset + 2].Color = tint;

            _vertices[offset + 3].Color = tint;
            _vertices[offset + 4].Color = tint;
            _vertices[offset + 5].Color = tint;
        }

        public override void Render(ICamera camera) {
            Build(camera);
            
            GL.UseProgram(_shaderProgramHandle);
            {
                Matrix4 world = Transform.World;
                Matrix4 mvp = world * camera.View * camera.Projection;

                GL.UniformMatrix4(u_mvpHandle, false, ref mvp);

                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
                {
                    GL.BufferSubData(BufferTarget.ArrayBuffer,
                        IntPtr.Zero,
                        new IntPtr(_vertices.Length * VertexPositionColorTexture.SizeInBytes),
                        _vertices);
               
                    if (_sprites.Count > 0) {
                        int startingOffset = 0;

                        Texture currentTexture = null;
                        Texture previousTexture = null;

                        for (int i = startingOffset; i < _sprites.Count; i++) {
                            Sprite sprite = _sprites[i];

                            currentTexture = sprite.Texture;

                            if (currentTexture != previousTexture) {
                                if (i > startingOffset) {
                                    RenderSprites(previousTexture,
                                        fromIndex: startingOffset,
                                        amount: i - startingOffset);
                                }

                                previousTexture = currentTexture;
                                startingOffset = i;
                            }
                        }
                        
                        RenderSprites(currentTexture, 
                            fromIndex: startingOffset, 
                            amount: _sprites.Count - startingOffset);
                    }
                }
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
            GL.UseProgram(0);
        }

        void RenderSprites(Texture texture, int fromIndex, int amount) {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture != null ? texture.TextureID : 0);
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

        public Transformable2D Transform {
            get {
                return _transform;
            }
        }
    }
}
