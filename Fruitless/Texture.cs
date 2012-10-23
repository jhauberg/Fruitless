using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenTK.Graphics.OpenGL;

namespace Fruitless {
    public class Texture : IDisposable, IEquatable<Texture> {
        static Dictionary<String, Texture> CachedTextures =
            new Dictionary<string, Texture>();

        public static Texture FromFile(string filename) {
            Texture texture = null;

            if (CachedTextures.ContainsKey(filename)) {
                texture = CachedTextures[filename];
            } else {
                texture = new Texture(filename);

                CachedTextures[filename] = texture;
            }

            return texture;
        }

        bool _repeats;

        public Texture(string filename) {
            TextureID = -1;

            Load(filename);
        }

        public void Load(string filename) {
            if (TextureID != -1) {
                Destroy();
            }

            TextureID = -1;

            try {
                using (Bitmap bitmap = new Bitmap(filename)) {
                    TextureID = GL.GenTexture();

                    BitmapData bitmapData = bitmap.LockBits(
                        new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        ImageLockMode.ReadOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    Width = bitmapData.Width;
                    Height = bitmapData.Height;

                    GL.BindTexture(TextureTarget.Texture2D, TextureID);
                    {
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmapData.Width, bitmapData.Height, 0,
                            OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bitmapData.Scan0);

                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);

                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                    }
                    GL.BindTexture(TextureTarget.Texture2D, 0);

                    bitmap.UnlockBits(bitmapData);
                }
            } catch (FileNotFoundException) {

            }

            if (TextureID != -1) {
                Filename = filename;
            }
        }

        public void Destroy() {
            GL.DeleteTexture(TextureID);

            TextureID = -1;

            if (CachedTextures.ContainsKey(Filename)) {
                CachedTextures.Remove(Filename);
            }
        }

        public bool Equals(Texture other) {
            return
                Filename.Equals(other.Filename) &&
                TextureID.Equals(other.TextureID);
        }

        public void Dispose() {
            Destroy();
        }

        public string Filename {
            get;
            private set;
        }

        public int TextureID {
            get;
            private set;
        }

        public int Width {
            get;
            private set;
        }

        public int Height {
            get;
            private set;
        }

        public bool Repeats {
            get {
                return _repeats;
            }
            set {
                if (_repeats != value) {
                    _repeats = value;

                    GL.BindTexture(TextureTarget.Texture2D, TextureID);
                    {
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                            _repeats ?
                                (int)TextureWrapMode.Repeat :
                                (int)TextureWrapMode.ClampToEdge);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                            _repeats ?
                                (int)TextureWrapMode.Repeat :
                                (int)TextureWrapMode.ClampToEdge);
                    }
                    GL.BindTexture(TextureTarget.Texture2D, 0);
                }
            }
        }
    }
}
