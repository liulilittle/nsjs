namespace nsjsdotnet.Core.Utilits
{
    using nsjsdotnet.Core.Diagnostics;
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ModelConverterAttribute : Attribute
    {
        public enum ConversionMode
        {
            List,
            Object
        }

        public Type Conversion
        {
            get;
            private set;
        }

        public ConversionMode Mode
        {
            get;
            private set;
        }

        public ModelConverterAttribute(Type conversion, ConversionMode mode)
        {
            Contract.Requires<ArgumentNullException>(conversion != null);

            if (mode != ConversionMode.List && mode != ConversionMode.Object)
            {
                throw new NotSupportedException("mode");
            }
            this.Mode = mode;
            this.Conversion = conversion;
        }
    }
}
