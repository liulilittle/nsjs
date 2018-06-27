namespace nsjsdotnet
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public class NSJSInt64 : NSJSValue
    {
        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static long nsjs_localvalue_get_int64(IntPtr localValue);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private object value;

        public override object GetValue()
        {
            lock (this)
            {
                if (this.value == null)
                {
                    this.value = nsjs_localvalue_get_int64(this.Handle);
                }
                return this.value;
            }
        }

        internal NSJSInt64(IntPtr handle, NSJSVirtualMachine machine) : base(handle, NSJSDataType.kInt64, machine)
        {

        }

        public long Value
        {
            get
            {
                return (long)this.GetValue();
            }
        }

        public override string ToString()
        {
            return this.Value.ToString();
        }
    }
}
