namespace nsjsdotnet
{
    using System;
    using System.Runtime.InteropServices;
    using System.Collections.Generic;

    public unsafe class NSJSInt32Array : NSJSArray, IEnumerable<int>
    {
        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_localvalue_int32array_new(IntPtr isolate, int* buffer, uint count);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static void* memcpy(void* dest, void* src, uint count);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int* nsjs_localvalue_get_int32array(IntPtr localValue, ref int len);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int nsjs_localvalue_int32array_get_length(IntPtr value);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static bool nsjs_localvalue_int32array_indexset(IntPtr s, int index, int value);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int nsjs_localvalue_int32array_indexget(IntPtr s, int index);

        public override int Length
        {
            get
            {
                return nsjs_localvalue_int32array_get_length(this.Handle);
            }
        }

        internal NSJSInt32Array(IntPtr handle, NSJSVirtualMachine machine) : base(handle, NSJSValueType.kInt32Array, machine)
        {

        }

        public int[] Buffer
        {
            get
            {
                return (int[])this.GetValue();
            }
        }

        public override object GetValue()
        {
            return LocalValueToArray();
        }

        protected virtual int[] LocalValueToArray()
        {
            int count = 0;
            void* cch = null;
            try
            {
                cch = nsjs_localvalue_get_int32array(this.Handle, ref count);
                int[] buf = new int[count];
                if (count > 0)
                {
                    fixed (int* pinned = buf)
                    {
                        memcpy(pinned, cch, (uint)(sizeof(int) * count));
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

        public new int this[int index]
        {
            get
            {
                return nsjs_localvalue_int32array_indexget(this.Handle, index);
            }
            set
            {
                nsjs_localvalue_int32array_indexset(this.Handle, index, value);
            }
        }

        public static NSJSInt32Array New(NSJSVirtualMachine machine, int[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            return New(machine, buffer, 0, buffer.Length);
        }

        public static NSJSInt32Array New(NSJSVirtualMachine machine, int[] buffer, int count)
        {
            return New(machine, buffer, 0, count);
        }

        public static NSJSInt32Array New(NSJSVirtualMachine machine, int[] buffer, int ofs, int count)
        {
            if (buffer.Length == ofs)
            {
                return New(machine, (int*)null, count);
            }
            fixed (int* p = &buffer[ofs])
            {
                return New(machine, p, count);
            }
        }

        public static NSJSInt32Array New(NSJSVirtualMachine machine, int* buffer, int count)
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
                handle = nsjs_localvalue_int32array_new(isolate, null, 0);
            }
            else
            {
                handle = nsjs_localvalue_int32array_new(isolate, buffer, (uint)count);
            }
            if (handle == NULL)
            {
                throw new InvalidOperationException("machine");
            }
            return new NSJSInt32Array(handle, machine);
        }

        public IEnumerator<int> GetEnumerator()
        {
            return base.GetEnumerator(this.Buffer);
        }
    }
}
