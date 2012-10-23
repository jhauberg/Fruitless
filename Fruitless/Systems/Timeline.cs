using System;
using System.Collections.Generic;
using ComponentKit;

namespace Fruitless.Systems {
    public class Timeline : ITimeline {
        IList<ITimeline> _advanceables = new List<ITimeline>();
        IList<IAnimatable> _animatables = new List<IAnimatable>();

        double _t = 0.0;
        double _accumulated = 0.0;

        public Timeline() {
            Timescale = 1.0;
            TimeStep = 1.0 / 60.0;
        }

        public void Entered(IEnumerable<IComponent> components) {
            foreach (IComponent component in components) {
                if (component is ITimeline) {
                    ITimeline timeline = component as ITimeline;

                    if (!_advanceables.Contains(timeline)) {
                        _advanceables.Add(timeline);
                    }
                }

                if (component is IAnimatable) {
                    IAnimatable animatable = component as IAnimatable;

                    if (!_animatables.Contains(animatable)) {
                        _animatables.Add(animatable);
                    }
                }
            }
        }

        public void Removed(IEnumerable<IComponent> components) {
            foreach (IComponent component in components) {
                if (component is ITimeline) {
                    ITimeline timeline = component as ITimeline;

                    if (_advanceables.Contains(timeline)) {
                        _advanceables.Remove(timeline);
                    }
                }

                if (component is IAnimatable) {
                    IAnimatable animatable = component as IAnimatable;

                    if (_animatables.Contains(animatable)) {
                        _animatables.Remove(animatable);
                    }
                }
            }
        }

        public void Advance(TimeSpan delta) {
            if (delta.TotalSeconds > 0.25) {
                delta = TimeSpan.FromSeconds(0.25);
            }

            _accumulated += delta.TotalSeconds;

            double scaledStepTime = TimeStep * Timescale;
            
            lock (_advanceables) {
                while (_accumulated >= TimeStep) {
                    TimeSpan dt = TimeSpan.FromSeconds(scaledStepTime);

                    foreach (ITimeline timeline in _advanceables) {
                        if (timeline.IsPaused) {
                            continue;
                        }

                        timeline.Advance(dt);
                    }

                    _t += scaledStepTime;
                    _accumulated -= TimeStep;
                }
            }

            double interpolation = _accumulated / TimeStep;

            lock (_animatables) {
                foreach (IAnimatable animatable in _animatables) {
                    animatable.Animate(interpolation);
                }
            }
        }

        public double TimeStep {
            get;
            set;
        }

        public double Timescale {
            get;
            set;
        }

        public bool IsPaused {
            get;
            set;
        }
    }
}
