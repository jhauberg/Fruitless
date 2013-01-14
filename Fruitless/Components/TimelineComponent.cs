using System;

namespace Fruitless.Components {
    /// <summary>
    /// Represents a component that moves along a timeline.
    /// </summary>
    public class TimelineComponent : GameComponent, ITimeline {
        /// <summary>
        /// Advances time for this component.
        /// </summary>
        public virtual void Advance(TimeSpan delta) { }

        /// <summary>
        /// Gets or sets whether this component should not be advanced with time.
        /// </summary>
        public bool IsPaused {
            get;
            set;
        }
    }
}
