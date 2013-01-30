using Fruitless.Components;

namespace Squadtris.Components {
    internal class Health : GameComponent {
        public int Amount {
            get;
            private set;
        }

        public bool IsAlive {
            get {
                return Amount > 0;
            }
        }

        public void Decrease(int by) {
            Amount -= by;
        }

        public void Increase(int by) {
            Amount += by;
        }
    }
}
