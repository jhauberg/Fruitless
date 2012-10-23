using System;

namespace Fruitless.Components {
    public class TimelineComponent : GameComponent, ITimeline {
        public bool IsPaused {
            get;
            set;
        }

        public virtual void Advance(TimeSpan delta) {

        }

        public override void Reset() {

        }
    }
}
