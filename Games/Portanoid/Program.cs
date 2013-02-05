using ComponentKit;
using ComponentKit.Model;
using Fruitless;
using Fruitless.Components;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using Portanoid.Components;
using System;
using System.Drawing;

namespace Portanoid {    
    internal class Program : DebuggableGameWindow {
        public Program()
            : base(600, 500, "Portanoid") {
            WindowBorder = OpenTK.WindowBorder.Fixed;
            
            CursorVisible = true;

            VSync = VSyncMode.On;
            
            TargetRenderFrequency = 60;
            TargetUpdateFrequency = 60;
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);
                        
            Sprite backgroundSprite = new Sprite() {
                Layer = Entities.Layers.Back,
                Texture = Texture.FromFile("Content/Graphics/tile-red.png"),
                Size = GameContext.Bounds,
                Repeats = true
            };

            Sprite ballSprite = new Sprite() {
                Layer = Entities.Layers.Front,
                Texture = Texture.FromFile("Content/Graphics/ballBlue.png"),
            };

            Sprite portalInSprite = new Sprite() {
                Layer = Entities.Layers.Front,
                Texture = Texture.FromFile("Content/Graphics/selectorA.png"),
                TintColor = Color4.Blue
            };
            
            Sprite portalOutSprite = new Sprite() {
                Layer = Entities.Layers.Front,
                Texture = Texture.FromFile("Content/Graphics/selectorA.png"),
                TintColor = Color4.Orange
            };
            
            Sprite brickSprite = new Sprite() {
                Layer = Entities.Layers.Front,
                Texture = Texture.FromFile("Content/Graphics/element_grey_square_glossy.png")
            };

            SpriteBatch spriteBatch = Entity.Find("sprites").GetComponent<SpriteBatch>(); {
                spriteBatch.Add(backgroundSprite);
                spriteBatch.Add(ballSprite);
                spriteBatch.Add(portalInSprite);
                spriteBatch.Add(portalOutSprite);

                spriteBatch.Add(brickSprite);
            }
            
            Entity.Create(Entities.Background, backgroundSprite);
            
            Entity.Create(Entities.Ball, ballSprite, new HasVelocity());

            Entity.Create(Entities.PortalOut, portalOutSprite/*, new Pulsate() { From = 0.2f, To = 1.0f }*/);
            Entity.Create(Entities.PortalIn, portalInSprite, new Portal() { 
                Strength = 1,
                Destination = portalOutSprite.Transform 
            });
            
            Entity.Create(Entities.PortalGun, new PortalsPlacer() {
                In = portalInSprite.Transform,
                Out = portalOutSprite.Transform
            });
            
            IEntityRecord brick = Entity.Create("a brick", brickSprite);

            GameContext.Annotate(brick, "breakable");
            
            brickSprite.Transform.Position = new Vector2(0, -100);

            portalInSprite.Transform.Position = new Vector2(-100, 0);
            portalOutSprite.Transform.Position = new Vector2(100, 0);
            
            // note that the Pulsate component does exactly the same thing, but this is less lines of code!
            StartPulsating(portalInSprite.Transform, TimeSpan.FromSeconds(0.5), 1.0f, 0.75f);
            StartPulsating(portalOutSprite.Transform, TimeSpan.FromSeconds(1.0), 0.2f, 1.0f);
        }
        
        void StartPulsating(Transformable2D transform, TimeSpan duration, float from, float to) {
            Vector2 original = new Vector2(from);
            Vector2 target = new Vector2(to);

            transform.Scale = original;
  
            TaskManager.Main
                .WaitUntil(
                    elapsed => {
                        var t = elapsed / duration.TotalSeconds;
                        var step = Easing.EaseOut(t, EasingType.Cubic);

                        transform.Scale = Vector2.Lerp(original, target, step);

                        return t >= 1;
                    })
                .Then(
                    () => { 
                        StartPulsating(transform, duration, from, to); 
                    });
        }

        void Reset() {
            var ball = Entity.Find(Entities.Ball);

            ball.GetComponent<HasVelocity>().Reset();
            ball.GetComponent<Transformable2D>().Position = Vector2.Zero;
        }

        protected override void OnUpdateFrame(FrameEventArgs e) {
            base.OnUpdateFrame(e);
            
            if (KeyWasReleased(Key.Escape)) {
                Exit();
            }
            
            if (KeyWasReleased(Key.Space)) {
                Reset();
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e) {
            base.OnRenderFrame(e);

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
