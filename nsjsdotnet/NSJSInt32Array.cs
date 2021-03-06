﻿namespace nsjsdotnet
{
    using nsjsdotnet.Core;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public unsafe class NSJSInt32Array : NSJSArray, IEnumerable<int>
    {
        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_localvalue_int32array_new(IntPtr isolate, int* buffer, uint count);

        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static int* nsjs_localvalue_get_int32array(IntPtr localValue, ref int len);

        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static int nsjs_localvalue_int32array_get_length(IntPtr value);

        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static bool nsjs_localvalue_int32array_indexset(IntPtr s, int index, int value);

        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static int nsjs_localvalue_int32array_indexget(IntPtr s, int index);

        public override int Length
        {
            get
            {
                return nsjs_localvalue_int32array_get_length(this.Handle);
            }
        }

        internal NSJSInt32Array(IntPtr handle, NSJSVirtualMachine machine) : base(handle, NSJSDataType.kInt32Array, machine)
        {

        }

        public static new NSJSInt32Array Cast(NSJSValue value)
        {
            return Cast(value, (handle, machine) => new NSJSInt32Array(value.Handle, value.VirtualMachine));
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
            return this.ValueToArray();
        }

        protected virtual int[] ValueToArray()
        {
            int count = 0;
            void* cch = null;
            try
            {
                cch = nsjs_localvalue_get_int32array(this.Handle, ref count);
                int[] buffer = new int[count];
                if (count > 0)
                {
                    fixed (int* data = buffer)
                    {
                        BufferExtension.memcpy(cch, data, sizeof(int) * count);
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
