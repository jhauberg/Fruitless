using System.Collections.Generic;

namespace Fruitless.Pathfinding {
    public interface IHasNeighbors<TNode> {
        IEnumerable<TNode> GetNeighbours(TNode[,] grid);
    }
}
