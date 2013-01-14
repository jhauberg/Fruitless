using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenTK.Graphics.OpenGL;

namespace Fruitless {
    /// <summary>
    /// Represents an image that is uploaded to the GPU.
    /// </summary>
    public class Texture : IDisposable, IEquatable<Texture> {
        static Dictionary<String, Texture> CachedTextures =
            new Dictionary<string, Texture>();
        
        /// <summary>
        /// Loads an image from a file.
        /// If this file has been loaded previously, a cached copy of the texture is returned.
        /// </summary>
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

        public Texture(string filename) {
            TextureID = -1;

            Load(filename);
        }

        public bool Equals(Texture other) {
            return
                Filename.Equals(other.Filename) &&
                TextureID.Equals(other.TextureID);
        }

        /// <summary>
        /// Loads an image from a file.
        /// If this texture has already loaded a different image, it will be destroyed before loading the new image.
        /// </summary>
        public void Load(string filename) {
            if (TextureID != -1) {
                if (filename.Equals(Filename)) {
                    Destroy();
                } else {
                    return;
                }
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

        /// <summary>
        /// Releases the texture and removes any cached copies.
        /// </summary>
        public void Destroy() {
            GL.DeleteTexture(TextureID);

            TextureID = -1;
            
            if (CachedTextures.ContainsKey(Filename)) {
                CachedTextures.Remove(Filename);
            }
        }

        /// <summary>
        /// Destroys the texture.
        /// </summary>
        public void Dispose() {
            Destroy();
        }

        /// <summary>
        /// Gets the filename for the currently loaded image.
        /// </summary>
        public string Filename {
            get;
            private set;
        }

        /// <summary>
        /// Gets the associated OpenGL id for this texture.
        /// </summary>
        public int TextureID {
            get;
            private set;
        }

        /// <summary>
        /// Gets the width of the image in pixels.
        /// </summary>
        public int Width {
            get;
            private set;
        }

        /// <summary>
        /// Gets the height of the image in pixels.
        /// </summary>
        public int Height {
            get;
            private set;
        }

        public override string ToString() {
            return String.Format("{0}",
                Filename);
        }
    }
}
