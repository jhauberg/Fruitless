using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fruitless;
using Fruitless.Components;
using ComponentKit.Model;
using System.Drawing;
using OpenTK.Graphics;

namespace Labs.Components.Editor {
    public class SpriteDebug : Sprite {
        [RequireComponent]
        Sprite _sprite = null;

        public SpriteDebug() {
            Repeats = true;
            TintColor = new Color4(1f, 0f, 0f, 0.75f);

            Texture = Texture.FromFile("tile.png");
        }

        protected override void OnAdded(ComponentStateEventArgs registrationArgs) {
            base.OnAdded(registrationArgs);

            Layer =
                _sprite.Layer - 1;

            Bounds = new Size(
                _sprite.Bounds.Width + Frame.Width,
                _sprite.Bounds.Height + Frame.Height);
        }
    }
}
