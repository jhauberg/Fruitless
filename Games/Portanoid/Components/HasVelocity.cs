using ComponentKit.Model;
using Fruitless.Components;
using OpenTK;
using System;

namespace Portanoid.Components {
    /// <summary>
    /// Constantly applies velocity to position. No friction.
    /// </summary>
    internal class HasVelocity : TimelineComponent {
        [RequireComponent]
        Transformable2D _transform = null;

        Vector2 _velocity = Vector2.Zero;

        /// <summary>
        /// Direction + speed.
        /// </summary>
        public Vector2 Velocity {
            get {
                return _velocity;
            }
            set {
                if (_velocity != value) {
                    _velocity = value;
                }
            }
        }

        public override void Reset() {
            Velocity = Vector2.Zero;
        }

        public override void Advance(TimeSpan delta) {
            _transform.Position += _velocity * (float)delta.TotalSeconds;
        }
    }
}
