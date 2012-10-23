using System;
using System.Drawing;
using ComponentKit.Model;
using OpenTK;
using OpenTK.Graphics;

namespace Fruitless.Components {
    public class Sprite : GameComponent, IComparable<Sprite> {
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
                        if (_bounds == Size.Empty) {
                            _bounds = new Size(
                                _texture.Width, 
                                _texture.Height);
                        }

                        // default to show the entire texture
                        if (_frame == Rectangle.Empty) {
                            _frame = new Rectangle(Point.Empty, _bounds);
                        }
                    }

                    OnTextureChanged(new TextureChangedEventArgs(previousTexture, _texture));
                }
            }
        }

        Vector2 _anchor;

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

        Rectangle _frame = Rectangle.Empty;

        public Rectangle Frame {
            get {
                return _frame;
            }
            set {
                if (_frame != value) {
                    _frame = value;

                    IsDirty = true;
                }
            }
        }

        Size _bounds = Size.Empty;

        public Size Bounds {
            get {
                return _bounds;
            }
            set {
                if (_bounds != value) {
                    _bounds = value;

                    IsDirty = true;
                }
            }
        }

        int _layer;

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
            private set;
        }

        public int CompareTo(Sprite other) {
            return
                Layer != other.Layer ?
                    Layer < other.Layer ?
                        -1 : 1 :
                    0;
        }
    }
}
