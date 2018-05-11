namespace nsjsdotnet
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public class NSJSInt32 : NSJSValue
    {
        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_localvalue_int32_new(IntPtr isolate, int value);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int nsjs_localvalue_get_int32(IntPtr localValue);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private object value;

        public override object GetValue()
        {
            lock (this)
            {
                if (this.value == null)
                {
                    this.value = nsjs_localvalue_get_int32(this.Handle);
                }
                return this.value;
            }
        }

        internal NSJSInt32(IntPtr handle, NSJSVirtualMachine machine) : base(handle, NSJSValueType.kInt32, machine)
        {

        }

        public int Value
        {
            get
            {
                return (int)this.GetValue();
            }
        }

        public static NSJSInt32 New(NSJSVirtualMachine machine, int value)
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
            IntPtr handle = nsjs_localvalue_int32_new(isolate, value);
            if (handle == NULL)
            {
                throw new InvalidOperationException("machine");
            }
            return new NSJSInt32(handle, machine);
        }

        public override string ToString()
        {
            return this.Value.ToString();
        }
    }
}
