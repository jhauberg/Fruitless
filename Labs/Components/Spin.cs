using System;
using ComponentKit.Model;
using Fruitless;
using Fruitless.Components;

namespace Labs.Components {
    internal class Spin : TimelineComponent {
        [RequireComponent]
        Transformable2D _transform = null;

        public Spin() {
            Speed = 5f;
        }

        public float Speed {
            get;
            set;
        }

        public override void Advance(TimeSpan delta) {
            _transform.Rotation += Speed * (float)delta.TotalSeconds;
        }
    }
}
