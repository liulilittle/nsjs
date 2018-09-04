namespace nsjsdotnet
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public class NSJSInt64 : NSJSValue
    {
        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static long nsjs_localvalue_get_int64(IntPtr localValue);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private object value = default(object);

        public override object GetValue()
        {
            lock (this)
            {
                if (this.value == null)
                {
                    this.value = nsjs_localvalue_get_int64(this.Handle);
                }
                return this.value;
            }
        }

        internal NSJSInt64(IntPtr handle, NSJSVirtualMachine machine) : base(handle, NSJSDataType.kInt64, machine)
        {

        }

        public static NSJSInt64 New(NSJSVirtualMachine machine, long value)
        {
            return Cast(NSJSDouble.New(machine, value));
        }

        public long Value
        {
            get
            {
                return (long)this.GetValue();
            }
        }

        public override string ToString()
        {
            return this.Value.ToString();
        }

        public static NSJSInt64 Cast(NSJSValue value)
        {
            return Cast(value, (handle, machine) => new NSJSInt64(value.Handle, value.VirtualMachine));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public static bool operator ==(NSJSInt64 x, string y)
        {
            object ox = x;
            if (ox == null && y == null)
            {
                return true;
            }
            else if (ox != null && y == null || ox == null && y != null)
            {
                return false;
            }
            return unchecked(x.ToString() == y);
        }

        public static bool operator !=(NSJSInt64 x, string y)
        {
            return !(x == y);
        }

        public static NSJSInt64 operator +(NSJSInt64 x, long y)
        {
            if (x == null)
            {
                return null;
            }
            return New(x.VirtualMachine, x.Value + y);
        }

        public static NSJSInt64 operator -(NSJSInt64 x, long y)
        {
            if (x == null)
            {
                return null;
            }
            return New(x.VirtualMachine, x.Value - y);
        }

        public static NSJSInt64 operator /(NSJSInt64 x, long y)
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

        public static NSJSInt64 operator *(NSJSInt64 x, long y)
        {
            if (x == null)
            {
                return null;
            }
            return New(x.VirtualMachine, x.Value * y);
        }

        public static NSJSInt64 operator %(NSJSInt64 x, long y)
        {
            if (x == null)
            {
                return null;
            }
            return New(x.VirtualMachine, x.Value % y);
        }

        public static NSJSInt64 operator >>(NSJSInt64 x, int y)
        {
            if (x == null)
            {
                return null;
            }
            return New(x.VirtualMachine, x.Value >> y);
        }

        public static NSJSInt64 operator <<(NSJSInt64 x, int y)
        {
            if (x == null)
            {
                return null;
            }
            return New(x.VirtualMachine, x.Value << y);
        }

        public static NSJSInt64 operator &(NSJSInt64 x, long y)
        {
            if (x == null)
            {
                return null;
            }
            return New(x.VirtualMachine, x.Value & y);
        }

        public static NSJSInt64 operator ^(NSJSInt64 x, long y)
        {
            if (x == null)
            {
                return null;
            }
            return New(x.VirtualMachine, x.Value ^ y);
        }

        public static NSJSInt64 operator |(NSJSInt64 x, long y)
        {
            if (x == null)
            {
                return null;
            }
            return New(x.VirtualMachine, x.Value | y);
        }

        public static NSJSInt64 operator ~(NSJSInt64 x)
        {
            if (x == null)
            {
                return null;
            }
            return New(x.VirtualMachine, ~x.Value);
        }

        public static implicit operator double(NSJSInt64 x)
        {
            return x == null ? double.NaN : x.Value;
        }

        public static implicit operator long(NSJSInt64 x)
        {
            return x == null ? 0 : x.Value;
        }

        public static implicit operator decimal(NSJSInt64 x)
        {
            return x == null ? 0 : x.Value;
        }

        public static explicit operator int(NSJSInt64 x)
        {
            return x == null ? 0 : Convert.ToInt32(x.Value);
        }

        public static explicit operator uint(NSJSInt64 x)
        {
            uint r = 0;
            if (x != null)
            {
                r = unchecked((uint)x.Value);
            }
            return r;
        }

        public static explicit operator ulong(NSJSInt64 x)
        {
            ulong r = 0;
            if (x != null)
            {
                r = unchecked((ulong)x.Value);
            }
            return r;
        }

        public static explicit operator byte(NSJSInt64 x)
        {
            byte r = 0;
            if (x != null)
            {
                r = Convert.ToByte(x.Value);
            }
            return r;
        }

        public static explicit operator sbyte(NSJSInt64 x)
        {
            sbyte r = 0;
            if (x != null)
            {
                r = Convert.ToSByte(x.Value);
            }
            return r;
        }

        public static explicit operator short(NSJSInt64 x)
        {
            short r = 0;
            if (x != null)
            {
                r = Convert.ToInt16(x.Value);
            }
            return r;
        }

        public static explicit operator ushort(NSJSInt64 x)
        {
            ushort r = 0;
            if (x != null)
            {
                r = Convert.ToUInt16(x.Value);
            }
            return r;
        }

        public static explicit operator float(NSJSInt64 x)
        {
            return x == null ? float.NaN : x.Value;
        }

        public static implicit operator bool(NSJSInt64 x)
        {
            if (x == null)
            {
                return false;
            }
            return x.Value != 0;
        }

        public static explicit operator string(NSJSInt64 x)
        {
            if (x == null)
            {
                return null;
            }
            return x.ToString();
        }

        public static explicit operator char(NSJSInt64 x)
        {
            if (x == null)
            {
                return '\0';
            }
            return Convert.ToChar(x.Value);
        }

        public static explicit operator DateTime(NSJSInt64 x)
        {
            if (x == null)
            {
                return NSJSDateTime.Min;
            }
            return NSJSDateTime.LocalDateToDateTime(x.Value);
        }

        public static explicit operator NSJSDateTime(NSJSInt64 x)
        {
            if (x == null)
            {
                return null;
            }
            return NSJSDateTime.New(x.VirtualMachine, x.Value);
        }

        public static explicit operator NSJSDouble(NSJSInt64 x)
        {
            if (x == null)
            {
                return null;
            }
            return NSJSDouble.New(x.VirtualMachine, x.Value);
        }

        public static explicit operator NSJSUInt32(NSJSInt64 x)
        {
            if (x == null)
            {
                return null;
            }
            return NSJSUInt32.New(x.VirtualMachine, unchecked((uint)x.Value));
        }

        public static explicit operator NSJSInt32(NSJSInt64 x)
        {
            if (x == null)
            {
                return null;
            }
            return NSJSInt32.New(x.VirtualMachine, Convert.ToInt32(x.Value));
        }

        public static explicit operator NSJSBoolean(NSJSInt64 x)
        {
            if (x == null)
            {
                return null;
            }
            return NSJSBoolean.New(x.VirtualMachine, x.Value != 0);
        }

        public static explicit operator NSJSString(NSJSInt64 x)
        {
            if (x == null)
            {
                return null;
            }
            return NSJSString.New(x.VirtualMachine, x.ToString());
        }
    }
}
