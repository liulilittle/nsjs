namespace nsjsdotnet
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public class NSJSBoolean : NSJSValue
    {
        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_localvalue_boolean_new(IntPtr isolate, bool value);

        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
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

        internal NSJSBoolean(IntPtr handle, NSJSVirtualMachine machine) : base(handle, NSJSDataType.kBoolean, machine)
        {

        }

        public static NSJSBoolean Cast(NSJSValue value)
        {
            return Cast(value, (handle, machine) => new NSJSBoolean(value.Handle, value.VirtualMachine));
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

        public static implicit operator bool(NSJSBoolean x)
        {
            if (x == null)
            {
                return false;
            }
            return x.Value;
        }
    }
}
