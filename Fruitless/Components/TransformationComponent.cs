using ComponentKit.Model;
using OpenTK;

namespace Fruitless.Components {
    /// <summary>
    /// Provides properties and methods for spatial transformation.
    /// </summary>
    public class TransformationComponent : Component, ITransformable {
        public TransformationComponent() {
            World = Matrix4.Identity;

            RequiresWorldResolution = true;
        }

        TransformationComponent _parent;

        public TransformationComponent Parent {
            get {
                return _parent;
            }
            set {
                _parent = value;

                RequiresWorldResolution = true;
            }
        }

        public void ApplyTransformation() {
            WasInvalidated = false;

            bool shouldResolveWorldTransformation = RequiresWorldResolution;

            if (!shouldResolveWorldTransformation) {
                TransformationComponent parent = _parent;

                while (parent != null) {
                    if (parent.WasInvalidated) {
                        RequiresWorldResolution = true;

                        break;
                    }

                    parent = parent.Parent;
                }
            }

            if (RequiresWorldResolution) {
                Matrix4 parentWorld = Matrix4.Identity;

                if (_parent != null) {
                    bool parentWasInvalidated = _parent.WasInvalidated;

                    _parent.ApplyTransformation();
                    _parent.WasInvalidated = parentWasInvalidated;

                    parentWorld = _parent.World;
                }

                // note - for inheritance of scale/rotation/translation to work separately, we need to get each of these matrices
                World = parentWorld * Local;

                RequiresWorldResolution = false;
                WasInvalidated = true;
            }
        }

        public bool RequiresWorldResolution {
            get;
            set;
        }
        
        public bool WasInvalidated {
            get;
            set;
        }

        public virtual Matrix4 Local {
            get {
                return Matrix4.Identity;
            }
        }

        public Matrix4 World {
            get;
            private set;
        }
    }
}
