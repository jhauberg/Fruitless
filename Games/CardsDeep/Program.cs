using ComponentKit;
using ComponentKit.Model;
using Fruitless;
using Fruitless.Components;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using System;
using System.Drawing;

namespace Squadtris {
    internal class Program : DebuggableGameWindow {
        public Program()
            : base(960, 640, "Cards Deep") {
            WindowBorder = OpenTK.WindowBorder.Fixed;

            CursorVisible = true;

            VSync = VSyncMode.On;

            TargetRenderFrequency = 60;
            TargetUpdateFrequency = 60;
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            GameContext.Camera.Background = new Color4(255, 255, 255, 255);
        }

        protected override void OnUpdateFrame(FrameEventArgs e) {
            base.OnUpdateFrame(e);

            if (KeyWasReleased(Key.Escape)) {
                Exit();
            }
        }

        [STAThread]
        internal static void Main() {
            using (Program game = new Program()) {
                game.Icon = new Icon("icon.ico");
                game.Run(game.TargetUpdateFrequency);
            }
        }
    }
}
