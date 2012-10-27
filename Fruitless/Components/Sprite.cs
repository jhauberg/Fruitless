using System;
using System.Drawing;
using ComponentKit.Model;
using OpenTK;
using OpenTK.Graphics;
using Fruitless.Utility;
using System.Runtime.InteropServices;

namespace Fruitless.Components {
    public class Sprite : DependencyComponent, IComparable<Sprite> {
        public static readonly Vector2 AnchorFromCenter =
            new Vector2(0, 0);
        public static readonly Vector2 AnchorFromBottomLeft =
            new Vector2(-0.5f, -0.5f);
        public static readonly Vector2 AnchorFromTopLeft =
            new Vector2(-0.5f, 0.5f);
        public static readonly Vector2 AnchorDefault =
            AnchorFromCenter;

        public event EventHandler<TextureChangedEventArgs> TextureChanged;

        [RequireComponent]
        Transformable2D _transform = null;

        public Sprite() {
            Anchor = AnchorDefault;
            TintColor = Color4.White;
            Layer = 0;
        }

        protected void OnTextureChanged(TextureChangedEventArgs eventArgs) {
            if (TextureChanged != null) {
                TextureChanged(this, eventArgs);
            }
        }

        public Transformable2D Transform {
            get {
                return _transform;
            }
        }

        bool _repeats;

        public bool Repeats {
            get {
                return _repeats;
            }
            set {
                if (_repeats != value) {
                    _repeats = value;

                    IsDirty = true;
                }
            }
        }

        Texture _texture;

        public Texture Texture {
            get {
                return _texture;
            }
            set {
                Texture previousTexture = _texture;

                if (_texture != value) {
                    _texture = value;

                    if (_texture != null) {
                        // default boundaries to texture size
                        Size textureSize = new Size(
                            _texture.Width,
                            _texture.Height);

                        if (_size == Size.Empty) {
                            _size = textureSize;
                        }

                        // default to show the entire texture
                        if (_sourceRectangle == Rectangle.Empty) {
                            _sourceRectangle = new Rectangle(Point.Empty, textureSize);
                        }
                    }

                    IsDirty = true;

                    OnTextureChanged(new TextureChangedEventArgs(previousTexture, _texture));
                }
            }
        }

        Vector2 _anchor;

        /// <summary>
        /// The relative point at which the sprite has its center.
        /// 
        /// Defaults to AnchorFromCenter (0, 0).
        /// </summary>
        public Vector2 Anchor {
            get {
                return _anchor;
            }
            set {
                if (_anchor != value) {
                    _anchor = value;

                    IsDirty = true;
                }
            }
        }

        Color4 _tintColor;

        public Color4 TintColor {
            get {
                return _tintColor;
            }
            set {
                if (_tintColor != value) {
                    _tintColor = value;

                    IsDirty = true;
                }
            }
        }

        Rectangle _sourceRectangle = Rectangle.Empty;

        /// <summary>
        /// A rectangle that specifies to only use a part of the texture.
        /// </summary>
        public Rectangle SourceRectangle {
            get {
                return _sourceRectangle;
            }
            set {
                if (_sourceRectangle != value) {
                    _sourceRectangle = value;

                    IsDirty = true;
                }
            }
        }

        Size _size = Size.Empty;

        /// <summary>
        /// The size of the sprite in pixels.
        /// </summary>
        public Size Size {
            get {
                return _size;
            }
            set {
                if (_size != value) {
                    _size = value;

                    IsDirty = true;
                }
            }
        }

        int _layer;

        /// <summary>
        /// Determines the order that this sprite gets drawn in.
        /// 
        /// Higher is later.
        /// </summary>
        public int Layer {
            get {
                return _layer;
            }
            set {
                if (_layer != value) {
                    _layer = value;

                    IsDirty = true;
                }
            }
        }

        public bool IsDirty {
            get;
            set;
        }

        public int CompareTo(Sprite other) {
            if (Layer < other.Layer) {
                return -1;
            } else if (Layer == other.Layer) {
                return 0;
            }

            return 1;
        }

        public override string ToString() {
            return String.Format("{0}", 
                _texture != null ?  
                    _texture.Filename : 
                    "void");
        }
    }
}
