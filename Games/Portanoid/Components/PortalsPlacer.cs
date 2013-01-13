using Fruitless;
using Fruitless.Components;
using OpenTK;
using OpenTK.Input;
using System;

namespace Portanoid.Components {
    /// <summary>
    /// Places the `in` and `out` portals using the left and right mouse buttons.
    /// </summary>
    internal class PortalsPlacer : TimelineComponent {
        /// <summary>
        /// Stuff goes in here.
        /// </summary>
        [ComponentOutlet]
        public Transformable2D In { get; set; }
        /// <summary>
        /// Stuff comes out here.
        /// </summary>
        [ComponentOutlet]
        public Transformable2D Out { get; set; }

        MouseState _ms;
        MouseState _msLast;

        public override void Advance(TimeSpan delta) {
            _ms = Mouse.GetState();

            if (_msLast[MouseButton.Left] && _ms.IsButtonUp(MouseButton.Left)) {
                if (In != null) {
                    In.Position = new Vector2(_ms.X, _ms.Y);
                }
            }

            if (_msLast[MouseButton.Right] && _ms.IsButtonUp(MouseButton.Right)) {
                if (Out != null) {
                    Out.Position = new Vector2(_ms.X, _ms.Y);
                }
            }

            _msLast = _ms;
        }
    }
}
