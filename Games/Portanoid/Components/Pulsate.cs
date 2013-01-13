using ComponentKit.Model;
using Fruitless;
using Fruitless.Components;
using OpenTK;
using System;

namespace Portanoid.Components {
    internal class Pulsate : TimelineComponent {
        [RequireComponent]
        Transformable2D _transform = null;

        Vector2 _originalScale;
        Vector2 _targetScale;

        float _timeElapsed;

        public Pulsate() {
            From = 1.0f;
            To = 0.6f;
            Duration = TimeSpan.FromSeconds(1.0);
        }

        public float From {
            get;
            set;
        }

        public float To {
            get;
            set;
        }
 
        public TimeSpan Duration {
            get;
            set;
        }

        public override void Advance(TimeSpan delta) {
            bool shouldReset = false;

            _timeElapsed += (float)delta.TotalSeconds;

            if (_timeElapsed > Duration.TotalSeconds) {
                _timeElapsed = (float)Duration.TotalSeconds;

                shouldReset = true;
            }

            float step = Easing.EaseOut((_timeElapsed / Duration.TotalSeconds), EasingType.Cubic);

            _transform.Scale = Vector2.Lerp(_originalScale, _targetScale, step);

            if (shouldReset) {
                Reset();
            }
        }

        public override void Reset() {
            _timeElapsed = 0;

            if (_transform != null) {
                _transform.Scale = new Vector2(From, From);

                _originalScale = _transform.Scale;
                _targetScale = new Vector2(To, To);
            }
        }
    }
}
