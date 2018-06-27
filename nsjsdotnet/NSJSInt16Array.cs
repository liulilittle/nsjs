namespace nsjsdotnet
{
    using nsjsdotnet.Core;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public unsafe class NSJSInt16Array : NSJSArray, IEnumerable<short>
    {
        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_localvalue_int16array_new(IntPtr isolate, short* buffer, uint count);

        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static short* nsjs_localvalue_get_int16array(IntPtr localValue, ref int len);

        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static int nsjs_localvalue_int16array_get_length(IntPtr value);

        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static bool nsjs_localvalue_int16array_indexset(IntPtr s, int index, short value);

        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static short nsjs_localvalue_int16array_indexget(IntPtr s, int index);

        public override int Length
        {
            get
            {
                return nsjs_localvalue_int16array_get_length(this.Handle);
            }
        }

        internal NSJSInt16Array(IntPtr handle, NSJSVirtualMachine machine) : base(handle, NSJSDataType.kInt16Array, machine)
        {

        }

        public short[] Buffer
        {
            get
            {
                return (short[])this.GetValue();
            }
        }

        public override object GetValue()
        {
            return this.ValueToArray();
        }

        protected virtual short[] ValueToArray()
        {
            int count = 0;
            void* cch = null;
            try
            {
                cch = nsjs_localvalue_get_int16array(this.Handle, ref count);
                short[] buffer = new short[count];
                if (count > 0)
                {
                    fixed (short* data = buffer)
                    {
                        BufferExtension.memcpy(cch, data, sizeof(short) * count);
                    }
                }
                return buffer;
            }
            finally
            {
                if (cch != null)
                {
                    NSJSMemoryManagement.Free(cch);
                }
            }
        }

        public new short this[int index]
        {
            get
            {
                return nsjs_localvalue_int16array_indexget(this.Handle, index);
            }
            set
            {
                nsjs_localvalue_int16array_indexset(this.Handle, index, value);
            }
        }

        public static NSJSInt16Array New(NSJSVirtualMachine machine, short[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            return New(machine, buffer, 0, buffer.Length);
        }

        public static NSJSInt16Array New(NSJSVirtualMachine machine, short[] buffer, int count)
        {
            return New(machine, buffer, 0, count);
        }

        public static NSJSInt16Array New(NSJSVirtualMachine machine, short[] buffer, int ofs, int count)
        {
            fixed (short* p = &buffer[ofs])
            {
                return New(machine, p, count);
            }
        }

        public static NSJSInt16Array New(NSJSVirtualMachine machine, short* buffer, int count)
        {
            if (machine == null)
            {
                throw new ArgumentNullException("machine");
            }
            if (buffer == null && count > 0)
            {
                throw new ArgumentOutOfRangeException("Parameter count is greater than 0 o'clock, buffer not null is not allowed");
            }
            IntPtr isolate = machine.Isolate;
            if (isolate == NULL)
            {
                throw new InvalidOperationException("machine");
            }
            IntPtr handle = NULL;
            if (buffer == null)
            {
                handle = nsjs_localvalue_int16array_new(isolate, null, 0);
            }
            else
            {
                handle = nsjs_localvalue_int16array_new(isolate, buffer, (uint)count);
            }
            if (handle == NULL)
            {
                throw new InvalidOperationException("machine");
            }
            return new NSJSInt16Array(handle, machine);
        }

        public IEnumerator<short> GetEnumerator()
        {
            return base.GetEnumerator(this.Buffer);
        }
    }
}
