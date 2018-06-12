namespace nsjsdotnet
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public unsafe class NSJSUInt8Array : NSJSArray, IEnumerable<byte>
    {
        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_localvalue_uint8array_new(IntPtr isolate, byte* buffer, uint count);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static void* memcpy(void* dest, void* src, uint count);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static byte* nsjs_localvalue_get_uint8array(IntPtr localValue, ref int len);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int nsjs_localvalue_uint8array_get_length(IntPtr value);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static bool nsjs_localvalue_uint8array_indexset(IntPtr s, int index, byte value);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static byte nsjs_localvalue_uint8array_indexget(IntPtr s, int index);

        public override int Length
        {
            get
            {
                return nsjs_localvalue_uint8array_get_length(this.Handle);
            }
        }

        internal NSJSUInt8Array(IntPtr handle, NSJSVirtualMachine machine) : base(handle, NSJSValueType.kUInt8Array, machine)
        {

        }

        public byte[] Buffer
        {
            get
            {
                return (byte[])this.GetValue();
            }
        }

        public override object GetValue()
        {
            return this.ValueToArray();
        }

        protected virtual byte[] ValueToArray()
        {
            int count = 0;
            void* cch = null;
            try
            {
                cch = nsjs_localvalue_get_uint8array(this.Handle, ref count);
                byte[] buf = new byte[count];
                if (count > 0)
                {
                    fixed (byte* pinned = buf)
                    {
                        memcpy(pinned, cch, (uint)(sizeof(byte) * count));
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

        public new byte this[int index]
        {
            get
            {
                return nsjs_localvalue_uint8array_indexget(this.Handle, index);
            }
            set
            {
                nsjs_localvalue_uint8array_indexset(this.Handle, index, value); 
            }
        }

        public static NSJSUInt8Array New(NSJSVirtualMachine machine, byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            return New(machine, buffer, 0, buffer.Length);
        }

        public static NSJSUInt8Array New(NSJSVirtualMachine machine, byte[] buffer, int count)
        {
            return New(machine, buffer, 0, count);
        }

        public static NSJSUInt8Array New(NSJSVirtualMachine machine, byte[] buffer, int ofs, int count)
        {
            if (buffer.Length == ofs)
            {
                return New(machine, (byte*)null, count);
            }
            fixed (byte* p = &buffer[ofs])
            {
                return New(machine, p, count);
            }
        }

        public static NSJSUInt8Array New(NSJSVirtualMachine machine, byte* buffer, int count)
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
                handle = nsjs_localvalue_uint8array_new(isolate, null, 0);
            }
            else
            {
                handle = nsjs_localvalue_uint8array_new(isolate, buffer, (uint)count);
            }
            if (handle == NULL)
            {
                throw new InvalidOperationException("machine");
            }
            return new NSJSUInt8Array(handle, machine);
        }

        public IEnumerator<byte> GetEnumerator()
        {
            return base.GetEnumerator(this.Buffer);
        }
    }
}
