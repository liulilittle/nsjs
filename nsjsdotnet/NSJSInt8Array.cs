namespace nsjsdotnet
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public unsafe class NSJSInt8Array : NSJSArray, IEnumerable<sbyte>
    {
        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_localvalue_int8array_new(IntPtr isolate, sbyte* buffer, uint count);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static void* memcpy(void* dest, void* src, uint count);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static sbyte* nsjs_localvalue_get_int8array(IntPtr localValue, ref int len);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int nsjs_localvalue_int8array_get_length(IntPtr value);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static bool nsjs_localvalue_int8array_indexset(IntPtr s, int index, sbyte value);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static sbyte nsjs_localvalue_int8array_indexget(IntPtr s, int index);

        public override int Length
        {
            get
            {
                return nsjs_localvalue_int8array_get_length(this.Handle);
            }
        }

        internal NSJSInt8Array(IntPtr handle, NSJSVirtualMachine machine) : base(handle, NSJSValueType.kInt8Array, machine)
        {

        }

        public sbyte[] Buffer
        {
            get
            {
                return (sbyte[])this.GetValue();
            }
        }

        public override object GetValue()
        {
            return LocalValueToArray();
        }

        protected virtual sbyte[] LocalValueToArray()
        {
            int count = 0;
            void* cch = null;
            try
            {
                cch = nsjs_localvalue_get_int8array(this.Handle, ref count);
                sbyte[] buf = new sbyte[count];
                if (count > 0)
                {
                    fixed (sbyte* pinned = buf)
                    {
                        memcpy(pinned, cch, (uint)(sizeof(sbyte) * count));
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

        public new sbyte this[int index]
        {
            get
            {
                return nsjs_localvalue_int8array_indexget(this.Handle, index);
            }
            set
            {
                nsjs_localvalue_int8array_indexset(base.Handle, index, value);
            }
        }

        public static NSJSInt8Array New(NSJSVirtualMachine machine, sbyte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            return New(machine, buffer, 0, buffer.Length);
        }

        public static NSJSInt8Array New(NSJSVirtualMachine machine, sbyte[] buffer, int count)
        {
            return New(machine, buffer, 0, count);
        }

        public static NSJSInt8Array New(NSJSVirtualMachine machine, sbyte[] buffer, int ofs, int count)
        {
            if (buffer.Length == ofs)
            {
                return New(machine, (sbyte*)null, count);
            }
            fixed (sbyte* p = &buffer[ofs])
            {
                return New(machine, p, count);
            }
        }

        public static NSJSInt8Array New(NSJSVirtualMachine machine, sbyte* buffer, int count)
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
                handle = nsjs_localvalue_int8array_new(isolate, null, 0);
            }
            else
            {
                handle = nsjs_localvalue_int8array_new(isolate, buffer, (uint)count);
            }
            if (handle == NULL)
            {
                throw new InvalidOperationException("machine");
            }
            return new NSJSInt8Array(handle, machine);
        }

        public IEnumerator<sbyte> GetEnumerator()
        {
            return base.GetEnumerator(this.Buffer);
        }
    }
}
