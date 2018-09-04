namespace nsjsdotnet
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public class NSJSDateTime : NSJSValue
    {
        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_localvalue_datetime_new(IntPtr isolate, long value);

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
                    this.value = NSJSDateTime.LocalDateToDateTime(nsjs_localvalue_get_int64(this.Handle));
                }
            }
            return this.value;
        }

        public static DateTime Min = new DateTime(1970, 1, 1, 8, 0, 0, DateTimeKind.Utc);

        internal NSJSDateTime(IntPtr handle, NSJSVirtualMachine machine) : base(handle, NSJSDataType.kDateTime, machine)
        {

        }

        public static NSJSDateTime Cast(NSJSValue value)
        {
            return Cast(value, (handle, machine) => new NSJSDateTime(value.Handle, value.VirtualMachine));
        }

        public DateTime Value
        {
            get
            {
                return (DateTime)this.GetValue();
            }
        }

        public static bool Invalid(DateTime dateTime)
        {
            return dateTime.Ticks < Min.Ticks;
        }

        public static long DateTimeToLocalDate(DateTime datetime)
        {
            TimeSpan ts = new TimeSpan(datetime.Ticks - Min.Ticks);
            return unchecked((long)ts.TotalMilliseconds);
        }

        public static DateTime LocalDateToDateTime(long ticks)
        {
            return new DateTime(unchecked(Min.Ticks + ticks * 10000));
        }

        public static NSJSDateTime New(NSJSVirtualMachine machine, long ticks)
        {
            return NSJSDateTime.New(machine, NSJSDateTime.LocalDateToDateTime(ticks));
        }

        public static NSJSDateTime New(NSJSVirtualMachine machine, DateTime value)
        {
            if (NSJSDateTime.Invalid(value))
            {
                throw new ArgumentOutOfRangeException("Parameter cannot be an invalid time");
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
            long time = NSJSDateTime.DateTimeToLocalDate(value);
            IntPtr handle = nsjs_localvalue_datetime_new(isolate, time);
            if (handle == NULL)
            {
                throw new InvalidOperationException("machine");
            }
            return new NSJSDateTime(handle, machine);
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

        public static bool operator ==(NSJSDateTime x, DateTime y)
        {
            long ticks = NSJSDateTime.DateTimeToLocalDate(y);
            if (x == null)
            {
                return ticks == 0;
            }
            else if (ticks < 0)
            {
                return false;
            }
            return unchecked(x.Value == y);
        }

        public static bool operator !=(NSJSDateTime x, DateTime y)
        {
            return !(x == y);
        }

        public static bool operator ==(NSJSDateTime x, long y)
        {
            object ox = x;
            if (ox == null)
            {
                return y == 0;
            }
            else if (y < 0)
            {
                return false;
            }
            return ((long)x) == y;
        }

        public static bool operator !=(NSJSDateTime x, long y)
        {
            return !(x == y);
        }

        public static implicit operator long(NSJSDateTime x)
        {
            long r = 0;
            if (x != null)
            {
                r = NSJSDateTime.DateTimeToLocalDate(x.Value);
            }
            return r;
        }

        public static implicit operator double(NSJSDateTime x)
        {
            if (x == null)
            {
                return double.NaN;
            }
            return unchecked((long)x);
        }

        public static implicit operator decimal(NSJSDateTime x)
        {
            if (x == null)
            {
                return 0;
            }
            return unchecked((long)x);
        }

        public static implicit operator DateTime(NSJSDateTime x)
        {
            if (x == null)
            {
                return NSJSDateTime.Min;
            }
            return x.Value;
        }

        public static explicit operator int(NSJSDateTime x)
        {
            int r = 0;
            if (x != null)
            {
                r = Convert.ToInt32((long)x);
            }
            return r;
        }

        public static explicit operator float(NSJSDateTime x)
        {
            if (x == null)
            {
                return float.NaN;
            }
            return Convert.ToSingle(unchecked((long)x));
        }

        public static explicit operator uint(NSJSDateTime x)
        {
            uint r = 0;
            if (x != null)
            {
                r = Convert.ToUInt32((long)x);
            }
            return r;
        }

        public static explicit operator ulong(NSJSDateTime x)
        {
            ulong r = 0;
            if (x != null)
            {
                r = unchecked((ulong)((long)x));
            }
            return r;
        }

        public static NSJSDateTime operator +(NSJSDateTime x, long y)
        {
            if (x == null)
            {
                return null;
            }
            long ticks = unchecked((long)x) + y;
            if (ticks < 0)
            {
                ticks = 0;
            }
            return NSJSDateTime.New(x.VirtualMachine, ticks);
        }

        public static NSJSDateTime operator -(NSJSDateTime x, long y)
        {
            if (x == null)
            {
                return null;
            }
            long ticks = unchecked((long)x) - y;
            if (ticks < 0)
            {
                ticks = 0;
            }
            return NSJSDateTime.New(x.VirtualMachine, ticks);
        }
    }
}
