using System;
using System.Drawing;
using System.Linq;
using ComponentKit;
using ComponentKit.Model;
using Fruitless;
using Fruitless.Components;
using Labs.Components.Editor;

namespace Labs {
    internal class EditableGameContext : DefaultGameContext {
        public const string EntityNamingPrefix = "editor-";
        public const string EntityNamingSuffix = "~editor";

        static string GetEditorEntityName(string name) {
            return String.Format("{0}{1}",
                EntityNamingPrefix, name);
        }

        static string GetInspectableEntityName(IEntityRecord entity) {
            return String.Format("{0}{1}",
                entity.Name, EntityNamingSuffix);
        }

        internal sealed class Entities {
            public const string SpriteBatch = EntityNamingPrefix + "spritebatch";
        }

        SpriteBatch spriteBatch;

        public EditableGameContext(Size windowBoundsInPixels)
            : base(windowBoundsInPixels) {
            spriteBatch = new SpriteBatch() {
                IsTransparent = true,
                Layer = 1
            };

            Entity.Create(Entities.SpriteBatch, spriteBatch);

            Registry.Entered += OnEntityEntered;
            Registry.Removed += OnEntityRemoved;

            Registry.SetTrigger(
                component => component is Sprite,
                (sender, args) => {
                    foreach (Sprite sprite in args.AttachedComponents.ToList()) {
                        // instead attach to name~editor entity, it makes more sense... but remember to set transform parent to the original entity!
                        /*
                        SpriteDebug debug = new SpriteDebug();
                        {
                            sprite.Record.Add(debug);
                            spriteBatch.Add(debug);
                        }*/
                    }

                    foreach (Sprite sprite in args.DettachedComponents.ToList()) {
                        // todo: figure out how to remove the SpriteDebug component, since the `sprite` no longer
                        // knows which entity it was dettached from at this point...
                    }
                }
            );
        }

        void OnEntityEntered(object sender, EntityEventArgs e) {
            if (e.Record.Name.EndsWith(EntityNamingSuffix)) {
                return;
            }

            //Entity.Create(GetInspectableEntityName(e.Record));
        }

        void OnEntityRemoved(object sender, EntityEventArgs e) {
            //Entity.Drop(GetInspectableEntityName(e.Record));
        }
    }
}
