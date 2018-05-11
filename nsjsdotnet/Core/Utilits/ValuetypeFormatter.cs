namespace nsjsdotnet.Core.Utilits
{
    using System;
    using System.Globalization;
    using System.Reflection;

    public static class ValuetypeFormatter
    {
        public static object Parse(this string value, Type type, NumberStyles style = NumberStyles.Number | NumberStyles.Float, IFormatProvider provider = null)
        {
            if (type == null)
            {
                return null;
            }
            if (type == typeof(string))
            {
                return value;
            }
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            //
            MethodInfo met = type.GetMethod("TryParse", new Type[] { typeof(string), typeof(NumberStyles), typeof(IFormatProvider), type.MakeByRefType() });
            object[] args = null;
            if (met == null)
            {
                met = type.GetMethod("TryParse", new Type[] { typeof(string), type.MakeByRefType() });
                if (met == null)
                {
                    return null;
                }
                if (type == typeof(bool) && value == "1")
                {
                    value = "True";
                }
                args = new object[] { value, null };
            }
            else
            {
                args = new object[] { value, style, provider, null };
            }
            if (args == null)
            {
                return null;
            }
            if ((true).Equals(met.Invoke(null, args)))
            {
                return args[args.Length - 1];
            }
            return null;
        }

        public static T? Parse<T>(this string value, NumberStyles style = NumberStyles.Number | NumberStyles.Float, IFormatProvider provider = null) where T : struct
        {
            object o = ValuetypeFormatter.Parse(value, typeof(T), style, provider);
            if (o == null)
            {
                return null;
            }
            return (T)o;
        }

        public static string To(this object value)
        {
            return Convert.ToString(value);
        }

        public static string To<T>(this T value)
        {
            return Convert.ToString(value);
        }

        public static T TryParse<T>(this string value, NumberStyles style = NumberStyles.Number | NumberStyles.Float, IFormatProvider provider = null) where T : struct
        {
            T? result = Parse<T>(value);
            if (result == null)
            {
                return default(T);
            }
            return (T)result;
        }
    }
}
