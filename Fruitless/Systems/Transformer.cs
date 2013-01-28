using ComponentKit;
using System.Collections.Generic;

namespace Fruitless.Systems {
    /// <summary>
    /// Applies spatial transformation on `ITransformable` components.
    /// </summary>
    public class Transformer : ISystem, ITransformable {
        IList<ITransformable> _transformables =
            new List<ITransformable>();

        public void Entered(IEnumerable<IComponent> components) {
            foreach (IComponent component in components) {
                if (component is ITransformable) {
                    ITransformable transformable = component as ITransformable;

                    if (!_transformables.Contains(transformable)) {
                        _transformables.Add(transformable);
                    }
                }
            }
        }

        public void Removed(IEnumerable<IComponent> components) {
            foreach (IComponent component in components) {
                if (component is ITransformable) {
                    ITransformable transformable = component as ITransformable;

                    if (_transformables.Contains(transformable)) {
                        _transformables.Remove(transformable);
                    }
                }
            }
        }

        public void ApplyTransformation() {
            lock (_transformables) {
                foreach (ITransformable transformable in _transformables) {
                    transformable.ApplyTransformation();
                }
            }
        }
    }
}
