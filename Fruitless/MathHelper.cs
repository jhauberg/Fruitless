using OpenTK;
using System;

namespace Fruitless {
    public static class MathHelper {
        public const float Pi = (float)Math.PI;
        public const float HalfPi = (float)(Math.PI / 2);

        public static float Lerp(float from, float to, float step) {
            return (to - from) * step + from;
        }

        public static float SmoothStep(float min, float max, float step) {
            float r = (step - min) / (max - min);

            return r * r * (3.0f - 2.0f * r);  
        }
    }

    public static class FloatExtensions {
        public const Single Precision = 0.00001f;

        public static bool IsAlmostEqualTo(this Single value, Single other) {
            return (Single)Math.Abs(other - value) < Precision;
        }
    }
}
