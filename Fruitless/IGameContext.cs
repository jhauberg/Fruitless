using ComponentKit;
using Fruitless.Collections;

namespace Fruitless {
    public interface IGameContext {
        void Refresh(double timePassedSinceLastRefresh);
        void Render();

        IEntityRecordCollection Registry { get; }
        IBucketCollection<string, IEntityRecord> Annotations { get; }
    }
}
