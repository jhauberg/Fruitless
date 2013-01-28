using ComponentKit;
using System.Collections.Generic;

namespace Fruitless.Systems {
    internal interface ISystem {
        void Entered(IEnumerable<IComponent> components);
        void Removed(IEnumerable<IComponent> components);
    }
}
