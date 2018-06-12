namespace nsjsdotnet
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public unsafe class NSJSUInt16Array : NSJSArray, IEnumerable<ushort>
    {
        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_localvalue_uint16array_new(IntPtr isolate, ushort* buffer, uint count);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static void* memcpy(void* dest, void* src, uint count);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static ushort* nsjs_localvalue_get_uint16array(IntPtr localValue, ref int len);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int nsjs_localvalue_uint16array_get_length(IntPtr value);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static bool nsjs_localvalue_uint16array_indexset(IntPtr s, int index, ushort value);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static ushort nsjs_localvalue_uint16array_indexget(IntPtr s, int index);

        public override int Length
        {
            get
            {
                return nsjs_localvalue_uint16array_get_length(this.Handle);
            }
        }

        internal NSJSUInt16Array(IntPtr handle, NSJSVirtualMachine machine) : base(handle, NSJSValueType.kUInt16Array, machine)
        {

        }

        public ushort[] Buffer
        {
            get
            {
                return (ushort[])this.GetValue();
            }
        }

        public override object GetValue()
        {
            return this.ValueToArray();
        }

        protected virtual ushort[] ValueToArray()
        {
            int count = 0;
            void* cch = null;
            try
            {
                cch = nsjs_localvalue_get_uint16array(this.Handle, ref count);
                ushort[] buf = new ushort[count];
                if (count > 0)
                {
                    fixed (ushort* pinned = buf)
                    {
                        memcpy(pinned, cch, (uint)(sizeof(ushort) * count));
                    }
                }
                return buf;
            }
            finally
            {
                if (cch != null)
                {
                    NSJSMemoryManagement.Free(cch);
                }
            }
        }

        public new ushort this[int index]
        {
            get
            {
                return nsjs_localvalue_uint16array_indexget(this.Handle, index);
            }
            set
            {
                nsjs_localvalue_uint16array_indexset(this.Handle, index, value);
            }
        }

        public static NSJSUInt16Array New(NSJSVirtualMachine machine, ushort[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            return New(machine, buffer, 0, buffer.Length);
        }

        public static NSJSUInt16Array New(NSJSVirtualMachine machine, ushort[] buffer, int count)
        {
            return New(machine, buffer, 0, count);
        }

        public static NSJSUInt16Array New(NSJSVirtualMachine machine, ushort[] buffer, int ofs, int count)
        {
            if (buffer.Length == ofs)
            {
                return New(machine, (ushort*)null, count);
            }
            fixed (ushort* p = &buffer[ofs])
            {
                return New(machine, p, count);
            }
        }

        public static NSJSUInt16Array New(NSJSVirtualMachine machine, ushort* buffer, int count)
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
                handle = nsjs_localvalue_uint16array_new(isolate, null, 0);
            }
            else
            {
                handle = nsjs_localvalue_uint16array_new(isolate, buffer, (uint)count);
            }
            if (handle == NULL)
            {
                throw new InvalidOperationException("machine");
            }
            return new NSJSUInt16Array(handle, machine);
        }

        public IEnumerator<ushort> GetEnumerator()
        {
            return base.GetEnumerator(this.Buffer);
        }
    }
}
