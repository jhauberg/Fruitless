using System;
using System.Runtime.InteropServices;
using OpenTK;

namespace Fruitless {
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionTexture {
        /// <summary>
        /// The vertex position.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        ///     The texture coordinates.
        /// </summary>
        public Vector2 TextureCoordinate;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="color">Color</param>
        /// <param name="textureCoordinate">Texture coordinate</param>
        public VertexPositionTexture(Vector3 position, Vector2 textureCoordinate) {
            this.Position = position;
            this.TextureCoordinate = textureCoordinate;
        }

        /// <summary>
        /// Size of the structure in bytes
        /// </summary>
        public static int SizeInBytes {
            get {
                return
                    Vector3.SizeInBytes +
                    Vector2.SizeInBytes;
            }
        }
    }
}
