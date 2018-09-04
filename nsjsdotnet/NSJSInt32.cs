namespace nsjsdotnet
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public class NSJSInt32 : NSJSValue
    {
        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_localvalue_int32_new(IntPtr isolate, int value);

        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static int nsjs_localvalue_get_int32(IntPtr localValue);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private object value;

        public override object GetValue()
        {
            lock (this)
            {
                if (this.value == null)
                {
                    this.value = nsjs_localvalue_get_int32(this.Handle);
                }
                return this.value;
            }
        }

        internal NSJSInt32(IntPtr handle, NSJSVirtualMachine machine) : base(handle, NSJSDataType.kInt32, machine)
        {

        }

        public int Value
        {
            get
            {
                return (int)this.GetValue();
            }
        }

        public static NSJSInt32 New(NSJSVirtualMachine machine, int value)
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
            IntPtr handle = nsjs_localvalue_int32_new(isolate, value);
            if (handle == NULL)
            {
                throw new InvalidOperationException("machine");
            }
            return new NSJSInt32(handle, machine);
        }

        public static NSJSInt32 Cast(NSJSValue value)
        {
            return Cast(value, (handle, machine) => new NSJSInt32(value.Handle, value.VirtualMachine));
        }

        public override string ToString()
        {
            return this.Value.ToString();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(NSJSInt32 x, string y)
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

        public static bool operator !=(NSJSInt32 x, string y)
        {
            return !(x == y);
        }

        public static NSJSInt32 operator +(NSJSInt32 x, int y)
        {
            if (x == null)
            {
                return null;
            }
            return NSJSInt32.New(x.VirtualMachine, x.Value + y);
        }

        public static NSJSInt32 operator -(NSJSInt32 x, int y)
        {
            if (x == null)
            {
                return null;
            }
            return NSJSInt32.New(x.VirtualMachine, x.Value - y);
        }

        public static NSJSInt32 operator /(NSJSInt32 x, int y)
        {
            if (x == null)
            {
                return null;
            }
            int n = x.Value;
            if (n != 0 && y != 0)
            {
                n /= y;
            }
            else
            {
                n = 0;
            }
            return NSJSInt32.New(x.VirtualMachine, n);
        }

        public static NSJSInt32 operator *(NSJSInt32 x, int y)
        {
            if (x == null)
            {
                return null;
            }
            return NSJSInt32.New(x.VirtualMachine, x.Value * y);
        }

        public static NSJSInt32 operator %(NSJSInt32 x, int y)
        {
            if (x == null)
            {
                return null;
            }
            return NSJSInt32.New(x.VirtualMachine, x.Value % y);
        }

        public static NSJSInt32 operator >>(NSJSInt32 x, int y)
        {
            if (x == null)
            {
                return null;
            }
            return NSJSInt32.New(x.VirtualMachine, x.Value >> y);
        }

        public static NSJSInt32 operator <<(NSJSInt32 x, int y)
        {
            if (x == null)
            {
                return null;
            }
            return NSJSInt32.New(x.VirtualMachine, x.Value << y);
        }

        public static NSJSInt32 operator &(NSJSInt32 x, int y)
        {
            if (x == null)
            {
                return null;
            }
            return NSJSInt32.New(x.VirtualMachine, x.Value & y);
        }

        public static NSJSInt32 operator ^(NSJSInt32 x, int y)
        {
            if (x == null)
            {
                return null;
            }
            return NSJSInt32.New(x.VirtualMachine, x.Value ^ y);
        }

        public static NSJSInt32 operator |(NSJSInt32 x, int y)
        {
            if (x == null)
            {
                return null;
            }
            return NSJSInt32.New(x.VirtualMachine, x.Value | y);
        }

        public static NSJSInt32 operator ~(NSJSInt32 x)
        {
            if (x == null)
            {
                return null;
            }
            return NSJSInt32.New(x.VirtualMachine, ~x.Value);
        }

        public static implicit operator int(NSJSInt32 x)
        {
            return x == null ? default(int) : x.Value;
        }

        public static implicit operator long(NSJSInt32 x)
        {
            return x == null ? default(long) : x.Value;
        }

        public static implicit operator decimal(NSJSInt32 x)
        {
            decimal r = 0;
            if (x != null)
            {
                r = x.Value;
            }
            return r;
        }

        public static implicit operator double(NSJSInt32 x)
        {
            if (x == null)
            {
                return double.NaN;
            }
            return x.Value;
        }

        public static explicit operator uint(NSJSInt32 x)
        {
            uint r = 0;
            if (x != null)
            {
                r = unchecked((uint)x.Value);
            }
            return r;
        }

        public static explicit operator ulong(NSJSInt32 x)
        {
            ulong r = 0;
            if (x != null)
            {
                r = unchecked((ulong)x.Value);
            }
            return r;
        }

        public static explicit operator byte(NSJSInt32 x)
        {
            byte r = 0;
            if (x != null)
            {
                r = Convert.ToByte(x.Value);
            }
            return r;
        }

        public static explicit operator sbyte(NSJSInt32 x)
        {
            sbyte r = 0;
            if (x != null)
            {
                r = Convert.ToSByte(x.Value);
            }
            return r;
        }

        public static explicit operator short(NSJSInt32 x)
        {
            short r = 0;
            if (x != null)
            {
                r = Convert.ToInt16(x.Value);
            }
            return r;
        }

        public static explicit operator ushort(NSJSInt32 x)
        {
            ushort r = 0;
            if (x != null)
            {
                r = Convert.ToUInt16(x.Value);
            }
            return r;
        }

        public static explicit operator float(NSJSInt32 x)
        {
            if (x == null)
            {
                return float.NaN;
            }
            return x.Value;
        }

        public static implicit operator bool(NSJSInt32 x)
        {
            if (x == null)
            {
                return false;
            }
            return x.Value != 0;
        }

        public static explicit operator string(NSJSInt32 x)
        {
            if (x == null)
            {
                return null;
            }
            return x.ToString();
        }

        public static explicit operator char(NSJSInt32 x)
        {
            if (x == null)
            {
                return '\0';
            }
            return Convert.ToChar(x.Value);
        }

        public static explicit operator DateTime(NSJSInt32 x)
        {
            if (x == null)
            {
                return NSJSDateTime.Min;
            }
            return NSJSDateTime.LocalDateToDateTime(x.Value);
        }

        public static explicit operator NSJSDateTime(NSJSInt32 x)
        {
            if (x == null)
            {
                return null;
            }
            return NSJSDateTime.New(x.VirtualMachine, x.Value);
        }

        public static explicit operator NSJSDouble(NSJSInt32 x)
        {
            if (x == null)
            {
                return null;
            }
            return NSJSDouble.New(x.VirtualMachine, x.Value);
        }

        public static explicit operator NSJSUInt32(NSJSInt32 x)
        {
            if (x == null)
            {
                return null;
            }
            return NSJSUInt32.New(x.VirtualMachine, unchecked((uint)x.Value));
        }
    }
}
