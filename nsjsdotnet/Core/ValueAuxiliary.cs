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

        public static decimal ToDecimal(NSJSValue value)
        {
            return Convert.ToDecimal(ToDouble(value));
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

        public static ulong ToUInt64(NSJSValue value)
        {
            return unchecked((ulong)ToInt64(value));
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
            Type type = typeof(T);
            object out_ = null;
            if (type == typeof(object))
            {
                if (value.IsNullOrUndfined)
                {
                    return (T)out_;
                }
                return (T)value.GetValue();
            }
            else if (type == typeof(string))
            {
                out_ = ToString(value);
            }
            else if (type == typeof(int))
            {
                out_ = ToInt32(value);
            }
            else if (type == typeof(uint))
            {
                out_ = ToUInt32(value);
            }
            else if (type == typeof(DateTime))
            {
                out_ = ToDateTime(value);
            }
            else if (type == typeof(bool))
            {
                out_ = ToBoolean(value);
            }
            else if (type == typeof(float))
            {
                out_ = ToSingle(value);
            }
            else if (type == typeof(char))
            {
                out_ = ToChar(value);
            }
            else if (type == typeof(double))
            {
                out_ = ToDouble(value);
            }
            else if (type == typeof(byte))
            {
                out_ = ToByte(value);
            }
            else if (type == typeof(sbyte))
            {
                out_ = ToSByte(value);
            }
            else if (type == typeof(short))
            {
                out_ = ToInt16(value);
            }
            else if (type == typeof(ushort))
            {
                out_ = ToUInt16(value);
            }
            else if (type == typeof(byte[]))
            {
                out_ = (value as NSJSUInt8Array)?.Buffer;
            }
            else if (type == typeof(sbyte[]))
            {
                out_ = (value as NSJSInt8Array)?.Buffer;
            }
            else if (type == typeof(ushort[]))
            {
                out_ = (value as NSJSUInt16Array)?.Buffer;
            }
            else if (type == typeof(short[]))
            {
                out_ = (value as NSJSInt16Array)?.Buffer;
            }
            else if (type == typeof(uint[]))
            {
                out_ = (value as NSJSUInt32Array)?.Buffer;
            }
            else if (type == typeof(int[]))
            {
                out_ = (value as NSJSInt32Array)?.Buffer;
            }
            else if (type == typeof(float[]))
            {
                out_ = (value as NSJSFloat32Array)?.Buffer;
            }
            else if (type == typeof(double[]))
            {
                out_ = (value as NSJSFloat64Array)?.Buffer;
            }
            else if (type.IsInstanceOfType(value))
            {
                return (T)((object)value);
            }
            return out_ == null ? default(T) : (T)out_;
        }

        public static object Value(this NSJSValue value)
        {
            if (value == null || value.IsNullOrUndfined)
            {
                return null;
            }
            if ((value.DateType & NSJSDataType.kFunction) > 0 ||
                (value.DateType & NSJSDataType.kArray) > 0 ||
                (value.DateType & NSJSDataType.kObject) > 0)
            {
                return value;
            }
            return value.GetValue();
        }

        public static T FetchValue<T>(this NSJSValue value)
        {
            return (T)FetchValue(value, typeof(T));
        }

        public static object FetchValue(this NSJSValue value, Type type)
        {
            return NSJSValueMetaObject.FetchValue(type, value);
        }

        public static T ConvertValue<T>(this NSJSValue value)
        {
            return (T)ConvertValue(value, typeof(T));
        }

        public static object ConvertValue(this NSJSValue value, Type type)
        {
            return NSJSValueMetaObject.ConvertValue(type, value);
        }

        public static NSJSValue ToValue(this object obj, NSJSVirtualMachine machine)
        {
            return NSJSValueMetaObject.ConvertValue(machine, obj);
        }

        public static NSJSValue As(this object value, NSJSVirtualMachine machine)
        {
            if (machine == null)
            {
                return null;
            }
            if (value == null || value == DBNull.Value)
            {
                return NSJSValue.Null(machine);
            }
            if (value is NSJSValue)
            {
                return value as NSJSValue;
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
