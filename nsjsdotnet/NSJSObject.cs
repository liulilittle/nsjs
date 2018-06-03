namespace nsjsdotnet
{
    using nsjsdotnet.Core;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;                                                                                               

    public unsafe class NSJSObject : NSJSValue
    {
        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_localvalue_object_new(IntPtr isolate, int fieldcount);

        internal NSJSObject(NSJSVirtualMachine machine, int fieldcount) : base(nsjs_localvalue_object_new(machine.Isolate, fieldcount), NSJSValueType.kObject, machine)
        {

        }

        public static NSJSObject New(NSJSVirtualMachine machine)
        {
            return New(machine, 0);
        }

        protected static NSJSObject New(NSJSVirtualMachine machine, int fieldcount)
        {
            if (machine == null)
            {
                throw new ArgumentNullException("machine");
            }
            if (machine.Isolate == NULL)
            {
                throw new InvalidOperationException("machine");
            }
            return new NSJSObject(machine, fieldcount);
        }

        internal NSJSObject(IntPtr handle, NSJSVirtualMachine machine) : base(handle, NSJSValueType.kObject, machine)
        {

        }

        internal NSJSObject(IntPtr handle, NSJSValueType datatype, NSJSVirtualMachine machine) : base(handle, datatype, machine)
        {

        }

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_localvalue_object_property_get(IntPtr isolate, IntPtr localValue, void* key);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static sbyte** nsjs_localvalue_object_getallkeys(IntPtr localValue, ref int count);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static bool nsjs_localvalue_object_property_set(IntPtr isolate, IntPtr obj, IntPtr key, IntPtr value);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static bool nsjs_localvalue_object_property_set_int32(IntPtr isolate, IntPtr obj, IntPtr key, int value);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static bool nsjs_localvalue_object_property_set_uint32(IntPtr isolate, IntPtr obj, IntPtr key, uint value);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static bool nsjs_localvalue_object_property_set_boolean(IntPtr isolate, IntPtr obj, IntPtr key, bool value);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static bool nsjs_localvalue_object_property_set_string(IntPtr isolate, IntPtr obj, IntPtr key, void* value);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static bool nsjs_localvalue_object_property_set_float64(IntPtr isolate, IntPtr obj, IntPtr key, double value);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static bool nsjs_localvalue_object_property_set_datetime(IntPtr isolate, IntPtr obj, IntPtr key, long value);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static bool nsjs_localvalue_object_property_set_function(IntPtr isolate, IntPtr obj, IntPtr key, IntPtr value);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static bool nsjs_localvalue_object_property_set_int8array(IntPtr isolate, IntPtr obj, IntPtr key, sbyte* buffer, uint count);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static bool nsjs_localvalue_object_property_set_uint8array(IntPtr isolate, IntPtr obj, IntPtr key, byte* buffer, uint count);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static bool nsjs_localvalue_object_property_set_int16array(IntPtr isolate, IntPtr obj, IntPtr key, short* buffer, uint count);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static bool nsjs_localvalue_object_property_set_uint16array(IntPtr isolate, IntPtr obj, IntPtr key, ushort* buffer, uint count);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static bool nsjs_localvalue_object_property_set_int32array(IntPtr isolate, IntPtr obj, IntPtr key, int* buffer, uint count);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static bool nsjs_localvalue_object_property_set_uint32array(IntPtr isolate, IntPtr obj, IntPtr key, uint* buffer, uint count);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static bool nsjs_localvalue_object_property_set_float32array(IntPtr isolate, IntPtr obj, IntPtr key, float* buffer, uint count);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static bool nsjs_localvalue_object_property_set_float64array(IntPtr isolate, IntPtr obj, IntPtr key, double* buffer, uint count);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static bool nsjs_localvalue_object_property_delete(IntPtr isolate, IntPtr obj, void* key);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int nsjs_localvalue_object_internalfield_count([In]IntPtr info);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_localvalue_object_internalfield_get([In]IntPtr obj, int solt);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static void nsjs_localvalue_object_internalfield_set([In]IntPtr obj, int solt, IntPtr value);

        private const string RUNTIME_DEFINEPROPERTY_PROPERTYKEY = @"____nsjsdotnet_framework_object_defineproperty";
        private const string RUNTIME_GETPROPERTYDESCRIPTOR_PROPERTYKEY = @"____nsjsdotnet_framework_object_getpropertydescriptor";
        private const string RUNTIME_GETPROPERTYNAMES_PROPERTYKEY = @"____nsjsdotnet_framework_object_getpropertynames";

        protected internal virtual NSJSFunction GetFrameworkFunction(string key)
        {
            return NSJSFunction.GetFrameworkFunction(this.VirtualMachine, key);
        }

        public virtual NSJSObject GetPropertyDescriptor(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }
            NSJSFunction function = this.GetFrameworkFunction(RUNTIME_GETPROPERTYDESCRIPTOR_PROPERTYKEY);
            return function.Call(this, NSJSString.New(this.VirtualMachine, key)) as NSJSObject;
        }

        public virtual IEnumerable<string> GetPropertyNames()
        {
            NSJSFunction function = this.GetFrameworkFunction(RUNTIME_GETPROPERTYNAMES_PROPERTYKEY);
            return ArrayAuxiliary.ToStringList(function.Call(this));
        }

        private void InternalDefineProperty(string key, Action<NSJSVirtualMachine, NSJSFunction> executing)
        {
            if (executing == null)
            {
                throw new ArgumentNullException("executing");
            }
            if (key == null)
            {
                throw new ArgumentNullException("key is not allowed to be null");
            }
            if (key.Length <= 0)
            {
                throw new ArgumentNullException("key is not allowed to be empty");
            }
            NSJSFunction function = this.GetFrameworkFunction(RUNTIME_DEFINEPROPERTY_PROPERTYKEY);
            executing(this.VirtualMachine, function);
        }

        public virtual void DefineProperty(string key, NSJSFunctionCallback2 get, NSJSFunctionCallback2 set)
        {
            this.InternalDefineProperty(key, (machine, function) =>
            {
                NSJSValue[] s = new NSJSValue[]
                {
                    this,
                    NSJSString.New(machine, key),
                    get == null ? NSJSValue.Undefined(machine) : NSJSFunction.New(machine, get),
                    set == null ? NSJSValue.Undefined(machine) : NSJSFunction.New(machine, set),
                };
                function.Call(s);
            });
        }

        public virtual void DefineProperty(string key, NSJSFunctionCallback2 get, NSJSFunctionCallback set)
        {
            this.InternalDefineProperty(key, (machine, function) =>
            {
                NSJSValue[] s = new NSJSValue[]
                {
                    this,
                    NSJSString.New(machine, key),
                    get == null ? NSJSValue.Undefined(machine) : NSJSFunction.New(machine, get),
                    set == null ? NSJSValue.Undefined(machine) : NSJSFunction.New(machine, set),
                };
                function.Call(s);
            });
        }

        public virtual void DefineProperty(string key, NSJSFunctionCallback get, NSJSFunctionCallback2 set)
        {
            this.InternalDefineProperty(key, (machine, function) =>
            {
                NSJSValue[] s = new NSJSValue[]
                {
                    this,
                    NSJSString.New(machine, key),
                    get == null ? NSJSValue.Undefined(machine) : NSJSFunction.New(machine, get),
                    set == null ? NSJSValue.Undefined(machine) : NSJSFunction.New(machine, set),
                };
                function.Call(s);
            });
        }

        public virtual void DefineProperty(string key, NSJSFunctionCallback get, NSJSFunctionCallback set)
        {
            this.InternalDefineProperty(key, (machine, function) =>
            {
                NSJSValue[] s = new NSJSValue[]
                {
                    this,
                    NSJSString.New(machine, key),
                    get == null ? NSJSValue.Undefined(machine) : NSJSFunction.New(machine, get),
                    set == null ? NSJSValue.Undefined(machine) : NSJSFunction.New(machine, set),
                };
                function.Call(s);
            });
        }

        public virtual object UserToken
        {
            get
            {
                return NSJSKeyValueCollection.Get(this as NSJSObject);
            }
            set
            {
                NSJSKeyValueCollection.Set(this as NSJSObject, value);
            }
        }

        public override object GetValue()
        {
            return NSJSJson.Stringify(this);
        }

        protected virtual int GetInternalFieldCount()
        {
            return nsjs_localvalue_object_internalfield_count(this.Handle);
        }

        protected virtual NSJSValue GetInternalField(int solt)
        {
            if (solt < 0 || solt >= GetInternalFieldCount())
            {
                throw new ArgumentOutOfRangeException("The solt location of the access exceeds the specified range");
            }
            IntPtr handle = nsjs_localvalue_object_internalfield_get(this.Handle, solt);
            if (handle == NULL)
            {
                throw new InvalidOperationException();
            }
            return NSJSValueBuilder.From(this.Handle, this, this.VirtualMachine);
        }

        protected virtual void SetInternalField(int solt, NSJSValue value)
        {
            if (solt < 0 || solt >= GetInternalFieldCount())
            {
                throw new ArgumentOutOfRangeException("The solt location of the access exceeds the specified range");
            }
            nsjs_localvalue_object_internalfield_set(this.Handle, solt, value.Handle);
        }

        protected internal virtual IntPtr GetPropertyAndReturnHandle(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (key.Length <= 0)
            {
                throw new ArgumentException("key");
            }
            fixed (byte* s = Encoding.UTF8.GetBytes(key))
            {
                return nsjs_localvalue_object_property_get(this.Isolate, this.Handle, s);
            }
        }

        public virtual NSJSValue Get(string key)
        {
            IntPtr p = this.GetPropertyAndReturnHandle(key);
            if (p == null)
            {
                return null;
            }
            return NSJSValueBuilder.From(p, this, this.VirtualMachine);
        }

        public virtual bool Set(string key, NSJSValue value)
        {
            if (value == null)
            {
                return this.Set(key, NSJSValue.Null(this.VirtualMachine));
            }
            return this.Set(key, (name) => nsjs_localvalue_object_property_set(this.Isolate, this.Handle, name, value.Handle));
        }

        protected virtual bool Set(string key, Func<IntPtr, bool> func)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (key.Length <= 0)
            {
                throw new ArgumentException("key");
            }
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }
            fixed (byte* s = Encoding.UTF8.GetBytes(key))
            {
                return func((IntPtr)s);
            }
        }

        public virtual bool Set(string key, int value)
        {
            return this.Set(key, (name) => nsjs_localvalue_object_property_set_int32(this.Isolate, this.Handle, name, value));
        }

        public virtual bool Set(string key, uint value)
        {
            return this.Set(key, (name) => nsjs_localvalue_object_property_set_uint32(this.Isolate, this.Handle, name, value));
        }

        public virtual bool Set(string key, bool value)
        {
            if (this.IntegerBoolean)
            {
                return this.Set(key, value ? 1 : 0);
            }
            return this.Set(key, (name) => nsjs_localvalue_object_property_set_boolean(this.Isolate, this.Handle, name, value));
        }

        public virtual bool Set(string key, string value)
        {
            if (value == null)
            {
                return this.Set(key, NSJSValue.Null(this.VirtualMachine));
            }
            return this.Set(key, (name) =>
            {
                if (value == null)
                {
                    return nsjs_localvalue_object_property_set_string(this.Isolate, this.Handle, name, null);
                }
                byte[] cch = Encoding.UTF8.GetBytes(value);
                if (cch.Length <= 0)
                {
                    cch = new byte[] { 0 };
                }
                fixed (byte* s = cch)
                {
                    return nsjs_localvalue_object_property_set_string(this.Isolate, this.Handle, name, s);
                }
            });
        }

        public virtual bool Set(string key, float value)
        {
            return this.Set(key, (double)value);
        }

        public virtual bool Set(string key, double value)
        {
            return this.Set(key, (name) => nsjs_localvalue_object_property_set_float64(this.Isolate, this.Handle, name, value));
        }

        public virtual bool Set(string key, sbyte[] value)
        {
            if (value == null)
            {
                return this.Set(key, NSJSValue.Null(this.VirtualMachine));
            }
            return this.Set(key, (name) =>
            {
                if (value == null || value.Length <= 0)
                {
                    return nsjs_localvalue_object_property_set_int8array(this.Isolate, this.Handle, name, null, 0);
                }
                fixed (sbyte* p = value)
                {
                    return nsjs_localvalue_object_property_set_int8array(this.Isolate, this.Handle, name, p, (uint)value.Length);
                }
            });
        }

        public virtual bool Set(string key, sbyte[] buffer, int ofs, int count)
        {
            if (buffer == null)
            {
                return this.Set(key, NSJSValue.Null(this.VirtualMachine));
            }
            return this.Set(key, (name) =>
            {
                if (ofs < 0 || ofs >= buffer.Length)
                {
                    throw new ArgumentOutOfRangeException("The value of ofs is less than 0 or greater than or equal to the buffer length");
                }
                if (count > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException("The value of count is outside the buffer range");
                }
                if ((ofs + count) > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException("ofs and count the value overflow buffer range");
                }
                fixed (sbyte* ch = &buffer[ofs])
                {
                    return this.Set(key, ch, count);
                }
            });
        }

        public virtual bool Set(string key, sbyte* value, int count)
        {
            if (value == null)
            {
                return this.Set(key, NSJSValue.Null(this.VirtualMachine));
            }
            return this.Set(key, (name) =>
            {
                if (value == null && count != 0)
                {
                    throw new ArgumentOutOfRangeException("Parameter count is greater than 0 o'clock, buffer not null is not allowed");
                }
                return nsjs_localvalue_object_property_set_int8array(this.Isolate, this.Handle, name, value, (uint)count);
            });
        }

        public virtual bool Set(string key, byte[] value)
        {
            if (value == null)
            {
                return this.Set(key, NSJSValue.Null(this.VirtualMachine));
            }
            return this.Set(key, (name) =>
            {
                if (value == null || value.Length <= 0)
                {
                    return nsjs_localvalue_object_property_set_uint8array(this.Isolate, this.Handle, name, null, 0);
                }
                fixed (byte* p = value)
                {
                    return nsjs_localvalue_object_property_set_uint8array(this.Isolate, this.Handle, name, p, (uint)value.Length);
                }
            });
        }

        public virtual bool Set(string key, byte[] buffer, int ofs, int count)
        {
            if (buffer == null)
            {
                return this.Set(key, NSJSValue.Null(this.VirtualMachine));
            }
            return this.Set(key, (name) =>
            {
                if (ofs < 0 || ofs >= buffer.Length)
                {
                    throw new ArgumentOutOfRangeException("The value of ofs is less than 0 or greater than or equal to the buffer length");
                }
                if (count > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException("The value of count is outside the buffer range");
                }
                if ((ofs + count) > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException("ofs and count the value overflow buffer range");
                }
                fixed (byte* ch = &buffer[ofs])
                {
                    return this.Set(key, ch, count);
                }
            });
        }

        public virtual bool Set(string key, byte* value, int count)
        {
            if (value == null)
            {
                return this.Set(key, NSJSValue.Null(this.VirtualMachine));
            }
            return this.Set(key, (name) =>
            {
                if (value == null && count != 0)
                {
                    throw new ArgumentOutOfRangeException("Parameter count is greater than 0 o'clock, buffer not null is not allowed");
                }
                return nsjs_localvalue_object_property_set_uint8array(this.Isolate, this.Handle, name, value, (uint)count);
            });
        }

        public virtual bool Set(string key, short[] value)
        {
            if (value == null)
            {
                return this.Set(key, NSJSValue.Null(this.VirtualMachine));
            }
            return this.Set(key, (name) =>
            {
                if (value == null || value.Length <= 0)
                {
                    return nsjs_localvalue_object_property_set_int16array(this.Isolate, this.Handle, name, null, 0);
                }
                fixed (short* p = value)
                {
                    return nsjs_localvalue_object_property_set_int16array(this.Isolate, this.Handle, name, p, (uint)value.Length);
                }
            });
        }

        public virtual bool Set(string key, short[] buffer, int ofs, int count)
        {
            if (buffer == null)
            {
                return this.Set(key, NSJSValue.Null(this.VirtualMachine));
            }
            return this.Set(key, (name) =>
            {
                if (ofs < 0 || ofs >= buffer.Length)
                {
                    throw new ArgumentOutOfRangeException("The value of ofs is less than 0 or greater than or equal to the buffer length");
                }
                if (count > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException("The value of count is outside the buffer range");
                }
                if ((ofs + count) > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException("ofs and count the value overflow buffer range");
                }
                fixed (short* ch = &buffer[ofs])
                {
                    return this.Set(key, ch, count);
                }
            });
        }

        public virtual bool Set(string key, short* value, int count)
        {
            if (value == null)
            {
                return this.Set(key, NSJSValue.Null(this.VirtualMachine));
            }
            return this.Set(key, (name) =>
            {
                if (value == null && count != 0)
                {
                    throw new ArgumentOutOfRangeException("Parameter count is greater than 0 o'clock, buffer not null is not allowed");
                }
                return nsjs_localvalue_object_property_set_int16array(this.Isolate, this.Handle, name, value, (uint)count);
            });
        }

        public virtual bool Set(string key, ushort[] value)
        {
            if (value == null)
            {
                return this.Set(key, NSJSValue.Null(this.VirtualMachine));
            }
            return this.Set(key, (name) =>
            {
                if (value == null || value.Length <= 0)
                {
                    return nsjs_localvalue_object_property_set_uint16array(this.Isolate, this.Handle, name, null, 0);
                }
                fixed (ushort* p = value)
                {
                    return nsjs_localvalue_object_property_set_uint16array(this.Isolate, this.Handle, name, p, (uint)value.Length);
                }
            });
        }

        public virtual bool Set(string key, ushort[] buffer, int ofs, int count)
        {
            if (buffer == null)
            {
                return this.Set(key, NSJSValue.Null(this.VirtualMachine));
            }
            return this.Set(key, (name) =>
            {
                if (ofs < 0 || ofs >= buffer.Length)
                {
                    throw new ArgumentOutOfRangeException("The value of ofs is less than 0 or greater than or equal to the buffer length");
                }
                if (count > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException("The value of count is outside the buffer range");
                }
                if ((ofs + count) > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException("ofs and count the value overflow buffer range");
                }
                fixed (ushort* ch = &buffer[ofs])
                {
                    return this.Set(key, ch, count);
                }
            });
        }

        public virtual bool Set(string key, ushort* value, int count)
        {
            if (value == null)
            {
                return this.Set(key, NSJSValue.Null(this.VirtualMachine));
            }
            return this.Set(key, (name) =>
            {
                if (value == null && count != 0)
                {
                    throw new ArgumentOutOfRangeException("Parameter count is greater than 0 o'clock, buffer not null is not allowed");
                }
                return nsjs_localvalue_object_property_set_uint16array(this.Isolate, this.Handle, name, value, (uint)count);
            });
        }

        public virtual bool Set(string key, int[] value)
        {
            if (value == null)
            {
                return this.Set(key, NSJSValue.Null(this.VirtualMachine));
            }
            return this.Set(key, (name) =>
            {
                if (value == null || value.Length <= 0)
                {
                    return nsjs_localvalue_object_property_set_int32array(this.Isolate, this.Handle, name, null, 0);
                }
                fixed (int* p = value)
                {
                    return nsjs_localvalue_object_property_set_int32array(this.Isolate, this.Handle, name, p, (uint)value.Length);
                }
            });
        }

        public virtual bool Set(string key, int[] buffer, int ofs, int count)
        {
            if (buffer == null)
            {
                return this.Set(key, NSJSValue.Null(this.VirtualMachine));
            }
            return this.Set(key, (name) =>
            {
                if (ofs < 0 || ofs >= buffer.Length)
                {
                    throw new ArgumentOutOfRangeException("The value of ofs is less than 0 or greater than or equal to the buffer length");
                }
                if (count > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException("The value of count is outside the buffer range");
                }
                if ((ofs + count) > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException("ofs and count the value overflow buffer range");
                }
                fixed (int* ch = &buffer[ofs])
                {
                    return this.Set(key, ch, count);
                }
            });
        }

        public virtual bool Set(string key, int* value, int count)
        {
            if (value == null)
            {
                return this.Set(key, NSJSValue.Null(this.VirtualMachine));
            }
            return this.Set(key, (name) =>
            {
                if (value == null && count != 0)
                {
                    throw new ArgumentOutOfRangeException("Parameter count is greater than 0 o'clock, buffer not null is not allowed");
                }
                return nsjs_localvalue_object_property_set_int32array(this.Isolate, this.Handle, name, value, (uint)count);
            });
        }

        public virtual bool Set(string key, uint[] value)
        {
            if (value == null)
            {
                return this.Set(key, NSJSValue.Null(this.VirtualMachine));
            }
            return this.Set(key, (name) =>
            {
                if (value == null || value.Length <= 0)
                {
                    return nsjs_localvalue_object_property_set_uint32array(this.Isolate, this.Handle, name, null, 0);
                }
                fixed (uint* p = value)
                {
                    return nsjs_localvalue_object_property_set_uint32array(this.Isolate, this.Handle, name, p, (uint)value.Length);
                }
            });
        }

        public virtual bool Set(string key, uint[] buffer, int ofs, int count)
        {
            if (buffer == null)
            {
                return this.Set(key, NSJSValue.Null(this.VirtualMachine));
            }
            return this.Set(key, (name) =>
            {
                if (ofs < 0 || ofs >= buffer.Length)
                {
                    throw new ArgumentOutOfRangeException("The value of ofs is less than 0 or greater than or equal to the buffer length");
                }
                if (count > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException("The value of count is outside the buffer range");
                }
                if ((ofs + count) > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException("ofs and count the value overflow buffer range");
                }
                fixed (uint* ch = &buffer[ofs])
                {
                    return this.Set(key, ch, count);
                }
            });
        }

        public virtual bool Set(string key, uint* value, int count)
        {
            if (value == null)
            {
                return this.Set(key, NSJSValue.Null(this.VirtualMachine));
            }
            return this.Set(key, (name) =>
            {
                if (value == null && count != 0)
                {
                    throw new ArgumentOutOfRangeException("Parameter count is greater than 0 o'clock, buffer not null is not allowed");
                }
                return nsjs_localvalue_object_property_set_uint32array(this.Isolate, this.Handle, name, value, (uint)count);
            });
        }

        public virtual bool Set(string key, float[] value)
        {
            if (value == null)
            {
                return this.Set(key, NSJSValue.Null(this.VirtualMachine));
            }
            return this.Set(key, (name) =>
            {
                if (value == null || value.Length <= 0)
                {
                    return nsjs_localvalue_object_property_set_float32array(this.Isolate, this.Handle, name, null, 0);
                }
                fixed (float* p = value)
                {
                    return nsjs_localvalue_object_property_set_float32array(this.Isolate, this.Handle, name, p, (uint)value.Length);
                }
            });
        }

        public virtual bool Set(string key, float[] buffer, int ofs, int count)
        {
            if (buffer == null)
            {
                return this.Set(key, NSJSValue.Null(this.VirtualMachine));
            }
            return this.Set(key, (name) =>
            {
                if (ofs < 0 || ofs >= buffer.Length)
                {
                    throw new ArgumentOutOfRangeException("The value of ofs is less than 0 or greater than or equal to the buffer length");
                }
                if (count > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException("The value of count is outside the buffer range");
                }
                if ((ofs + count) > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException("ofs and count the value overflow buffer range");
                }
                fixed (float* ch = &buffer[ofs])
                {
                    return this.Set(key, ch, count);
                }
            });
        }

        public virtual bool Set(string key, float* value, int count)
        {
            if (value == null)
            {
                return this.Set(key, NSJSValue.Null(this.VirtualMachine));
            }
            return this.Set(key, (name) =>
            {
                if (value == null && count != 0)
                {
                    throw new ArgumentOutOfRangeException("Parameter count is greater than 0 o'clock, buffer not null is not allowed");
                }
                return nsjs_localvalue_object_property_set_float32array(this.Isolate, this.Handle, name, value, (uint)count);
            });
        }

        public virtual bool Set(string key, double[] value)
        {
            if (value == null)
            {
                return this.Set(key, NSJSValue.Null(this.VirtualMachine));
            }
            return this.Set(key, (name) =>
            {
                if (value == null || value.Length <= 0)
                {
                    return nsjs_localvalue_object_property_set_float64array(this.Isolate, this.Handle, name, null, 0);
                }
                fixed (double* p = value)
                {
                    return nsjs_localvalue_object_property_set_float64array(this.Isolate, this.Handle, name, p, (uint)value.Length);
                }
            });
        }

        public virtual bool Set(string key, double[] buffer, int ofs, int count)
        {
            if (buffer == null)
            {
                return this.Set(key, NSJSValue.Null(this.VirtualMachine));
            }
            return this.Set(key, (name) =>
            {
                if (ofs < 0 || ofs >= buffer.Length)
                {
                    throw new ArgumentOutOfRangeException("The value of ofs is less than 0 or greater than or equal to the buffer length");
                }
                if (count > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException("The value of count is outside the buffer range");
                }
                if ((ofs + count) > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException("ofs and count the value overflow buffer range");
                }
                fixed (double* ch = &buffer[ofs])
                {
                    return this.Set(key, ch, count);
                }
            });
        }

        public virtual bool Set(string key, double* value, int count)
        {
            if (value == null)
            {
                return this.Set(key, NSJSValue.Null(this.VirtualMachine));
            }
            return this.Set(key, (name) =>
            {
                if (value == null && count != 0)
                {
                    throw new ArgumentOutOfRangeException("Parameter count is greater than 0 o'clock, buffer not null is not allowed");
                }
                return nsjs_localvalue_object_property_set_float64array(this.Isolate, this.Handle, name, value, (uint)count);
            });
        }

        public virtual bool Set(string key, DateTime value)
        {
            if (NSJSDateTime.Invalid(value))
            {
                throw new ArgumentNullException("Parameter cannot be an invalid time");
            }
            long time = NSJSDateTime.DateTimeToLocalDate(value);
            return this.Set(key, (name) => nsjs_localvalue_object_property_set_datetime(this.Isolate, this.Handle, name, time));
        }

        public virtual bool Set(string key, NSJSFunctionCallback value)
        {
            return InternalSet(key, value);
        }

        public virtual bool Set(string key, NSJSFunctionCallback2 value)
        {
            return InternalSet(key, value);
        }

        private bool InternalSet(string key, Delegate value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return this.Set(key, (name) => nsjs_localvalue_object_property_set_function(this.Isolate, this.Handle, name, NSJSFunction.DelegateToFunctionPtr(value)));
        }

        public virtual bool Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (key.Length <= 0)
            {
                throw new ArgumentException("key");
            }
            fixed (byte* s = Encoding.UTF8.GetBytes(key))
            {
                return nsjs_localvalue_object_property_delete(this.Isolate, this.Handle, s);
            }
        }

        public virtual IEnumerable<string> GetAllKeys()
        {
            int count = 0;
            sbyte** ch = nsjs_localvalue_object_getallkeys(this.Handle, ref count);
            ISet<string> r = new HashSet<string>();
            if (ch != null)
            {
                for (int i = 0; i < count; i++)
                {
                    sbyte* s = ch[i];
                    string key = new string(s);
                    if (s != null)
                    {
                        NSJSMemoryManagement.Free(s);
                    }
                    if (string.IsNullOrEmpty(key))
                    {
                        continue;
                    }
                    r.Add(key);
                }
                NSJSMemoryManagement.Free(ch);
            }
            return r;
        }
    }
}
