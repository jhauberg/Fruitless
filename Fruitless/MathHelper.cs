using System;

namespace Fruitless {
    public static class MathHelper {
        public const float Pi = (float)Math.PI;
        public const float HalfPi = (float)(Math.PI / 2);

        public static float Lerp(double from, double to, double step) {
            return (float)((to - from) * step + from);
        }
    }
}
