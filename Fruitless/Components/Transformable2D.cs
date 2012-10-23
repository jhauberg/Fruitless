using OpenTK;

namespace Fruitless.Components {
    public class Transformable2D : TransformationComponent {
        private float _rotationInRadians = 0;

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

        private Vector2 _scale = Vector2.One;

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

        private Vector2 _position = Vector2.Zero;

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
