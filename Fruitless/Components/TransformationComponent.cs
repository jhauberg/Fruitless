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

        /// <summary>
        /// Gets or sets the immediate parent of this component.
        /// </summary>
        public TransformationComponent Parent {
            get {
                return _parent;
            }
            set {
                _parent = value;

                RequiresWorldResolution = true;
            }
        }

        /// <summary>
        /// Resolves any unresolved transformations.
        /// </summary>
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

                World = parentWorld * Local;

                RequiresWorldResolution = false;
                WasInvalidated = true;
            }
        }

        /// <summary>
        /// Gets or sets whether this component's world transformation should be resolved when possible.
        /// </summary>
        public bool RequiresWorldResolution {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets whether this component's world transformation was just resolved.
        /// </summary>
        public bool WasInvalidated {
            get;
            set;
        }

        /// <summary>
        /// Gets the local transformation.
        /// </summary>
        public virtual Matrix4 Local {
            get {
                return Matrix4.Identity;
            }
        }

        /// <summary>
        /// Gets the world transformation.
        /// </summary>
        public Matrix4 World {
            get;
            private set;
        }
    }
}
