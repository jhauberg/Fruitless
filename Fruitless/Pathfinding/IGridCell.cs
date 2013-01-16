using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fruitless.Pathfinding {
    public interface IGridCell : IEquatable<IGridCell> {
        int Column { get; set; }
        int Row { get; set; }
    }
}
