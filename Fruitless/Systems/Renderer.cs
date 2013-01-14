using ComponentKit;
using Fruitless.Components;
using System.Collections.Generic;

namespace Fruitless.Systems {
    /// <summary>
    /// Renders `RenderComponents` with respect to their renderstates.
    /// Components are always sorted, and renderstates are applied/removed only when necessary.
    /// </summary>
    public class Renderer : IRenderable {
        List<RenderComponent> _renderables =
            new List<RenderComponent>();

        RenderState _currentlyEnabledState;

        public void Entered(IEnumerable<IComponent> components) {
            foreach (IComponent component in components) {
                if (component is RenderComponent) {
                    RenderComponent renderable = component as RenderComponent;

                    if (!_renderables.Contains(renderable)) {
                        _renderables.Add(renderable);
                    }
                }
            }
        }

        public void Removed(IEnumerable<IComponent> components) {
            foreach (IComponent component in components) {
                if (component is RenderComponent) {
                    RenderComponent renderable = component as RenderComponent;

                    if (_renderables.Contains(renderable)) {
                        _renderables.Remove(renderable);
                    }
                }
            }
        }

        /// <summary>
        /// Renders all renderable components using the specified camera.
        /// </summary>
        public void Render(ICamera camera) {
            lock (_renderables) {
                _renderables.Sort(delegate(RenderComponent drawable, RenderComponent otherDrawable) {
                    return drawable.CompareTo(otherDrawable);
                });
           
                camera.Clear();

                foreach (RenderComponent renderable in _renderables) {
                    if (renderable.IsHidden) {
                        continue;
                    }

                    Render(camera, renderable);
                }

                if (_currentlyEnabledState != null) {
                    _currentlyEnabledState(false);

                    _currentlyEnabledState = null;
                }
            }
        }

        /// <summary>
        /// Renders a renderable components and applies its required renderstate if necessary.
        /// </summary>
        void Render(ICamera camera, RenderComponent renderable) {
            RenderState desiredState = renderable.RenderState;

            if (desiredState != null) {
                if (_currentlyEnabledState != desiredState) {
                    if (_currentlyEnabledState != null) {
                        _currentlyEnabledState(false);
                    }

                    desiredState(true);

                    _currentlyEnabledState = desiredState;
                }
            }

            renderable.Render(camera);
        }
    }
}
