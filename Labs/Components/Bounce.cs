using System;
using ComponentKit.Model;
using Fruitless;
using Fruitless.Components;
using OpenTK;

namespace Labs.Components {
    internal class Bounce : TimelineComponent {
        [RequireComponent]
        Transformable2D _transform = null;
        
        float _a;

        public Bounce() {
            Speed = 0.004f;
        }

        public float Speed {
            get;
            set;
        }

        public override void Advance(TimeSpan delta) {
            _a += Speed * (float)delta.TotalMilliseconds;

            float scale = 0.5f + (1 * (float)Math.Abs(Math.Sin(_a)));

            _transform.Scale = new Vector2(scale);
        }
    }
}
