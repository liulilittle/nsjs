namespace nsjsdotnet
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public class NSJSDateTime : NSJSValue
    {
        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_localvalue_datetime_new(IntPtr isolate, long value);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static long nsjs_localvalue_get_int64(IntPtr localValue);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private object value;

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

        private static readonly long Timestamp = NSJSDateTime.Min.Ticks;

        public static DateTime Min = new DateTime(1970, 1, 1, 8, 0, 0);

        internal NSJSDateTime(IntPtr handle, NSJSVirtualMachine machine) : base(handle, NSJSValueType.kDateTime, machine)
        {

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
            return dateTime.Ticks < Timestamp;
        }

        public static long DateTimeToLocalDate(DateTime datetime)
        {
            DateTime d2 = datetime.ToUniversalTime();
            TimeSpan ts = new TimeSpan(d2.Ticks - Timestamp);
            return (long)ts.TotalMilliseconds;
        }

        public static DateTime LocalDateToDateTime(long millitime)
        {
            DateTime datetime = new DateTime((Timestamp + millitime * 10000));
            return datetime;
        }

        public static NSJSDateTime New(NSJSVirtualMachine machine, long ticks)
        {
            return New(machine, NSJSDateTime.LocalDateToDateTime(ticks));
        }

        public static NSJSDateTime New(NSJSVirtualMachine machine, DateTime value)
        {
            if (NSJSDateTime.Invalid(value))
            {
                throw new ArgumentNullException("Parameter cannot be an invalid time");
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
    }
}
