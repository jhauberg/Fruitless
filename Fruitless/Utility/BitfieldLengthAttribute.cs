using System;

namespace Fruitless.Utility {
    [global::System.AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal sealed class BitfieldLengthAttribute : Attribute {
        public BitfieldLengthAttribute(uint length) {
            Length = length;
        }

        public uint Length {
            get;
            private set;
        }
    }
}
