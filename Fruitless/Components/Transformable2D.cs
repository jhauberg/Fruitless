using OpenTK;

namespace Fruitless.Components {
    public class Transformable2D : TransformationComponent { 
        float _rotationInRadians = 0;

        /// <summary>
        /// The angle of rotation in radians.
        /// </summary>
        public float Rotation {
            get {
                return _rotationInRadians;
            }
            set {
                if (_rotationInRadians != value) {
                    _rotationInRadians = value;

                    RequiresWorldResolution = true;
                }
            }
        }

        Vector2 _scale = Vector2.One;

        /// <summary>
        /// Defaults to Vector2.One (1, 1).
        /// </summary>
        public Vector2 Scale {
            get {
                return _scale;
            }
            set {
                if (_scale != value) {
                    _scale = value;

                    RequiresWorldResolution = true;
                }
            }
        }

        Vector2 _position = Vector2.Zero;

        /// <summary>
        /// The position in pixels relative to its parent.
        /// </summary>
        public Vector2 Position {
            get {
                return _position;
            }
            set {
                if (_position != value) {
                    _position = value;

                    RequiresWorldResolution = true;
                }
            }
        }
        
        /// <summary>
        /// The absolute position in pixels.
        /// </summary>
        public Vector2 AbsolutePosition {
            get {
                return new Vector2(World.M41, World.M42);
            }
        }
        
        public override Matrix4 Local {
            get {
                return
                    Matrix4.Scale(Scale.X, Scale.Y, 1) *
                    Matrix4.CreateRotationZ(_rotationInRadians) *
                    Matrix4.CreateTranslation(Position.X, Position.Y, 0);
            }
        }
    }
}
