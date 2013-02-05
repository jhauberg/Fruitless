using ComponentKit;
using ComponentKit.Model;
using Fruitless.Collections;
using Fruitless.Components;
using Fruitless.Systems;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using GL = OpenTK.Graphics.OpenGL.GL;

namespace Fruitless {
    public class DefaultGameContext : IGameContext, ISynchronizable {
        Renderer _renderer = new Renderer();
        Timeline _timeline = new Timeline();
        Transformer _transformer = new Transformer();

        OrthographicCamera _camera;

        public Camera Camera {
            get {
                return _camera;
            }
        }

        Size _windowBoundsInPixels = Size.Empty;

        static bool ForTransformables(IComponent component) {
            return component is ITransformable;
        }

        static bool ForRenderables(IComponent component) {
            return component is RenderComponent;
        }

        static bool ForAnimatables(IComponent component) {
            return component is TimelineComponent;
        }

        public DefaultGameContext(Size windowBoundsInPixels) {
            Bounds = windowBoundsInPixels;

            _camera = Camera.CreateOrthographic(windowBoundsInPixels);

            Annotations = new BucketCollection<string, IEntityRecord>();

            Registry = EntityRegistry.Current;
            Registry.Entered += OnEntityEntered;
            Registry.Removed += OnEntityRemoved;

            Registry.SetTrigger(
                ForRenderables,
                (sender, args) => {
                    _renderer.Entered(args.AttachedComponents);
                    _renderer.Removed(args.DettachedComponents);
                });

            Registry.SetTrigger(
                ForTransformables,
                (sender, args) => {
                    _transformer.Entered(args.AttachedComponents);
                    _transformer.Removed(args.DettachedComponents);
                });

            Registry.SetTrigger(
                ForAnimatables,
                (sender, args) => {
                    _timeline.Entered(args.AttachedComponents);
                    _timeline.Removed(args.DettachedComponents);
                });

            _camera.Background = new Color4(255, 0, 0, 255);
        }

        void OnEntityEntered(object sender, EntityEventArgs e) {

        }

        void OnEntityRemoved(object sender, EntityEventArgs e) {
            // remove any tags associated with only this entity
            foreach (KeyValuePair<string, ICollection<IEntityRecord>> pair in Annotations) {
                pair.Value.Remove(e.Record);
            }
        }

        public void Annotate(IEntityRecord entity, string annotation) {
            Annotations.Add(annotation, entity);
        }
        
        public void Refresh(double timePassedSinceLastRefresh) {
            _transformer.ApplyTransformation();
            _timeline.Advance(TimeSpan.FromSeconds(timePassedSinceLastRefresh));
        }

        public void Render() {
            _renderer.Render(_camera);

            GL.Flush();
        }

        public Size Bounds {
            get {
                return _windowBoundsInPixels;
            }
            set {
                if (_windowBoundsInPixels != value) {
                    _windowBoundsInPixels = value;

                    GL.Viewport(
                        0, 0,
                        _windowBoundsInPixels.Width,
                        _windowBoundsInPixels.Height);
                }
            }
        }

        public IEntityRecordCollection Registry {
            get;
            private set;
        }

        public IBucketCollection<string, IEntityRecord> Annotations {
            get;
            private set;
        }

        public void Synchronize() {
            if (Registry != null) {
                Registry.Synchronize();
            }
        }

        public bool IsOutOfSync {
            get {
                return 
                    Registry == null || 
                    Registry.IsOutOfSync;
            }
        }
    }
}
