namespace nsjsdotnet
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public class NSJSUInt32 : NSJSValue
    {
        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_localvalue_uint32_new(IntPtr isolate, uint value);

        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static uint nsjs_localvalue_get_uint32(IntPtr localValue);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private object value;

        public override object GetValue()
        {
            lock (this)
            {
                if (this.value == null)
                {
                    this.value = nsjs_localvalue_get_uint32(this.Handle);
                }
                return this.value;
            }
        }

        internal NSJSUInt32(IntPtr handle, NSJSVirtualMachine machine) : base(handle, NSJSDataType.kUInt32, machine)
        {

        }

        public uint Value
        {
            get
            {
                return (uint)this.GetValue();
            }
        }

        public static NSJSUInt32 New(NSJSVirtualMachine machine, uint value)
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
            IntPtr handle = nsjs_localvalue_uint32_new(isolate, value);
            if (handle == NULL)
            {
                throw new InvalidOperationException("machine");
            }
            return new NSJSUInt32(handle, machine);
        }
    }
}
