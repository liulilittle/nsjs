namespace nsjsdotnet
{
    using System;
    using System.Runtime.InteropServices;
    using System.Collections.Generic;
    using System.Collections;

    public unsafe class NSJSArray : NSJSObject, IEnumerable<NSJSValue>
    {
        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static int nsjs_localvalue_array_get_length(IntPtr obj);

        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_localvalue_array_indexget(IntPtr obj, uint index);

        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static bool nsjs_localvalue_array_indexset(IntPtr obj, uint index, IntPtr value);

        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_localvalue_array_new(IntPtr isolate, int length);

        internal NSJSArray(IntPtr handle, NSJSDataType datatype, NSJSVirtualMachine machine) : base(handle, datatype, machine)
        {

        }

        internal NSJSArray(IntPtr handle, NSJSVirtualMachine machine) : base(handle, NSJSDataType.kArray, machine)
        {
           
        }

        public virtual int Length
        {
            get
            {
                return nsjs_localvalue_array_get_length(this.Handle);
            }
        }

        public static new NSJSArray New(NSJSVirtualMachine machine, int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length cannot be less than 0");
            }
            if (machine == null)
            {
                throw new ArgumentNullException("machine");
            }
            IntPtr isolate = machine.Isolate;
            if (isolate == NULL)
            {
                throw new InvalidOperationException("machine");
            }
            IntPtr handle = nsjs_localvalue_array_new(isolate, length);
            if (handle == NULL)
            {
                throw new InvalidOperationException("machine");
            }
            return new NSJSArray(handle, machine);
        }

        public static new NSJSArray Cast(NSJSValue value)
        {
            return Cast(value, (handle, machine) => new NSJSArray(handle, machine));
        }

        public virtual NSJSValue this[int index]
        {
            get
            {
                IntPtr handle = nsjs_localvalue_array_indexget(this.Handle, (uint)index);
                if (handle == null)
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                return NSJSValueBuilder.From(handle, this.VirtualMachine);
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (value.Handle == NULL)
                {
                    throw new ArgumentException("value");
                }
                nsjs_localvalue_array_indexset(this.Handle, (uint)index, value.Handle);
            }
        }

        public virtual NSJSValue RemoveAt(int index)
        {
            if (index < 0 || index >= this.Length)
            {
                throw new ArgumentOutOfRangeException("value");
            }
            NSJSFunction splice = (NSJSFunction)base.Get("splice");
            return splice.Call(NSJSInt32.New(this.VirtualMachine, index));
        }

        public virtual NSJSValue Remove(NSJSValue value)
        {
            int index = this.IndexOf(value);
            if (index < 0)
            {
                return null;
            }
            return this.RemoveAt(index);
        }

        public virtual int IndexOf(NSJSValue value)
        {
            if (value == null)
            {
                return -1;
            }
            NSJSFunction indexOf = (NSJSFunction)base.Get("indexOf");
            NSJSInt32 result = indexOf.Call(value) as NSJSInt32;
            if (result == null)
            {
                return -1;
            }
            return result.Value;
        }

        public virtual NSJSValue Clear()
        {
            NSJSFunction splice = (NSJSFunction)base.Get("splice");
            return splice.Call(NSJSInt32.New(this.VirtualMachine, 0), NSJSInt32.New(this.VirtualMachine, this.Length));
        }

        public virtual void Add(NSJSValue value)
        {
            if (value == null)
            {
                value = NSJSValue.Null(this.VirtualMachine);
            }
            NSJSFunction push = (NSJSFunction)base.Get("push");
            push.Call(value);
        }

        public void Add(bool value)
        {
            if (this.IntegerBoolean)
            {
                this.Add(value ? 1 : 0);
            }
            else
            {
                this.Add((NSJSValue)NSJSBoolean.New(this.VirtualMachine, value));
            }
        }

        public void Add(int value)
        {
            this.Add((NSJSValue)NSJSInt32.New(this.VirtualMachine, value));
        }

        public void Add(double value)
        {
            this.Add((NSJSValue)NSJSDouble.New(this.VirtualMachine, value));
        }

        public void Add(uint value)
        {
            this.Add((NSJSValue)NSJSUInt32.New(this.VirtualMachine, value));
        }

        public void Add(DateTime value)
        {
            this.Add((NSJSValue)NSJSDateTime.New(this.VirtualMachine, value));
        }

        public void Add(string value)
        {
            if (value == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSString.New(this.VirtualMachine, value));
        }

        public void Add(int[] buffer)
        {
            if (buffer == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSInt32Array.New(this.VirtualMachine, buffer));
        }

        public void Add(int[] buffer, int count)
        {
            if (buffer == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSInt32Array.New(this.VirtualMachine, buffer, count));
        }

        public void Add(int* buffer, int count)
        {
            if (buffer == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSInt32Array.New(this.VirtualMachine, buffer, count));
        }

        public void Add(int[] buffer, int ofs, int count)
        {
            if (buffer == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSInt32Array.New(this.VirtualMachine, buffer, ofs, count));
        }

        public void Add(uint[] buffer)
        {
            if (buffer == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSUInt32Array.New(this.VirtualMachine, buffer));
        }

        public void Add(uint[] buffer, int count)
        {
            if (buffer == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSUInt32Array.New(this.VirtualMachine, buffer, count));
        }

        public void Add(uint* buffer, int count)
        {
            if (buffer == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSUInt32Array.New(this.VirtualMachine, buffer, count));
        }

        public void Add(uint[] buffer, int ofs, int count)
        {
            if (buffer == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSUInt32Array.New(this.VirtualMachine, buffer, ofs, count));
        }

        public void Add(short[] buffer)
        {
            if (buffer == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSInt16Array.New(this.VirtualMachine, buffer));
        }

        public void Add(short[] buffer, int count)
        {
            if (buffer == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSInt16Array.New(this.VirtualMachine, buffer, count));
        }

        public void Add(short* buffer, int count)
        {
            if (buffer == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSInt16Array.New(this.VirtualMachine, buffer, count));
        }

        public void Add(short[] buffer, int ofs, int count)
        {
            if (buffer == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSInt16Array.New(this.VirtualMachine, buffer, ofs, count));
        }

        public void Add(ushort[] buffer)
        {
            if (buffer == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSUInt16Array.New(this.VirtualMachine, buffer));
        }

        public void Add(ushort[] buffer, int count)
        {
            if (buffer == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSUInt16Array.New(this.VirtualMachine, buffer, count));
        }

        public void Add(ushort* buffer, int count)
        {
            if (buffer == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSUInt16Array.New(this.VirtualMachine, buffer, count));
        }

        public void Add(ushort[] buffer, int ofs, int count)
        {
            if (buffer == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSUInt16Array.New(this.VirtualMachine, buffer, ofs, count));
        }

        public void Add(byte[] buffer)
        {
            if (buffer == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSUInt8Array.New(this.VirtualMachine, buffer));
        }

        public void Add(byte[] buffer, int count)
        {
            if (buffer == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSUInt8Array.New(this.VirtualMachine, buffer, count));
        }

        public void Add(byte* buffer, int count)
        {
            if (buffer == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSUInt8Array.New(this.VirtualMachine, buffer, count));
        }

        public void Add(byte[] buffer, int ofs, int count)
        {
            if (buffer == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSUInt8Array.New(this.VirtualMachine, buffer, ofs, count));
        }

        public void Add(sbyte[] buffer)
        {
            if (buffer == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSInt8Array.New(this.VirtualMachine, buffer));
        }

        public void Add(sbyte[] buffer, int count)
        {
            if (buffer == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSInt8Array.New(this.VirtualMachine, buffer, count));
        }

        public void Add(sbyte* buffer, int count)
        {
            if (buffer == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSInt8Array.New(this.VirtualMachine, buffer, count));
        }

        public void Add(sbyte[] buffer, int ofs, int count)
        {
            if (buffer == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSInt8Array.New(this.VirtualMachine, buffer, ofs, count));
        }

        public void Add(float[] buffer)
        {
            if (buffer == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSFloat32Array.New(this.VirtualMachine, buffer));
        }

        public void Add(float[] buffer, int count)
        {
            if (buffer == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSFloat32Array.New(this.VirtualMachine, buffer, count));
        }

        public void Add(float* buffer, int count)
        {
            if (buffer == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSFloat32Array.New(this.VirtualMachine, buffer, count));
        }

        public void Add(float[] buffer, int ofs, int count)
        {
            if (buffer == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSFloat32Array.New(this.VirtualMachine, buffer, ofs, count));
        }

        public void Add(double[] buffer)
        {
            if (buffer == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSFloat64Array.New(this.VirtualMachine, buffer));
        }

        public void Add(double[] buffer, int count)
        {
            if (buffer == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSFloat64Array.New(this.VirtualMachine, buffer, count));
        }

        public void Add(double* buffer, int count)
        {
            if (buffer == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSFloat64Array.New(this.VirtualMachine, buffer, count));
        }

        public void Add(double[] buffer, int ofs, int count)
        {
            if (buffer == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSFloat64Array.New(this.VirtualMachine, buffer, ofs, count));
        }

        public void Add(NSJSFunctionCallback callback)
        {
            if (callback == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSFunction.New(this.VirtualMachine, callback));
        }

        public void Add(NSJSFunctionCallback2 callback)
        {
            if (callback == null)
            {
                this.Add(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.Add(NSJSFunction.New(this.VirtualMachine, callback));
        }

        IEnumerator<NSJSValue> IEnumerable<NSJSValue>.GetEnumerator()
        {
            int count = this.Length;
            for (int i = 0; i < count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            IEnumerable<NSJSValue> enumerator = this;
            return enumerator.GetEnumerator();
        }

        protected IEnumerator<T> GetEnumerator<T>(IList<T> buffer)
        {
            if (buffer == null)
            {
                buffer = new T[0];
            }
            return buffer.GetEnumerator();
        }
    }
}
