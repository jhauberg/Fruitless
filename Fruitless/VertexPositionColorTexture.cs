using System;
using System.Runtime.InteropServices;
using OpenTK;

namespace Fruitless {
    /// <summary>
    /// Describes a custom vertex format structure that contains position, color, and one set of texture coordinates. 
    /// </summary>
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionColorTexture {
        /// <summary>
        /// The vertex position.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        ///     The vertex color.
        /// </summary>
        public Vector4 Color;

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
        public VertexPositionColorTexture(Vector3 position, Vector4 color, Vector2 textureCoordinate) {
            this.Position = position;
            this.Color = color;
            this.TextureCoordinate = textureCoordinate;
        }

        /// <summary>
        /// Size of the structure in bytes
        /// </summary>
        public static int SizeInBytes {
            get {
                return
                    Vector3.SizeInBytes +
                    Vector4.SizeInBytes +
                    Vector2.SizeInBytes;
            }
        }

        public override string ToString() {
            return String.Format("{{ Position: {0}, Color: {1}, TextureCoordinate: {2} }}", 
                Position, Color, TextureCoordinate);
        }
    }
}
