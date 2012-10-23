using System;
using System.Runtime.InteropServices;
using Fruitless.Utility;

namespace Fruitless.Components {
    public abstract class RenderComponent : GameComponent, IRenderable, IComparable<RenderComponent> {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct DrawableSettings {
            [BitfieldLength(32)]
            public uint RenderState;

            [BitfieldLength(24)]
            public uint LayerDepth;
            [BitfieldLength(7)]
            public uint Layer;
            [BitfieldLength(1)]
            public uint IsTransparent;
        }

        DrawableSettings _settings;

        public virtual void Render(ICamera camera) { }

        public int CompareTo(RenderComponent other) {
            return SortingKey.CompareTo(other.SortingKey);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public override bool Equals(object obj) {
            return base.Equals(obj);
        }

        public static bool operator ==(RenderComponent left, RenderComponent right) {
            return left.Equals(right);
        }

        public static bool operator !=(RenderComponent left, RenderComponent right) {
            return !left.Equals(right);
        }

        public RenderState RenderState {
            get {
                if (_settings.RenderState != 0) {
                    return (RenderState)Marshal.GetDelegateForFunctionPointer(new IntPtr(_settings.RenderState), typeof(RenderState));
                }

                return null;
            }
            set {
                if (value != null) {
                    _settings.RenderState = (uint)Marshal.GetFunctionPointerForDelegate(value).ToInt32();

                    Settings = _settings;
                } else {
                    _settings.RenderState = 0;
                }
            }
        }

        /// <summary>
        /// 0-PLENTY.
        /// </summary>
        public uint LayerDepth {
            get {
                return _settings.LayerDepth;
            }
            set {
                _settings.LayerDepth = value;

                Settings = _settings;
            }
        }

        /// <summary>
        /// 0-128
        /// </summary>
        public uint Layer {
            get {
                return _settings.Layer;
            }
            set {
                _settings.Layer = value;

                Settings = _settings;
            }
        }

        public bool IsTransparent {
            get {
                return _settings.IsTransparent != 0;
            }
            set {
                _settings.IsTransparent =
                    value ?
                        (uint)1 :
                        (uint)0;

                Settings = _settings;
            }
        }

        private DrawableSettings Settings {
            get {
                return _settings;
            }
            set {
                _settings = value;

                SortingKey = PrimitiveConversion.ToLong<DrawableSettings>(_settings);
            }
        }

        private long SortingKey {
            get;
            set;
        }

        public bool IsHidden {
            get;
            set;
        }
    }
}
