using System.Collections.Generic;
using ComponentKit;
using Fruitless.Components;
using System;

namespace Fruitless.Systems {
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
