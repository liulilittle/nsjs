namespace nsjsdotnet
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public unsafe class NSJSFloat64Array : NSJSArray, IEnumerable<double>
    {
        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static double* nsjs_localvalue_get_float64array(IntPtr localValue, ref int len);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static void* memcpy(void* dest, void* src, uint count);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_localvalue_float64array_new(IntPtr isolate, double* buffer, uint count);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int nsjs_localvalue_float64array_get_length(IntPtr value);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static bool nsjs_localvalue_float64array_indexset(IntPtr s, int index, double value);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static double nsjs_localvalue_float64array_indexget(IntPtr s, int index);

        public override int Length
        {
            get
            {
                return nsjs_localvalue_float64array_get_length(this.Handle);
            }
        }

        internal NSJSFloat64Array(IntPtr handle, NSJSVirtualMachine machine) : base(handle, NSJSValueType.kFloat64Array, machine)
        {

        }

        public double[] Buffer
        {
            get
            {
                return (double[])this.GetValue();
            }
        }

        public override object GetValue()
        {
            return LocalValueToArray();
        }

        protected virtual double[] LocalValueToArray()
        {
            int count = 0;
            void* cch = null;
            try
            {
                cch = nsjs_localvalue_get_float64array(this.Handle, ref count);
                double[] buf = new double[count];
                if (count > 0)
                {
                    fixed (double* pinned = buf)
                    {
                        memcpy(pinned, cch, (uint)(sizeof(double) * count));
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

        public new double this[int index]
        {
            get
            {
                return nsjs_localvalue_float64array_indexget(this.Handle, index);
            }
            set
            {
                nsjs_localvalue_float64array_indexset(this.Handle, index, value);
            }
        }

        public static NSJSFloat64Array New(NSJSVirtualMachine machine, double[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            return New(machine, buffer, 0, buffer.Length);
        }

        public static NSJSFloat64Array New(NSJSVirtualMachine machine, double[] buffer, int count)
        {
            return New(machine, buffer, 0, count);
        }

        public static NSJSFloat64Array New(NSJSVirtualMachine machine, double[] buffer, int ofs, int count)
        {
            if (buffer.Length == ofs)
            {
                return New(machine, (double*)null, count);
            }
            fixed (double* p = &buffer[ofs])
            {
                return New(machine, p, count);
            }
        }

        public static NSJSFloat64Array New(NSJSVirtualMachine machine, double* buffer, int count)
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
                handle = nsjs_localvalue_float64array_new(isolate, null, 0);
            }
            else
            {
                handle = nsjs_localvalue_float64array_new(isolate, buffer, (uint)count);
            }
            if (handle == NULL)
            {
                throw new InvalidOperationException("machine");
            }
            return new NSJSFloat64Array(handle, machine);
        }

        public IEnumerator<double> GetEnumerator()
        {
            return base.GetEnumerator(this.Buffer);
        }
    }
}
