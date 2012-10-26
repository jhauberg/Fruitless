using System;
using System.Runtime.InteropServices;
using Fruitless.Utility;
using System.Collections.Generic;

namespace Fruitless.Components {
    public abstract class RenderComponent : GameComponent, IRenderable, IComparable<RenderComponent> {
        // RenderComponents are ordered by packing DrawableSettings into a long and then using that value to sort with.
        // based on this wonderful article! http://realtimecollisiondetection.net/blog/?p=86
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct DrawableSettings {
            // fields are listed least- to most important. e.g. transparency has a higher impact on order than renderstate.
            [BitfieldLength(32)]
            public uint RenderState;

            [BitfieldLength(22)]
            public uint LayerDepth;
            [BitfieldLength(8)]
            public uint Layer;
            //
            [BitfieldLength(2)]
            public uint IsTransparent;

            public override string ToString() {
                return String.Format("{{ IsTransparent: {0}, Layer: {1}, LayerDepth: {2}, RenderState: {3} }}", 
                    IsTransparent, Layer, LayerDepth, RenderState);
            }
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

        // this is ugleh! but works for now.
        // making new function pointers for each component resulted in bad sorting, so they had to be cached somehow...
        static Dictionary<RenderState, IntPtr> _cachedRenderStates =
            new Dictionary<RenderState, IntPtr>();

        static uint GetSortIndexForState(RenderState state) {
            uint sortIndex = 0;

            if (state != null) {
                if (!_cachedRenderStates.ContainsKey(state)) {
                    _cachedRenderStates[state] = Marshal.GetFunctionPointerForDelegate(state);
                }

                sortIndex = (uint)_cachedRenderStates[state].ToInt32();
            }

            return sortIndex;
        }

        public RenderState RenderState {
            get {
                if (_settings.RenderState != 0) {
                    return (RenderState)Marshal.GetDelegateForFunctionPointer(new IntPtr(_settings.RenderState), typeof(RenderState));
                }

                return null;
            }
            set {
                _settings.RenderState = (uint)GetSortIndexForState(value);

                Settings = _settings;
            }
        }

        /// <summary>
        /// An index specifying order within the current layer.
        /// Higher indices get rendered last.
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
        /// An index specifying which layer this component is on. 
        /// Higher layers get rendered last.
        /// 0-128.
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

                SortingKey = PrimitiveConversion
                    .ToLong<DrawableSettings>(_settings);

                //System.Diagnostics.Debug.WriteLine(String.Format("{0} = {1}", _settings.ToString(), SortingKey));
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
