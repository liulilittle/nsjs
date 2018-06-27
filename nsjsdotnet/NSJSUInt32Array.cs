namespace nsjsdotnet
{
    using nsjsdotnet.Core;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public unsafe class NSJSUInt32Array : NSJSArray, IEnumerable<uint>
    {
        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_localvalue_uint32array_new(IntPtr isolate, uint* buffer, uint count);

        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static uint* nsjs_localvalue_get_uint32array(IntPtr localValue, ref int len);

        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static int nsjs_localvalue_uint32array_get_length(IntPtr value);

        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static bool nsjs_localvalue_uint32array_indexset(IntPtr s, int index, uint value);

        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static uint nsjs_localvalue_uint32array_indexget(IntPtr s, int index);

        public override int Length
        {
            get
            {
                return nsjs_localvalue_uint32array_get_length(this.Handle);
            }
        }

        internal NSJSUInt32Array(IntPtr handle, NSJSVirtualMachine machine) : base(handle, NSJSDataType.kUInt32Array, machine)
        {

        }

        public uint[] Buffer
        {
            get
            {
                return (uint[])this.GetValue();
            }
        }

        public override object GetValue()
        {
            return this.ValueToArray();
        }

        protected virtual uint[] ValueToArray()
        {
            int count = 0;
            void* cch = null;
            try
            {
                cch = nsjs_localvalue_get_uint32array(this.Handle, ref count);
                uint[] buffer = new uint[count];
                if (count > 0)
                {
                    fixed (uint* data = buffer)
                    {
                        BufferExtension.memcpy(cch, data, sizeof(uint) * count);
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

        public new uint this[int index]
        {
            get
            {
                return nsjs_localvalue_uint32array_indexget(this.Handle, index);
            }
            set
            {
                nsjs_localvalue_uint32array_indexset(this.Handle, index, value);
            }
        }

        public static NSJSUInt32Array New(NSJSVirtualMachine machine, uint[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            return New(machine, buffer, 0, buffer.Length);
        }

        public static NSJSUInt32Array New(NSJSVirtualMachine machine, uint[] buffer, int count)
        {
            return New(machine, buffer, 0, count);
        }

        public static NSJSUInt32Array New(NSJSVirtualMachine machine, uint[] buffer, int ofs, int count)
        {
            if (buffer.Length == ofs)
            {
                return New(machine, (uint*)null, count);
            }
            fixed (uint* p = &buffer[ofs])
            {
                return New(machine, p, count);
            }
        }

        public static NSJSUInt32Array New(NSJSVirtualMachine machine, uint* buffer, int count)
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
                handle = nsjs_localvalue_uint32array_new(isolate, null, 0);
            }
            else
            {
                handle = nsjs_localvalue_uint32array_new(isolate, buffer, (uint)count);
            }
            if (handle == NULL)
            {
                throw new InvalidOperationException("machine");
            }
            return new NSJSUInt32Array(handle, machine);
        }

        public IEnumerator<uint> GetEnumerator()
        {
            return base.GetEnumerator(this.Buffer);
        }
    }
}
