namespace nsjsdotnet
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public class NSJSDouble : NSJSValue
    {
        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_localvalue_float64_new(IntPtr isolate, double value);

        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static double nsjs_localvalue_get_float64(IntPtr localValue);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private object value = default(object);

        public override object GetValue()
        {
            lock (this)
            {
                if (this.value == null)
                {
                    this.value = nsjs_localvalue_get_float64(this.Handle);
                }
            }
            return this.value;
        }

        internal NSJSDouble(IntPtr handle, NSJSVirtualMachine machine) : base(handle, NSJSDataType.kDouble, machine)
        {

        }

        public static NSJSDouble Cast(NSJSValue value)
        {
            return Cast(value, (handle, machine) => new NSJSDouble(value.Handle, value.VirtualMachine));
        }

        public double Value
        {
            get
            {
                return (double)this.GetValue();
            }
        }

        public static NSJSDouble New(NSJSVirtualMachine machine, double value)
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
            IntPtr handle = nsjs_localvalue_float64_new(isolate, value);
            if (handle == NULL)
            {
                throw new InvalidOperationException("machine");
            }
            return new NSJSDouble(handle, machine);
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

        public static bool operator ==(NSJSDouble x, string y)
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

        public static bool operator !=(NSJSDouble x, string y)
        {
            return !(x == y);
        }

        public static NSJSDouble operator +(NSJSDouble x, double y)
        {
            if (x == null)
            {
                return null;
            }
            return New(x.VirtualMachine, x.Value + y);
        }

        public static NSJSDouble operator -(NSJSDouble x, double y)
        {
            if (x == null)
            {
                return null;
            }
            return New(x.VirtualMachine, x.Value - y);
        }

        public static NSJSDouble operator /(NSJSDouble x, double y)
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

        public static NSJSDouble operator *(NSJSDouble x, double y)
        {
            if (x == null)
            {
                return null;
            }
            return New(x.VirtualMachine, x.Value * y);
        }

        public static NSJSDouble operator %(NSJSDouble x, double y)
        {
            if (x == null)
            {
                return null;
            }
            return New(x.VirtualMachine, x.Value % y);
        }

        public static implicit operator double(NSJSDouble x)
        {
            return x == null ? double.NaN : x.Value;
        }

        public static explicit operator int(NSJSDouble x)
        {
            return x == null ? 0 : Convert.ToInt32(x.Value);
        }

        public static explicit operator long(NSJSDouble x)
        {
            return x == null ? 0 : Convert.ToInt64(x.Value);
        }

        public static explicit operator decimal(NSJSDouble x)
        {
            return x == null ? 0 : Convert.ToDecimal(x.Value);
        }

        public static explicit operator uint(NSJSDouble x)
        {
            uint r = 0;
            if (x != null)
            {
                r = unchecked((uint)x.Value);
            }
            return r;
        }

        public static explicit operator ulong(NSJSDouble x)
        {
            ulong r = 0;
            if (x != null)
            {
                r = unchecked((ulong)x.Value);
            }
            return r;
        }

        public static explicit operator byte(NSJSDouble x)
        {
            byte r = 0;
            if (x != null)
            {
                r = Convert.ToByte(x.Value);
            }
            return r;
        }

        public static explicit operator sbyte(NSJSDouble x)
        {
            sbyte r = 0;
            if (x != null)
            {
                r = Convert.ToSByte(x.Value);
            }
            return r;
        }

        public static explicit operator short(NSJSDouble x)
        {
            short r = 0;
            if (x != null)
            {
                r = Convert.ToInt16(x.Value);
            }
            return r;
        }

        public static explicit operator ushort(NSJSDouble x)
        {
            ushort r = 0;
            if (x != null)
            {
                r = Convert.ToUInt16(x.Value);
            }
            return r;
        }

        public static explicit operator float(NSJSDouble x)
        {
            return x == null ? float.NaN : Convert.ToSingle(x.Value);
        }

        public static implicit operator bool(NSJSDouble x)
        {
            if (x == null)
            {
                return false;
            }
            return x.Value != 0;
        }

        public static explicit operator string(NSJSDouble x)
        {
            if (x == null)
            {
                return null;
            }
            return x.ToString();
        }

        public static explicit operator char(NSJSDouble x)
        {
            if (x == null)
            {
                return '\0';
            }
            return Convert.ToChar(x.Value);
        }
    }
}
