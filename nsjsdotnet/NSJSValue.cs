namespace nsjsdotnet
{
    using System;
    using System.Runtime.InteropServices;
    using System.Diagnostics;

    public unsafe class NSJSValue : IDisposable
    {
        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static sbyte* nsjs_localvalue_get_string(IntPtr localValue, ref int len);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int nsjs_localvalue_is_cross_threading(IntPtr h);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static bool nsjs_localvalue_set_cross_threading(IntPtr isolate, IntPtr value, bool disabled);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static bool nsjs_localvalue_free(IntPtr h);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static NSJSValueType nsjs_localvalue_get_typeid(IntPtr localValue);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static bool nsjs_localvalue_equals(IntPtr x, IntPtr y);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int nsjs_localvalue_hashcode([In]IntPtr info);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static sbyte* nsjs_localvalue_typeof(IntPtr isolate, IntPtr value);

        protected static readonly IntPtr NULL = IntPtr.Zero;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool disposed;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int? hashcodeTokenId;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private NSJSValueType? metadataToken;

        public const string NullString = "null";
        public const string UndefinedString = "undefined";

        public static readonly NSJSValue[] EmptyValues = new NSJSValue[0];

        public NSJSValueType MetadataToken
        {
            get
            {
                lock (this)
                {
                    if (this.metadataToken == null)
                    {
                        this.metadataToken = nsjs_localvalue_get_typeid(this.Handle);
                    }
                    return this.metadataToken.Value;
                }
            }
        }

        public static string TypeOf(NSJSValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            sbyte* s = nsjs_localvalue_typeof(value.Isolate, value.Handle);
            string r = s != null ? new string(s) : null;
            NSJSMemoryManagement.Free(s);
            return r;
        }

        public override int GetHashCode()
        {
            lock (this)
            {
                if (this.hashcodeTokenId == null)
                {
                    if (this.IsNullOrUndfined)
                    {
                        this.hashcodeTokenId = 0;
                    }
                    else
                    {
                        if (DateType == NSJSValueType.kBoolean ||
                            DateType == NSJSValueType.kDateTime ||
                            DateType == NSJSValueType.kDouble ||
                            DateType == NSJSValueType.kInt32 ||
                            DateType == NSJSValueType.kUInt32 ||
                            DateType == NSJSValueType.kString)
                        {
                            object value = this.GetValue();
                            if (value == null)
                            {
                                this.hashcodeTokenId = 0;
                            }
                            else
                            {
                                this.hashcodeTokenId = value.GetHashCode(); // RuntimeHelpers.GetHashCode(value);
                            }
                        }
                        else
                        {
                            this.hashcodeTokenId = nsjs_localvalue_hashcode(this.Handle);
                        }
                    }
                }
                return this.hashcodeTokenId.GetValueOrDefault();
            }
        }

        public virtual object Tag
        {
            get;
            set;
        }

        public virtual object UserToken
        {
            get
            {
                return NSJSKeyValueCollection.Get(this);
            }
            set
            {
                NSJSKeyValueCollection.Set(this, value);
            }
        }

        public static int GetHashCode(NSJSValue value)
        {
            if (value == null)
            {
                return 0;
            }
            return value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            NSJSValue y = obj as NSJSValue;
            NSJSValue x = this;
            if (x == null && y != null)
            {
                return false;
            }
            if (x != null && y == null)
            {
                return false;
            }
            return nsjs_localvalue_equals(x.Handle, y.Handle);
        }

        internal NSJSValue(IntPtr handle, NSJSValueType datatype, NSJSVirtualMachine machine)
        {
            if (machine == null)
            {
                throw new ArgumentNullException("machine");
            }
            if (handle == NULL)
            {
                throw new ArgumentNullException("handle");
            }
            this.Handle = handle;
            this.VirtualMachine = machine;
            this.Isolate = machine.Isolate;
            if (this.Isolate == NULL)
            {
                throw new InvalidOperationException("machine");
            }
            this.DateType = datatype;
            if (machine.CrossThreading)
            {
                this.CrossThreading = machine.CrossThreading;
            }
            this.IntegerBoolean = machine.IntegerBoolean;
        }

        ~NSJSValue()
        {
            this.Dispose();
        }

        public bool IntegerBoolean
        {
            get;
            set;
        }

        public IntPtr Handle { get; } = NULL;

        public bool IsUndfined
        {
            get
            {
                return this.DateType == NSJSValueType.kUndefined;
            }
        }

        public bool IsNull
        {
            get
            {
                return (this.DateType & NSJSValueType.kNull) > 0;
            }
        }

        public bool IsNullOrUndfined
        {
            get
            {
                return this.IsUndfined || this.IsNull;
            }
        }

        public IntPtr Isolate { get; } = NULL;

        public NSJSValueType DateType { get; }

        public NSJSVirtualMachine VirtualMachine { get; } = null;

        public virtual object GetValue()
        {
            if (this.IsUndfined)
            {
                return UndefinedString;
            }
            if (this.IsNull)
            {
                return NullString;
            }
            return null;
        }

        public bool CrossThreading
        {
            get
            {
                return nsjs_localvalue_is_cross_threading(this.Handle) != 0;
            }
            set
            {
                nsjs_localvalue_set_cross_threading(this.Isolate, this.Handle, !value);
            }
        }

        protected virtual string LocalValueToString()
        {
            int len = 0;
            sbyte* chunk = nsjs_localvalue_get_string(this.Handle, ref len);
            string result = chunk != null ? new string(chunk) : null;
            NSJSMemoryManagement.Free(chunk);
            return result;
        }

        public virtual void Dispose()
        {
            lock (this)
            {
                if (!this.disposed)
                {
                    this.disposed = true; // NSJSKeyValueCollection.Release(this.VirtualMachine, this.hashcodeTokenId.GetValueOrDefault());
                    if (!nsjs_localvalue_free(this.Handle))
                    {
                        throw new InvalidOperationException("Unable to free value");
                    }
                }
            }
            GC.SuppressFinalize(this);
        }

        public static NSJSValue Undefined(NSJSVirtualMachine machine)
        {
            return NewNullOrUndfined(machine, true);
        }

        public static NSJSValue Null(NSJSVirtualMachine machine)
        {
            return NewNullOrUndfined(machine, false);
        }

        public static NSJSValue NullMerge(NSJSVirtualMachine machine, NSJSValue value)
        {
            if (value == null)
            {
                return NSJSValue.Null(machine);
            }
            return value;
        }

        public static NSJSValue UndefinedMerge(NSJSVirtualMachine machine, NSJSValue value)
        {
            if (value == null)
            {
                return NSJSValue.Undefined(machine);
            }
            return value;
        }

        private static NSJSValue NewNullOrUndfined(NSJSVirtualMachine machine, bool undefined)
        {
            if (machine == null)
            {
                throw new InvalidOperationException("machine");
            }
            IntPtr isolate = machine.Isolate;
            if (isolate == NULL)
            {
                throw new InvalidOperationException("machine");
            }
            return undefined ? machine.Undefined() : machine.Null();
        }
    }
}
