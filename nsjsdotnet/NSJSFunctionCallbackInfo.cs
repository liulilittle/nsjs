namespace nsjsdotnet
{
    using System;
    using System.Runtime.InteropServices;
    using System.Diagnostics;

    public unsafe class NSJSFunctionCallbackInfo : EventArgs
    {
        private static readonly IntPtr NULL = IntPtr.Zero;

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.SysInt)]
        private extern static IntPtr nsjs_argument_get_solt(IntPtr info, int solt);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.SysInt)]
        private extern static IntPtr nsjs_argument_get_this([In]IntPtr info);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.SysInt)]
        private extern static IntPtr nsjs_argument_get_isolate([In]IntPtr info);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int nsjs_argument_get_length([In]IntPtr info);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static NSJSValueType nsjs_argument_get_solt_typeid([In]IntPtr info, int solt);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static void nsjs_virtualmachine_set_data2([In]IntPtr isolate, int solt, IntPtr data);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_virtualmachine_get_data2([In]IntPtr isolate, int solt);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_argument_returnvalue_get(IntPtr info);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static void nsjs_argument_returnvalue_set(IntPtr info, IntPtr value);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.SysInt)]
        private extern static IntPtr nsjs_argument_get_data([In]IntPtr info);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.SysInt)]
        private extern static IntPtr nsjs_argument_get_callee([In]IntPtr info);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static void nsjs_argument_returnvalue_set_boolean([In]IntPtr info, bool value);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static void nsjs_argument_returnvalue_set_int32([In]IntPtr info, int value);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static void nsjs_argument_returnvalue_set_uint32([In]IntPtr info, uint value);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static void nsjs_argument_returnvalue_set_float64([In]IntPtr info, double value);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private NSJSObject m_This;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private NSJSValue m_Data;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private NSJSFunction m_Callee;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IntPtr m_Isolate;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private NSJSValue[] m_Parameters;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private NSJSVirtualMachine m_VirtualMachine;

        private NSJSFunctionCallbackInfo(IntPtr handle)
        {
            if (handle == NULL)
            {
                throw new ArgumentNullException("handle");
            }
            this.Handle = handle;
            this.IntegerBoolean = this.VirtualMachine.IntegerBoolean;
        }

        public NSJSObject This
        {
            get
            {
                lock (this)
                {
                    if (m_This == null)
                    {
                        IntPtr handle = nsjs_argument_get_this(this.Handle);
                        if (handle == NULL)
                        {
                            throw new InvalidOperationException("this");
                        }
                        m_This = new NSJSObject(handle, this.VirtualMachine);
                    }
                    return m_This;
                }
            }
        }

        public NSJSValue Data
        {
            get
            {
                lock (this)
                {
                    if (m_Data == null)
                    {
                        IntPtr handle = nsjs_argument_get_data(this.Handle);
                        if (handle == NULL)
                        {
                            throw new InvalidOperationException("this");
                        }
                        m_Data = NSJSValueBuilder.From(handle, this.VirtualMachine);
                    }
                    return m_Data;
                }
            }
        }

        public NSJSFunction Callee
        {
            get
            {
                lock (this)
                {
                    if (m_Callee == null)
                    {
                        IntPtr handle = nsjs_argument_get_callee(this.Handle);
                        if (handle == NULL)
                        {
                            throw new InvalidOperationException("this");
                        }
                        m_Callee = new NSJSFunction(handle, null, this.VirtualMachine);
                    }
                    return m_Callee;
                }
            }
        }

        public IntPtr Isolate
        {
            get
            {
                lock (this)
                {
                    if (m_Isolate == NULL)
                    {
                        m_Isolate = nsjs_argument_get_isolate(this.Handle);
                    }
                    return m_Isolate;
                }
            }
        }

        public IntPtr Handle
        {
            get;
            private set;
        }

        public int Length
        {
            get
            {
                return this.GetParameters().Length;
            }
        }

        public NSJSValue GetReturnValue()
        {
            if (this.Handle == NULL)
            {
                throw new ArgumentNullException("this");
            }
            IntPtr handle = nsjs_argument_returnvalue_get(this.Handle);
            if (handle == NULL)
            {
                throw new InvalidOperationException();
            }
            return NSJSValueBuilder.From(handle, this.VirtualMachine);
        }

        public bool IntegerBoolean
        {
            get;
            set;
        }

        public void SetReturnValue(bool value)
        {
            if (this.IntegerBoolean)
            {
                this.SetReturnValue(value ? 1 : 0);
            }
            else
            {
                nsjs_argument_returnvalue_set_boolean(this.Handle, value);
            }
        }

        public void SetReturnValue(int value)
        {
            nsjs_argument_returnvalue_set_int32(this.Handle, value);
        }

        public void SetReturnValue(double value)
        {
            nsjs_argument_returnvalue_set_float64(this.Handle, value);
        }

        public void SetReturnValue(uint value)
        {
            nsjs_argument_returnvalue_set_uint32(this.Handle, value);
        }

        public void SetReturnValue(DateTime value)
        {
            this.SetReturnValue(NSJSDateTime.New(this.VirtualMachine, value));
        }

        public void SetReturnValue(string value)
        {
            if (value == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSString.New(this.VirtualMachine, value));
        }

        public void SetReturnValue(int[] buffer)
        {
            if (buffer == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSInt32Array.New(this.VirtualMachine, buffer));
        }

        public void SetReturnValue(int[] buffer, int count)
        {
            if (buffer == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSInt32Array.New(this.VirtualMachine, buffer, count));
        }

        public void SetReturnValue(int* buffer, int count)
        {
            if (buffer == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSInt32Array.New(this.VirtualMachine, buffer, count));
        }

        public void SetReturnValue(int[] buffer, int ofs, int count)
        {
            if (buffer == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSInt32Array.New(this.VirtualMachine, buffer, ofs, count));
        }

        public void SetReturnValue(uint[] buffer)
        {
            if (buffer == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSUInt32Array.New(this.VirtualMachine, buffer));
        }

        public void SetReturnValue(uint[] buffer, int count)
        {
            if (buffer == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSUInt32Array.New(this.VirtualMachine, buffer, count));
        }

        public void SetReturnValue(uint* buffer, int count)
        {
            if (buffer == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSUInt32Array.New(this.VirtualMachine, buffer, count));
        }

        public void SetReturnValue(uint[] buffer, int ofs, int count)
        {
            if (buffer == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSUInt32Array.New(this.VirtualMachine, buffer, ofs, count));
        }

        public void SetReturnValue(short[] buffer)
        {
            if (buffer == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSInt16Array.New(this.VirtualMachine, buffer));
        }

        public void SetReturnValue(short[] buffer, int count)
        {
            if (buffer == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSInt16Array.New(this.VirtualMachine, buffer, count));
        }

        public void SetReturnValue(short* buffer, int count)
        {
            if (buffer == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSInt16Array.New(this.VirtualMachine, buffer, count));
        }

        public void SetReturnValue(short[] buffer, int ofs, int count)
        {
            if (buffer == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSInt16Array.New(this.VirtualMachine, buffer, ofs, count));
        }

        public void SetReturnValue(ushort[] buffer)
        {
            if (buffer == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSUInt16Array.New(this.VirtualMachine, buffer));
        }

        public void SetReturnValue(ushort[] buffer, int count)
        {
            if (buffer == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSUInt16Array.New(this.VirtualMachine, buffer, count));
        }

        public void SetReturnValue(ushort* buffer, int count)
        {
            if (buffer == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSUInt16Array.New(this.VirtualMachine, buffer, count));
        }

        public void SetReturnValue(ushort[] buffer, int ofs, int count)
        {
            if (buffer == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSUInt16Array.New(this.VirtualMachine, buffer, ofs, count));
        }

        public void SetReturnValue(byte[] buffer)
        {
            if (buffer == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSUInt8Array.New(this.VirtualMachine, buffer));
        }

        public void SetReturnValue(byte[] buffer, int count)
        {
            if (buffer == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSUInt8Array.New(this.VirtualMachine, buffer, count));
        }

        public void SetReturnValue(byte* buffer, int count)
        {
            if (buffer == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSUInt8Array.New(this.VirtualMachine, buffer, count));
        }

        public void SetReturnValue(byte[] buffer, int ofs, int count)
        {
            if (buffer == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSUInt8Array.New(this.VirtualMachine, buffer, ofs, count));
        }

        public void SetReturnValue(sbyte[] buffer)
        {
            if (buffer == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSInt8Array.New(this.VirtualMachine, buffer));
        }

        public void SetReturnValue(sbyte[] buffer, int count)
        {
            if (buffer == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSInt8Array.New(this.VirtualMachine, buffer, count));
        }

        public void SetReturnValue(sbyte* buffer, int count)
        {
            if (buffer == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSInt8Array.New(this.VirtualMachine, buffer, count));
        }

        public void SetReturnValue(sbyte[] buffer, int ofs, int count)
        {
            if (buffer == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSInt8Array.New(this.VirtualMachine, buffer, ofs, count));
        }

        public void SetReturnValue(float[] buffer)
        {
            if (buffer == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSFloat32Array.New(this.VirtualMachine, buffer));
        }

        public void SetReturnValue(float[] buffer, int count)
        {
            if (buffer == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSFloat32Array.New(this.VirtualMachine, buffer, count));
        }

        public void SetReturnValue(float* buffer, int count)
        {
            if (buffer == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSFloat32Array.New(this.VirtualMachine, buffer, count));
        }

        public void SetReturnValue(float[] buffer, int ofs, int count)
        {
            if (buffer == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSFloat32Array.New(this.VirtualMachine, buffer, ofs, count));
        }

        public void SetReturnValue(double[] buffer)
        {
            if (buffer == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSFloat64Array.New(this.VirtualMachine, buffer));
        }

        public void SetReturnValue(double[] buffer, int count)
        {
            if (buffer == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSFloat64Array.New(this.VirtualMachine, buffer, count));
        }

        public void SetReturnValue(double* buffer, int count)
        {
            if (buffer == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSFloat64Array.New(this.VirtualMachine, buffer, count));
        }

        public void SetReturnValue(double[] buffer, int ofs, int count)
        {
            if (buffer == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSFloat64Array.New(this.VirtualMachine, buffer, ofs, count));
        }

        public void SetReturnValue(NSJSFunctionCallback callback)
        {
            if (callback == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSFunction.New(This.VirtualMachine, callback));
        }

        public void SetReturnValue(NSJSFunctionCallback2 callback)
        {
            if (callback == null)
            {
                this.SetReturnValue(NSJSValue.Null(this.VirtualMachine));
                return;
            }
            this.SetReturnValue(NSJSFunction.New(This.VirtualMachine, callback));
        }

        public void SetReturnValue(NSJSValue value)
        {
            if (value == null)
            {
                value = NSJSValue.Null(this.VirtualMachine);
            }
            nsjs_argument_returnvalue_set(this.Handle, value.Handle);
        }

        public virtual NSJSValue this[int index]
        {
            get
            {
                return this.GetParameters()[index];
            }
        }

        public virtual NSJSValue[] GetParameters()
        {
            lock (this)
            {
                if (this.m_Parameters == null)
                {
                    int count = nsjs_argument_get_length(this.Handle);
                    if (count < 0)
                    {
                        count = 0;
                    }
                    this.m_Parameters = new NSJSValue[count];
                    NSJSVirtualMachine machine = this.VirtualMachine;
                    for (int i = 0; i < count; i++)
                    {
                        IntPtr localValue = nsjs_argument_get_solt(this.Handle, i);
                        if (localValue == NULL)
                        {
                            continue;
                        }
                        this.m_Parameters[i] = NSJSValueBuilder.From(localValue, machine);
                    }
                }
                return this.m_Parameters;
            }
        }

        public static NSJSFunctionCallbackInfo From(IntPtr handle)
        {
            if (handle == NULL)
            {
                return null;
            }
            return new NSJSFunctionCallbackInfo(handle);
        }

        public NSJSVirtualMachine VirtualMachine
        {
            get
            {
                lock (this)
                {
                    if (this.m_VirtualMachine == null)
                    {
                        IntPtr p = nsjs_virtualmachine_get_data2(this.Isolate, 0);
                        if (p == null)
                        {
                            throw new InvalidOperationException();
                        }
                        this.m_VirtualMachine = NSJSVirtualMachine.From(p);
                    }
                    return this.m_VirtualMachine;
                }
            }
        }
    }
}
