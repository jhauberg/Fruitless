using System;

namespace Fruitless {
    public interface ITimeline : IAdvanceable<TimeSpan> {
        bool IsPaused { get; }
    }
}
