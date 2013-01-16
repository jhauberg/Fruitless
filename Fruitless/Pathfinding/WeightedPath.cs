using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fruitless.Pathfinding {
    internal class WeightedPath<TNode> : Path<TNode> {
        public WeightedPath(TNode start) :
            this(start, null, 0) {

        }

        protected WeightedPath(TNode lastStep, Path<TNode> previousSteps, double totalCost) :
            base(lastStep, previousSteps) {
            TotalCost = totalCost;
        }

        public Path<TNode> AddStep(TNode step, double stepCost) {
            return new WeightedPath<TNode>(step, this, TotalCost + stepCost);
        }

        public double TotalCost {
            get;
            private set;
        }
    }
}
