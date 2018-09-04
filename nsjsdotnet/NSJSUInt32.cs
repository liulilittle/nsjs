namespace nsjsdotnet
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public class NSJSUInt32 : NSJSValue
    {
        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_localvalue_uint32_new(IntPtr isolate, uint value);

        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static uint nsjs_localvalue_get_uint32(IntPtr localValue);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private object value = default(object);

        public override object GetValue()
        {
            lock (this)
            {
                if (this.value == null)
                {
                    this.value = nsjs_localvalue_get_uint32(this.Handle);
                }
                return this.value;
            }
        }

        internal NSJSUInt32(IntPtr handle, NSJSVirtualMachine machine) : base(handle, NSJSDataType.kUInt32, machine)
        {

        }

        public uint Value
        {
            get
            {
                return (uint)this.GetValue();
            }
        }

        public static NSJSUInt32 New(NSJSVirtualMachine machine, uint value)
        {
            if (machine == null)
            {
                throw new ArgumentNullException("machine");
            }
            IntPtr isolate = machine.Isolate;
            if (isolate == NULL)
            {
                throw new InvalidOperationException("machine");
            }
            IntPtr handle = nsjs_localvalue_uint32_new(isolate, value);
            if (handle == NULL)
            {
                throw new InvalidOperationException("machine");
            }
            return new NSJSUInt32(handle, machine);
        }

        public static NSJSUInt32 Cast(NSJSValue value)
        {
            return Cast(value, (handle, machine) => new NSJSUInt32(value.Handle, value.VirtualMachine));
        }

        public static NSJSUInt32 operator +(NSJSUInt32 x, uint y)
        {
            if (x == null)
            {
                return null;
            }
            return New(x.VirtualMachine, x.Value + y);
        }

        public static NSJSUInt32 operator -(NSJSUInt32 x, uint y)
        {
            if (x == null)
            {
                return null;
            }
            return New(x.VirtualMachine, x.Value - y);
        }

        public static NSJSUInt32 operator /(NSJSUInt32 x, uint y)
        {
            if (x == null)
            {
                return null;
            }
            var n = x.Value;
            if (n != 0 && y != 0)
            {
                n /= y;
            }
            else
            {
                n = 0;
            }
            return New(x.VirtualMachine, n);
        }

        public static NSJSUInt32 operator *(NSJSUInt32 x, uint y)
        {
            if (x == null)
            {
                return null;
            }
            return New(x.VirtualMachine, x.Value * y);
        }

        public static NSJSUInt32 operator %(NSJSUInt32 x, uint y)
        {
            if (x == null)
            {
                return null;
            }
            return New(x.VirtualMachine, x.Value % y);
        }

        public static NSJSUInt32 operator >>(NSJSUInt32 x, int y)
        {
            if (x == null)
            {
                return null;
            }
            return New(x.VirtualMachine, x.Value >> y);
        }

        public static NSJSUInt32 operator <<(NSJSUInt32 x, int y)
        {
            if (x == null)
            {
                return null;
            }
            return New(x.VirtualMachine, x.Value << y);
        }

        public static NSJSUInt32 operator &(NSJSUInt32 x, uint y)
        {
            if (x == null)
            {
                return null;
            }
            return New(x.VirtualMachine, x.Value & y);
        }

        public static NSJSUInt32 operator ^(NSJSUInt32 x, uint y)
        {
            if (x == null)
            {
                return null;
            }
            return New(x.VirtualMachine, x.Value ^ y);
        }

        public static NSJSUInt32 operator |(NSJSUInt32 x, uint y)
        {
            if (x == null)
            {
                return null;
            }
            return New(x.VirtualMachine, x.Value | y);
        }

        public static NSJSUInt32 operator ~(NSJSUInt32 x)
        {
            if (x == null)
            {
                return null;
            }
            return New(x.VirtualMachine, ~x.Value);
        }

        public static explicit operator int(NSJSUInt32 x)
        {
            return x == null ? 0 : Convert.ToInt32(x.Value);
        }

        public static implicit operator uint(NSJSUInt32 x)
        {
            uint r = 0;
            if (x != null)
            {
                r = unchecked((uint)x.Value);
            }
            return r;
        }

        public static implicit operator bool(NSJSUInt32 x)
        {
            if (x == null)
            {
                return false;
            }
            return x.Value != 0;
        }

        public static explicit operator ulong(NSJSUInt32 x)
        {
            ulong r = 0;
            if (x != null)
            {
                r = unchecked((ulong)x.Value);
            }
            return r;
        }

        public static explicit operator byte(NSJSUInt32 x)
        {
            byte r = 0;
            if (x != null)
            {
                r = Convert.ToByte(x.Value);
            }
            return r;
        }

        public static explicit operator sbyte(NSJSUInt32 x)
        {
            sbyte r = 0;
            if (x != null)
            {
                r = Convert.ToSByte(x.Value);
            }
            return r;
        }

        public static explicit operator short(NSJSUInt32 x)
        {
            short r = 0;
            if (x != null)
            {
                r = Convert.ToInt16(x.Value);
            }
            return r;
        }

        public static explicit operator ushort(NSJSUInt32 x)
        {
            ushort r = 0;
            if (x != null)
            {
                r = Convert.ToUInt16(x.Value);
            }
            return r;
        }

        public static explicit operator float(NSJSUInt32 x)
        {
            return x == null ? float.NaN : x.Value;
        }

        public static explicit operator string(NSJSUInt32 x)
        {
            if (x == null)
            {
                return null;
            }
            return x.ToString();
        }

        public static explicit operator char(NSJSUInt32 x)
        {
            if (x == null)
            {
                return '\0';
            }
            return Convert.ToChar(x.Value);
        }

        public static explicit operator DateTime(NSJSUInt32 x)
        {
            if (x == null)
            {
                return NSJSDateTime.Min;
            }
            return NSJSDateTime.LocalDateToDateTime(x.Value);
        }

        public static explicit operator NSJSDateTime(NSJSUInt32 x)
        {
            if (x == null)
            {
                return null;
            }
            return NSJSDateTime.New(x.VirtualMachine, x.Value);
        }

        public static explicit operator NSJSDouble(NSJSUInt32 x)
        {
            if (x == null)
            {
                return null;
            }
            return NSJSDouble.New(x.VirtualMachine, x.Value);
        }

        public static explicit operator NSJSInt64(NSJSUInt32 x)
        {
            if (x == null)
            {
                return null;
            }
            return NSJSInt64.New(x.VirtualMachine, x.Value);
        }

        public static explicit operator NSJSInt32(NSJSUInt32 x)
        {
            if (x == null)
            {
                return null;
            }
            return NSJSInt32.New(x.VirtualMachine, Convert.ToInt32(x.Value));
        }

        public static explicit operator NSJSBoolean(NSJSUInt32 x)
        {
            if (x == null)
            {
                return null;
            }
            return NSJSBoolean.New(x.VirtualMachine, x.Value != 0);
        }
    }
}
