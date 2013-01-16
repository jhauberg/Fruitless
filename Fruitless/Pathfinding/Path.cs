using System.Collections.Generic;

namespace Fruitless.Pathfinding {
    public class Path<TNode> : IEnumerable<TNode> {
        public Path(TNode start) :
            this(start, null) {

        }

        protected Path(TNode lastStep, Path<TNode> previousSteps) {
            LastStep = lastStep;
            PreviousSteps = previousSteps;
        }

        public virtual Path<TNode> AddStep(TNode step) {
            return new Path<TNode>(step, this);
        }

        public IEnumerator<TNode> GetEnumerator() {
            for (Path<TNode> p = this; p != null; p = p.PreviousSteps) {
                yield return p.LastStep;
            }
        }

        public TNode LastStep {
            get;
            private set;
        }

        public Path<TNode> PreviousSteps {
            get;
            private set;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
