using ComponentKit.Model;

namespace Fruitless.Components {
    /// <summary>
    /// Provides generally useful methods for most types of components.
    /// </summary>
    public class GameComponent : DependencyComponent {
        public virtual void Reset() { }

        protected override void OnAdded(ComponentStateEventArgs registrationArgs) {
            base.OnAdded(registrationArgs);

            Reset();
        }
    }
}
