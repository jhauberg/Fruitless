using System;

namespace Fruitless.Utility {
    // this guy!! http://stackoverflow.com/questions/14464/bit-fields-in-c/14591#14591
    internal static class PrimitiveConversion {
        public static long ToLong<T>(T t) where T : struct {
            long r = 0;
            int offset = 0;

            // For every field suitably attributed with a BitfieldLength
            foreach (System.Reflection.FieldInfo f in t.GetType().GetFields()) {
                object[] attrs = f.GetCustomAttributes(typeof(BitfieldLengthAttribute), false);

                if (attrs.Length == 1) {
                    uint fieldLength = ((BitfieldLengthAttribute)attrs[0]).Length;

                    // Calculate a bitmask of the desired length
                    long mask = 0;

                    for (int i = 0; i < fieldLength; i++) {
                        mask |= (uint)(1 << i);
                    }

                    r |= ((UInt32)f.GetValue(t) & mask) << offset;

                    offset += (int)fieldLength;
                }
            }

            return r;
        }
    }
}
