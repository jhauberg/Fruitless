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
    /// <summary>
    /// Consumes and teleports nearby entities that are `Transformable2D` and `HasVelocity`.
    /// </summary>
    internal class Portal : TimelineComponent {
        [RequireComponent]
        Transformable2D _transform = null;

        /// <summary>
        /// The destination that teleported objects appears at.
        /// </summary>
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
            InfluenceRadius = 100;
            TerminalRadius = InfluenceRadius * 0.1f;
        }

        /// <summary>
        /// Pull an entity toward this portal.
        /// </summary>
        void Pull(IEntityRecord entity) {
            Transformable2D transform = entity.GetComponent<Transformable2D>();
            HasVelocity velocity = entity.GetComponent<HasVelocity>();

            float distance = _transform.DistanceTo(transform);
            float strength = Strength * (InfluenceRadius / distance);

            Vector2 directionTowardsPortal = Vector2.Normalize(_transform.Position - transform.Position);
            Vector2 movement = directionTowardsPortal * strength;

            velocity.Velocity += movement;
        }

        /// <summary>
        /// Consume an entity and then do something.
        /// </summary>
        void Consume(IEntityRecord entity, Vector2 original, Vector2 target, Action then) {
            Transformable2D transform = entity.GetComponent<Transformable2D>();

            TimeSpan duration = TimeSpan.FromSeconds(0.2);
            
            TaskManager.Main
                .WaitUntil(
                    elapsed => {
                        var t = elapsed / duration.TotalSeconds;
                        var step = Easing.EaseIn(t, EasingType.Cubic);

                        transform.Scale = Vector2.Lerp(original, target, step);

                        return t >= 1;
                    })
                .Then(then);
        }

        /// <summary>
        /// Make entity appear at destination after being consumed by portal.
        /// </summary>
        void Eject(IEntityRecord entity, Vector2 original, Vector2 target) {
            if (!_entitiesBeingTeleported.Contains(entity)) {
                return;
            }

            Transformable2D transform = entity.GetComponent<Transformable2D>();

            TimeSpan duration = TimeSpan.FromSeconds(0.2);

            transform.Position = Destination.Position;

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
                        transform.Scale = target;

                        _entitiesBeingTeleported.Remove(entity);
                    });
        }

        /// <summary>
        /// Teleport entity to portal destination.
        /// </summary>
        void Teleport(IEntityRecord entity) {
            if (_entitiesBeingTeleported.Contains(entity)) {
                return;
            }

            _entitiesBeingTeleported.Add(entity);

            Transformable2D transform = entity.GetComponent<Transformable2D>();

            Vector2 originalScale = new Vector2(transform.Scale.X);
            Vector2 targetScale = Vector2.Zero;

            Consume(entity, originalScale, targetScale,
                () => { 
                    Eject(entity, targetScale, originalScale); 
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
