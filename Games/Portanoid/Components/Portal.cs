using ComponentKit;
using ComponentKit.Model;
using Fruitless;
using Fruitless.Components;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Portanoid.Components {
    // sucks in entities that are Transformable and HasVelocity
    // can have a target transform which is where sucked in objects will appear (i.e. the out portal)
    internal class Portal : TimelineComponent {
        [RequireComponent]
        Transformable2D _transform = null;

        [ComponentOutlet]
        public Transformable2D Destination { get; set; }

        List<IEntityRecord> _entitiesBeingTeleported = 
            new List<IEntityRecord>();

        /// <summary>
        /// The strength that this portal pulls stuff in with.
        /// Closer objects are pulled with more strength.
        /// </summary>
        public float Strength {
            get;
            set;
        }

        /// <summary>
        /// The radius of influence for this portal.
        /// </summary>
        public float InfluenceRadius {
            get;
            set;
        }

        /// <summary>
        /// The radius of which any object within will be sucked in completely.
        /// </summary>
        public float TerminalRadius {
            get;
            set;
        }

        public Portal() {
            Strength = 1;
            InfluenceRadius = 105;
            TerminalRadius = InfluenceRadius * 0.1f;
        }

        void Pull(IEntityRecord entity) {
            Transformable2D transform = entity.GetComponent<Transformable2D>();
            HasVelocity velocity = entity.GetComponent<HasVelocity>();

            float distance = _transform.DistanceTo(transform);
            float strength = Strength * (InfluenceRadius / distance);

            Vector2 directionTowardsPortal = Vector2.Normalize(_transform.Position - transform.Position);
            Vector2 movement = directionTowardsPortal * strength;

            velocity.Velocity += movement;
        }

        void Teleport(IEntityRecord entity) {
            if (_entitiesBeingTeleported.Contains(entity)) {
                return;
            }

            _entitiesBeingTeleported.Add(entity);

            Transformable2D transform = entity.GetComponent<Transformable2D>();
            
            TimeSpan duration = TimeSpan.FromSeconds(0.2);

            Vector2 original = transform.Scale;
            Vector2 target = Vector2.Zero;

            TaskManager.Main
                .WaitUntil(
                    elapsed => {
                        var t = elapsed / duration.TotalSeconds;
                        var step = Easing.EaseIn(t, EasingType.Cubic);

                        transform.Scale = Vector2.Lerp(original, target, step);

                        return t >= 1;
                    })
                .Then(
                    () => {
                        transform.Position = Destination.Position;

                        TaskManager.Main
                            .WaitUntil(
                                elapsed => {
                                    var t = elapsed / duration.TotalSeconds;
                                    var step = Easing.EaseOut(t, EasingType.Cubic);

                                    transform.Scale = Vector2.Lerp(target, original, step);

                                    return t >= 1;
                                })
                            .Then(
                                () => {
                                    transform.Scale = original;

                                    _entitiesBeingTeleported.Remove(entity);
                                });
                    });
        }

        public override void Advance(TimeSpan delta) {
            IEnumerable<IEntityRecord> entities = 
                Entity.FindAllWithComponents<Transformable2D, HasVelocity>();

            foreach (IEntityRecord entity in entities) {
                if (_entitiesBeingTeleported.Contains(entity)) {
                    continue;
                }

                Transformable2D transform = entity.GetComponent<Transformable2D>();

                float distance = _transform.DistanceTo(transform);

                if (distance <= InfluenceRadius) {
                    if (distance <= TerminalRadius) {
                        Teleport(entity);
                    } else {
                        Pull(entity);
                    }
                }
            }
        }
    }
}
