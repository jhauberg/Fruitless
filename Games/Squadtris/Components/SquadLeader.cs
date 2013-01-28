using ComponentKit.Model;
using Fruitless;
using Fruitless.Components;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squadtris.Components {
    internal class SquadLeader : TimelineComponent {
        readonly static Vector2 Left = new Vector2(-1, 0);
        readonly static Vector2 Right = new Vector2(1, 0);
        readonly static Vector2 Forward = new Vector2(0, 1);
        readonly static Vector2 Backward = new Vector2(0, -1);

        [RequireComponent]
        Transformable2D _transform = null;

        [RequireComponent(FromRecordNamed = Entities.Game.SquadUnits, AllowDerivedTypes = true /* because it's probably a MappedSpriteGrid */)]
        SpriteGrid _sprites = null;

        public TimeSpan DelayBetweenMovements {
            get;
            set;
        }

        public SquadLeader() {
            DelayBetweenMovements = TimeSpan.Zero;
        }

        DateTime _lastMovementTime = DateTime.Now;

        KeyboardState _ks;
        KeyboardState _ksLast;

        void Move(Vector2 direction) {
            _transform.TranslateBy(direction * _sprites.Texture.Height);

            _lastMovementTime = DateTime.Now;
        }

        void Sprint(Vector2 direction) {
            TimeSpan snapBackwardDuration = TimeSpan.FromSeconds(0.2);
            TimeSpan snapForwardDuration = TimeSpan.FromSeconds(0.1);

            Vector2 originalPosition = _transform.Position;

            TaskManager.Main
                .WaitUntil(
                    elapsed => {
                        var t = elapsed / snapBackwardDuration.TotalSeconds;
                        var step = Easing.EaseOut(t, EasingType.Cubic);

                        _transform.Position = Vector2.Lerp(
                            originalPosition, 
                            originalPosition + (-direction * (_sprites.Texture.Height * 0.3f)),
                            step);

                        return t > 1;
                    })
                .ThenWaitUntil(
                    elapsed => {
                        var t = elapsed / snapForwardDuration.TotalSeconds;
                        var step = Easing.EaseIn(t, EasingType.Cubic);

                        Vector2 currentPosition = _transform.Position;

                        _transform.Position = Vector2.Lerp(
                            currentPosition,
                            originalPosition + (direction * (_sprites.Texture.Height * 0.2f)),
                            step);

                        return t > 1;
                    })
                .Then(
                    () => {
                        _transform.Position = originalPosition;
                    });
        }

        bool CanMove() {
            return DateTime.Now - _lastMovementTime > 
                DelayBetweenMovements;
        }

        bool KeyWasReleased(Key key) {
            return _ksLast[key] && !_ks[key];
        }

        bool KeyWasPressed(Key key) {
            return _ks[key] && !_ksLast[key];
        }

        public override void Advance(TimeSpan delta) {
            _ks = Keyboard.GetState();
            
            if (CanMove()) {
                TimeSpan timeSinceLastMove = DateTime.Now - _lastMovementTime;

                bool canSprint = timeSinceLastMove < TimeSpan.FromSeconds(0.3);


                if (KeyWasPressed(Key.A)) {
                    Move(Left);
                }

                if (KeyWasPressed(Key.D)) {
                    Move(Right);
                }

                if (KeyWasPressed(Key.W)) {
                    /*
                     * double-tap to sprint
                     * 
                    TimeSpan timeSinceLastMove = DateTime.Now - _lastMovementTime;

                    if (timeSinceLastMove < TimeSpan.FromSeconds(0.15)) {
                        Sprint(Forward);
                    } else {
                        Move(Forward);
                    }
                    */

                    /*
                     * hold space and press forward to sprint
                     * 
                    if (_ks[Key.Space]) {
                        Sprint(Forward);
                    } else {
                        Move(Forward);
                    }
                    */

                    Move(Forward);
                }

                if (KeyWasPressed(Key.S)) {
                    Move(Backward);
                }

                if (KeyWasPressed(Key.Space)) {
                    Sprint(Forward);
                }
            }

            _ksLast = _ks;
        }
    }
}
