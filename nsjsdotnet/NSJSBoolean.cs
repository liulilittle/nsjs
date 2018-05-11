namespace nsjsdotnet
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public class NSJSBoolean : NSJSValue
    {
        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_localvalue_boolean_new(IntPtr isolate, bool value);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private extern static bool nsjs_localvalue_get_boolean(IntPtr localValue);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private object value;

        public override object GetValue()
        {
            lock (this)
            {
                if (this.value == null)
                {
                    this.value = nsjs_localvalue_get_boolean(this.Handle);
                }
                return this.value;
            }
        }

        internal NSJSBoolean(IntPtr handle, NSJSVirtualMachine machine) : base(handle, NSJSValueType.kBoolean, machine)
        {

        }

        public bool Value
        {
            get
            {
                return (bool)this.GetValue();
            }
        }

        public static NSJSBoolean New(NSJSVirtualMachine machine, bool value)
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
            IntPtr handle = nsjs_localvalue_boolean_new(isolate, value);
            if (handle == NULL)
            {
                throw new InvalidOperationException("machine");
            }
            return new NSJSBoolean(handle, machine);
        }

        public override string ToString()
        {
            return this.Value.ToString();
        }
    }
}
