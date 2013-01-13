using Fruitless;
using Fruitless.Components;
using OpenTK;
using OpenTK.Input;
using System;

namespace Portanoid.Components {
    /// <summary>
    /// Places the `in` and `out` portals using the left and right mouse buttons.
    /// </summary>
    internal class SetPortals : TimelineComponent {
        [ComponentOutlet]
        public Transformable2D InPortalTransform { get; set; }
        [ComponentOutlet]
        public Transformable2D OutPortalTransform { get; set; }

        MouseState _ms;
        MouseState _msLast;

        // todo: show shadow of portal when button=down and place it when releasing.

        public override void Advance(TimeSpan delta) {
            _ms = Mouse.GetState();

            if (_msLast[MouseButton.Left] && _ms.IsButtonUp(MouseButton.Left)) {
                if (InPortalTransform != null) {
                    InPortalTransform.Position = new Vector2(_ms.X, _ms.Y);
                }
            }
            /*
            System.Diagnostics.Debug.WriteLine(
                String.Format("{0}, {1}", _ms.X, _ms.Y));
            */
            _msLast = _ms;
        }
    }
}
