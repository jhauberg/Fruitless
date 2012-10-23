using System;

namespace Fruitless.Components {
    /// <summary>
    /// Indicates that the field can be used to connect variables externally.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)] // Inherited = true or false?
    public sealed class ComponentOutletAttribute : Attribute {
        /*
         * when you have an optional dependency (also note that required dependencies can not be properties!)
        [ComponentOutlet]
        public WhenOutOfBounds OutOfBoundsRule {
            get;
            set;
        }*/
    }
}
