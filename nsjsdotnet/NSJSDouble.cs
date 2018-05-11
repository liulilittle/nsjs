namespace nsjsdotnet
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public class NSJSDouble : NSJSValue
    {
        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_localvalue_float64_new(IntPtr isolate, double value);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static double nsjs_localvalue_get_float64(IntPtr localValue);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private object value;

        public override object GetValue()
        {
            lock (this)
            {
                if (this.value == null)
                {
                    this.value = nsjs_localvalue_get_float64(this.Handle);
                }
            }
            return this.value;
        }

        internal NSJSDouble(IntPtr handle, NSJSVirtualMachine machine) : base(handle, NSJSValueType.kDouble, machine)
        {

        }

        public double Value
        {
            get
            {
                return (double)this.GetValue();
            }
        }

        public static NSJSDouble New(NSJSVirtualMachine machine, double value)
        {
            if (machine == null)
            {
                throw new ArgumentNullException("machine");
            }
            IntPtr isolate = machine.Isolate;
            if (isolate == NULL)
            {
                throw new InvalidOperationException("machine");
            }
            IntPtr handle = nsjs_localvalue_float64_new(isolate, value);
            if (handle == NULL)
            {
                throw new InvalidOperationException("machine");
            }
            return new NSJSDouble(handle, machine);
        }

        public override string ToString()
        {
            return this.Value.ToString();
        }
    }
}
