using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ComponentKit.Model;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Fruitless.Components {
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

        // todo: combine vertices and vbo id into a struct or class to avoid having 2 dicts
        Dictionary<Texture, uint> _vbos =
            new Dictionary<Texture, uint>();
        Dictionary<Texture, VertexPositionColorTexture[]> _vertices =
            new Dictionary<Texture, VertexPositionColorTexture[]>();

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
                _sprites.Sort();

                sprite.TextureChanged += OnTextureChanged;

                Prepare(sprite.Texture);

                CreateBufferObjects();
            }
        }

        void Prepare(Texture texture) {
            int numberOfSprites = _sprites
                .Where(sprite => sprite.Texture.Equals(texture))
                .Count();

            if (numberOfSprites > 0) {
                _vertices[texture] =
                    new VertexPositionColorTexture[numberOfSprites * 2 * 3];
            } else {
                _vertices.Remove(texture);
            }
        }

        void OnTextureChanged(object sender, TextureChangedEventArgs e) {
            Prepare(e.Previous);
            Prepare(e.Current);
        }

        public void Remove(Sprite sprite) {
            if (_sprites.Contains(sprite)) {
                if (_sprites.Remove(sprite)) {
                    CreateBufferObjects();
                }
            }
        }

        private void Build(ICamera camera) {
            for (int j = 0; j < _vertices.Count; j++) {
                Texture texture = _vertices.Keys.ElementAt(j);

                IList<Sprite> sprites = _sprites
                    .Where(sprite => sprite.Texture == texture)
                    .OrderBy(sprite => sprite.Layer)
                    .ToList();

                for (int i = 0; i < sprites.Count; i++) {
                    Sprite sprite = sprites[i];

                    //System.Diagnostics.Debug.WriteLine("" + sprite.Layer);

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

                    VertexPositionColorTexture[] vertices = _vertices[sprite.Texture];

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

                    vertices[v0].Position = Vector3.Transform(tl, modelView);
                    vertices[v1].Position = Vector3.Transform(br, modelView);
                    vertices[v2].Position = Vector3.Transform(bl, modelView);

                    vertices[v0].TextureCoordinate = textl;
                    vertices[v1].TextureCoordinate = texbr;
                    vertices[v2].TextureCoordinate = texbl;
                    
                    vertices[v0].Color = tint;
                    vertices[v1].Color = tint;
                    vertices[v2].Color = tint;
                    
                    // second triangle (cw)
                    v0 = offset + 3;
                    v1 = offset + 4;
                    v2 = offset + 5;

                    vertices[v0].Position = Vector3.Transform(tl, modelView);
                    vertices[v1].Position = Vector3.Transform(tr, modelView);
                    vertices[v2].Position = Vector3.Transform(br, modelView);

                    vertices[v0].TextureCoordinate = textl;
                    vertices[v1].TextureCoordinate = textr;
                    vertices[v2].TextureCoordinate = texbr;
                    
                    vertices[v0].Color = tint;
                    vertices[v1].Color = tint;
                    vertices[v2].Color = tint;
         
                    _vertices[sprite.Texture] = vertices;
                }
            }
        }

        public override void Render(ICamera camera) {
            Build(camera);

            Matrix4 world = Transform.World;
            Matrix4 mvp = world * camera.View * camera.Projection;

            GL.UseProgram(shaderProgramHandle);
            {
                foreach (Texture texture in _vertices.Keys) {
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, texture.TextureID);
                    GL.Uniform1(s_textureHandle, 0);

                    GL.UniformMatrix4(u_mvpHandle, false, ref mvp);

                    GL.EnableVertexAttribArray(a_positionHandle);
                    GL.EnableVertexAttribArray(a_tintHandle);
                    GL.EnableVertexAttribArray(a_textureCoordHandle);
                    {
                        VertexPositionColorTexture[] vertices = _vertices[texture];

                        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbos[texture]);
                        {
                            GL.BufferSubData(BufferTarget.ArrayBuffer,
                                IntPtr.Zero,
                                new IntPtr(vertices.Length * VertexPositionColorTexture.SizeInBytes),
                                vertices);

                            GL.VertexAttribPointer(a_positionHandle, 3, VertexAttribPointerType.Float, false, VertexPositionColorTexture.SizeInBytes, 0);
                            GL.VertexAttribPointer(a_tintHandle, 4, VertexAttribPointerType.Float, true, VertexPositionColorTexture.SizeInBytes, 1 * Vector3.SizeInBytes);
                            GL.VertexAttribPointer(a_textureCoordHandle, 2, VertexAttribPointerType.Float, true, VertexPositionColorTexture.SizeInBytes, (1 * Vector3.SizeInBytes) + (1 * Vector4.SizeInBytes));
                        }
                        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                        GL.DrawArrays(BeginMode.Triangles, 0, vertices.Length);
                    }
                    GL.DisableVertexAttribArray(a_positionHandle);
                    GL.DisableVertexAttribArray(a_tintHandle);
                    GL.DisableVertexAttribArray(a_textureCoordHandle);

                    GL.BindTexture(TextureTarget.Texture2D, 0);
                }
            }
            GL.UseProgram(0);
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

            foreach (Texture texture in _vertices.Keys) {
                uint vbo;

                VertexPositionColorTexture[] vertices = _vertices[texture];

                GL.GenBuffers(1, out vbo);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                {
                    GL.BufferData(BufferTarget.ArrayBuffer,
                        new IntPtr(vertices.Length * VertexPositionColorTexture.SizeInBytes),
                        vertices,
                        BufferUsageHint.DynamicDraw);
                }
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                _vbos[texture] = vbo;
            }
        }

        void DeleteBufferObjects() {
            if (_vbos.Count > 0) {
                GL.DeleteBuffers(_vbos.Count, _vbos.Values.ToArray());
            }

            _vbos.Clear();
        }
    }
}
