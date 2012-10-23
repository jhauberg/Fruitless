using ComponentKit.Model;

namespace Fruitless.Components {
    public class GameComponent : DependencyComponent {
        public virtual void Reset() { }

        protected override void OnAdded(ComponentStateEventArgs registrationArgs) {
            base.OnAdded(registrationArgs);

            Reset();
        }
    }
}
