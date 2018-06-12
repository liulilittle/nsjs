namespace nsjsdotnet.Core
{
    using System;
    using System.Globalization;

    public static class ValueAuxiliary
    {
        public static int ToInt32(NSJSValue value)
        {
            return Convert.ToInt32(ToDouble(value));
        }

        public static uint ToUInt32(NSJSValue value)
        {
            return Convert.ToUInt32(ToDouble(value));
        }

        public static short ToInt16(NSJSValue value)
        {
            return Convert.ToInt16(ToDouble(value));
        }

        public static ushort ToUInt16(NSJSValue value)
        {
            return Convert.ToUInt16(ToDouble(value));
        }

        public static byte ToByte(NSJSValue value)
        {
            return Convert.ToByte(ToDouble(value));
        }

        public static char ToChar(NSJSValue value)
        {
            return Convert.ToChar(ToDouble(value));
        }

        public static sbyte ToSByte(NSJSValue value)
        {
            return Convert.ToSByte(ToDouble(value));
        }

        public static long ToInt64(NSJSValue value)
        {
            return Convert.ToInt64(ToDouble(value));
        }

        public static DateTime ToDateTime(NSJSValue value)
        {
            NSJSDateTime datetime = value as NSJSDateTime;
            if (datetime != null)
            {
                return datetime.Value;
            }
            return NSJSDateTime.LocalDateToDateTime(ToInt64(value));
        }

        public static float ToSingle(NSJSValue value)
        {
            return unchecked((float)ToDouble(value));
        }

        public static bool ToBoolean(NSJSValue value)
        {
            if (value == null || value.IsNullOrUndfined)
            {
                return false;
            }
            NSJSBoolean boolean = value as NSJSBoolean;
            if (boolean != null)
            {
                return boolean.Value;
            }
            NSJSObject obj = value as NSJSObject;
            if (obj != null)
            {
                return true;
            }
            return ToDouble(value) != 0;
        }

        public static double ToDouble(NSJSValue value)
        {
            if (value == null || value.IsNullOrUndfined)
            {
                return 0;
            }
            NSJSInt32 i32 = value as NSJSInt32;
            if (i32 != null)
            {
                return i32.Value;
            }
            NSJSUInt32 u32 = value as NSJSUInt32;
            if (u32 != null)
            {
                return u32.Value;
            }
            NSJSBoolean boolean = value as NSJSBoolean;
            if (boolean != null)
            {
                return boolean.Value ? 1 : 0;
            }
            NSJSDateTime time = value as NSJSDateTime;
            if (time != null)
            {
                return NSJSDateTime.DateTimeToLocalDate(time.Value);
            }
            NSJSDouble dbl = value as NSJSDouble;
            if (dbl != null)
            {
                return dbl.Value;
            }
            NSJSInt64 i64 = value as NSJSInt64;
            if (i64 != null)
            {
                return i64.Value;
            }
            NSJSString str = value as NSJSString;
            if (str != null)
            {
                double n;
                if (double.TryParse(str.Value, NumberStyles.Float | NumberStyles.None, null, out n))
                {
                    return n;
                }
            }
            return 0;
        }

        public static string AsString(NSJSValue value)
        {
            if (value == null)
            {
                return null;
            }
            if (value.IsNull)
            {
                return NSJSValue.NullString;
            }
            if (value.IsUndfined)
            {
                return NSJSValue.UndefinedString;
            }
            return ToString(value);
        }

        public static string ToString(NSJSValue value)
        {
            if (value == null)
            {
                return null;
            }
            if (value.IsNullOrUndfined)
            {
                return null;
            }
            NSJSString s = value as NSJSString;
            if (s != null)
            {
                return s.Value;
            }
            NSJSObject o = value as NSJSObject;
            if (o != null)
            {
                return NSJSJson.Stringify(o);
            }
            return NSJSString.Cast(value).Value;
        }

        public static T As<T>(this NSJSValue value)
        {
            if (value == null)
            {
                return default(T);
            }
            Type typeid = typeof(T);
            object result = null;
            if (typeid == typeof(object))
            {
                if (value.IsNullOrUndfined)
                {
                    return (T)result;
                }
                return (T)value.GetValue();
            }
            else if (typeid == typeof(string))
            {
                result = ToString(value);
            }
            else if (typeid == typeof(int))
            {
                result = ToInt32(value);
            }
            else if (typeid == typeof(uint))
            {
                result = ToUInt32(value);
            }
            else if (typeid == typeof(DateTime))
            {
                result = ToDateTime(value);
            }
            else if (typeid == typeof(bool))
            {
                result = ToBoolean(value);
            }
            else if (typeid == typeof(float))
            {
                result = ToSingle(value);
            }
            else if (typeid == typeof(char))
            {
                result = ToChar(value);
            }
            else if (typeid == typeof(double))
            {
                result = ToDouble(value);
            }
            else if (typeid == typeof(byte))
            {
                result = ToByte(value);
            }
            else if (typeid == typeof(sbyte))
            {
                result = ToSByte(value);
            }
            else if (typeid == typeof(short))
            {
                result = ToInt16(value);
            }
            else if (typeid == typeof(ushort))
            {
                result = ToUInt16(value);
            }
            else if (typeid == typeof(byte[]))
            {
                result = (value as NSJSUInt8Array)?.Buffer;
            }
            else if (typeid == typeof(sbyte[]))
            {
                result = (value as NSJSInt8Array)?.Buffer;
            }
            else if (typeid == typeof(ushort[]))
            {
                result = (value as NSJSUInt16Array)?.Buffer;
            }
            else if (typeid == typeof(short[]))
            {
                result = (value as NSJSInt16Array)?.Buffer;
            }
            else if (typeid == typeof(uint[]))
            {
                result = (value as NSJSUInt32Array)?.Buffer;
            }
            else if (typeid == typeof(int[]))
            {
                result = (value as NSJSInt32Array)?.Buffer;
            }
            else if (typeid == typeof(float[]))
            {
                result = (value as NSJSFloat32Array)?.Buffer;
            }
            else if (typeid == typeof(double[]))
            {
                result = (value as NSJSFloat64Array)?.Buffer;
            }
            if (result == null)
            {
                return default(T);
            }
            return (T)result;
        }

        public static object Value(this NSJSValue value)
        {
            if (value == null || value.IsNullOrUndfined)
            {
                return null;
            }
            if ((value.DateType & NSJSValueType.kFunction) > 0)
            {
                return null;
            }
            if ((value.DateType & NSJSValueType.kArray) > 0)
            {
                return ArrayAuxiliary.ToArray(value as NSJSArray);
            }
            return value.GetValue();
        }

        public static NSJSValue As(this object value, NSJSVirtualMachine machine)
        {
            if (machine == null)
            {
                return null;
            }
            if (value == null)
            {
                return NSJSValue.Null(machine);
            }
            Type typeid = value.GetType();
            if (typeid == typeof(int) ||
                typeid == typeof(short) ||
                typeid == typeof(sbyte) ||
                typeid == typeof(char))
            {
                return NSJSInt32.New(machine, Convert.ToInt32(value));
            }
            else if (typeid == typeof(uint) ||
                typeid == typeof(ushort) ||
                typeid == typeof(byte))
            {
                return NSJSUInt32.New(machine, Convert.ToUInt32(value));
            }
            else if (typeid == typeof(string))
            {
                return NSJSString.New(machine, value.ToString());
            }
            else if (typeid == typeof(bool))
            {
                return NSJSBoolean.New(machine, Convert.ToBoolean(value));
            }
            else if (typeid == typeof(DateTime))
            {
                DateTime datetime = Convert.ToDateTime(value);
                if (NSJSDateTime.Invalid(datetime))
                {
                    datetime = NSJSDateTime.Min;
                }
                return NSJSDateTime.New(machine, datetime);
            }
            else if (typeid == typeof(float) || typeid == typeof(double))
            {
                return NSJSDouble.New(machine, Convert.ToDouble(value));
            }
            else if (typeid == typeof(byte[]))
            {
                byte[] buffer = (byte[])(object)value;
                if (buffer == null)
                {
                    return NSJSValue.Null(machine);
                }
                return NSJSUInt8Array.New(machine, buffer);
            }
            else if (typeid == typeof(sbyte[]))
            {
                sbyte[] buffer = (sbyte[])(object)value;
                if (buffer == null)
                {
                    return NSJSValue.Null(machine);
                }
                return NSJSInt8Array.New(machine, buffer);
            }
            else if (typeid == typeof(short[]))
            {
                short[] buffer = (short[])(object)value;
                if (buffer == null)
                {
                    return NSJSValue.Null(machine);
                }
                return NSJSInt16Array.New(machine, buffer);
            }
            else if (typeid == typeof(ushort[]))
            {
                ushort[] buffer = (ushort[])(object)value;
                if (buffer == null)
                {
                    return NSJSValue.Null(machine);
                }
                return NSJSUInt16Array.New(machine, buffer);
            }
            else if (typeid == typeof(int[]))
            {
                int[] buffer = (int[])(object)value;
                if (buffer == null)
                {
                    return NSJSValue.Null(machine);
                }
                return NSJSInt32Array.New(machine, buffer);
            }
            else if (typeid == typeof(uint[]))
            {
                uint[] buffer = (uint[])(object)value;
                if (buffer == null)
                {
                    return NSJSValue.Null(machine);
                }
                return NSJSUInt32Array.New(machine, buffer);
            }
            else if (typeid == typeof(float[]))
            {
                float[] buffer = (float[])(object)value;
                if (buffer == null)
                {
                    return NSJSValue.Null(machine);
                }
                return NSJSFloat32Array.New(machine, buffer);
            }
            else if (typeid == typeof(double[]))
            {
                double[] buffer = (double[])(object)value;
                if (buffer == null)
                {
                    return NSJSValue.Null(machine);
                }
                return NSJSFloat64Array.New(machine, buffer);
            }
            return NSJSValue.Null(machine);
        }
    }
}
