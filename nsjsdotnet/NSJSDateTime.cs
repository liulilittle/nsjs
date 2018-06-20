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

        public static DateTime Min = new DateTime(1970, 1, 1, 8, 0, 0, DateTimeKind.Utc);

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
            return dateTime.Ticks < Min.Ticks;
        }

        public static long DateTimeToLocalDate(DateTime datetime)
        {
            TimeSpan ts = new TimeSpan(datetime.Ticks - Min.Ticks);
            return unchecked((long)ts.TotalMilliseconds);
        }

        public static DateTime LocalDateToDateTime(long millitime)
        {
            DateTime dt = new DateTime(Min.Ticks + millitime * 10000);
            return dt;
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
