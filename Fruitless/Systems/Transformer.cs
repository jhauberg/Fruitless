using System.Collections.Generic;
using ComponentKit;

namespace Fruitless.Systems {
    public class Transformer : ITransformable {
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
