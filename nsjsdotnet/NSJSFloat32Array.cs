﻿namespace nsjsdotnet
{
    using nsjsdotnet.Core;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public unsafe class NSJSFloat32Array : NSJSArray, IEnumerable<float>
    {
        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_localvalue_float32array_new(IntPtr isolate, float* buffer, uint count);

        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static float* nsjs_localvalue_get_float32array(IntPtr localValue, ref int len);

        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static int nsjs_localvalue_float32array_get_length(IntPtr value);

        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static bool nsjs_localvalue_float32array_indexset(IntPtr s, int index, float value);

        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static float nsjs_localvalue_float32array_indexget(IntPtr s, int index);

        public override int Length
        {
            get
            {
                return nsjs_localvalue_float32array_get_length(this.Handle);
            }
        }

        internal NSJSFloat32Array(IntPtr handle, NSJSVirtualMachine machine) : base(handle, NSJSDataType.kFloat32Array, machine)
        {

        }

        public static new NSJSFloat32Array Cast(NSJSValue value)
        {
            return Cast(value, (handle, machine) => new NSJSFloat32Array(value.Handle, value.VirtualMachine));
        }

        public float[] Buffer
        {
            get
            {
                return (float[])this.GetValue();
            }
        }

        public override object GetValue()
        {
            return this.ValueToArray();
        }

        protected virtual float[] ValueToArray()
        {
            int count = 0;
            void* cch = null;
            try
            {
                cch = nsjs_localvalue_get_float32array(this.Handle, ref count);
                float[] buffer = new float[count];
                if (count > 0)
                {
                    fixed (float* data = buffer)
                    {
                        BufferExtension.memcpy(cch, data, sizeof(float) * count);
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

        public new float this[int index]
        {
            get
            {
                return nsjs_localvalue_float32array_indexget(this.Handle, index);
            }
            set
            {
                nsjs_localvalue_float32array_indexset(this.Handle, index, value);
            }
        }

        public static NSJSFloat32Array New(NSJSVirtualMachine machine, float[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            return New(machine, buffer, 0, buffer.Length);
        }

        public static NSJSFloat32Array New(NSJSVirtualMachine machine, float[] buffer, int count)
        {
            return New(machine, buffer, 0, count);
        }

        public static NSJSFloat32Array New(NSJSVirtualMachine machine, float[] buffer, int ofs, int count)
        {
            if (buffer.Length == ofs)
            {
                return New(machine, (float*)null, count);
            }
            fixed (float* p = &buffer[ofs])
            {
                return New(machine, p, count);
            }
        }

        public static NSJSFloat32Array New(NSJSVirtualMachine machine, float* buffer, int count)
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
                handle = nsjs_localvalue_float32array_new(isolate, null, 0);
            }
            else
            {
                handle = nsjs_localvalue_float32array_new(isolate, buffer, (uint)count);
            }
            if (handle == NULL)
            {
                throw new InvalidOperationException("machine");
            }
            return new NSJSFloat32Array(handle, machine);
        }

        public IEnumerator<float> GetEnumerator()
        {
            return base.GetEnumerator(this.Buffer);
        }
    }
}
