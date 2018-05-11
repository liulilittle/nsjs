namespace nsjsdotnet.Core.Utilits
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Text;

    public static partial class TypeTool
    {
        private static IList<Type> m_numberType = new Type[] {
                                                      typeof(long),
                                                      typeof(ulong),
                                                      typeof(byte),
                                                      typeof(sbyte),
                                                      typeof(short),
                                                      typeof(ushort),
                                                      typeof(char),
                                                      typeof(decimal),
                                                      typeof(int),
                                                      typeof(uint),
        };

        private static IList<Type> m_floatType = new Type[] {
                  typeof(double),
                  typeof(float)
        };

        public static Type GetArrayElement(Type array)
        {
            if (array.IsArray)
            {
                return array.GetElementType();
            }
            if (IsList(array))
            {
                Type[] args = array.GetGenericArguments();
                return args[0];
            }
            return null;
        }

        public static bool IsBuffer(Type clazz)
        {
            return clazz == typeof(byte[]);
        }

        public static bool IsList(Type clazz)
        {
            if (clazz == null)
            {
                return false;
            }
            if (clazz.IsArray)
            {
                return true;
            }
            return clazz.IsGenericType && (typeof(IList<>).GUID == clazz.GUID || typeof(List<>).GUID == clazz.GUID);
        }

        public static bool IsNumber(Type type)
        {
            if (type == null)
            {
                return false;
            }
            for (int i = 0; i < m_numberType.Count; i++)
            {
                if (m_numberType[i] == type)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsString(Type type)
        {
            return typeof(string) == type;
        }

        public static bool IsBasicType(Type type)
        {
            return type == typeof(IPAddress) || TypeTool.IsDateTime(type) || TypeTool.IsNumber(type)
                || TypeTool.IsString(type) || TypeTool.IsFloat(type);
        }

        public static bool IsNumber(object o)
        {
            if (o == null)
            {
                return false;
            }
            if (o is TypeTool)
            {
                return TypeTool.IsNumber(o.GetType());
            }
            return false;
        }

        public static bool IsFloat(Type type)
        {
            if (type == null)
            {
                return false;
            }
            for (int i = 0; i < m_floatType.Count; i++)
            {
                if (m_floatType[i] == type)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsFloat(object o)
        {
            if (o == null)
            {
                return false;
            }
            if (o is TypeTool)
            {
                return TypeTool.IsFloat(o.GetType());
            }
            return false;
        }

        public static bool IsDateTime(object o)
        {
            if (o == null)
            {
                return false;
            }
            if (o is TypeTool)
            {
                return TypeTool.IsFloat(o.GetType());
            }
            return false;
        }

        public static bool IsDateTime(Type type)
        {
            if (type == null)
            {
                return false;
            }
            return (type == typeof(DateTime));
        }

        public static bool IsULong(Type type)
        {
            return type == typeof(ulong);
        }

        public static bool IsIPAddress(Type type)
        {
            return type == typeof(IPAddress);
        }

        public static bool IsValueType(Type type)
        {
            if (type == null)
            {
                return false;
            }
            if (type == typeof(TypeTool))
            {
                return true;
            }
            return type.IsSubclassOf(typeof(TypeTool));
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static IList<Type> m_type = new Type[] { typeof(double), typeof(float), typeof(long), typeof(ulong), typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(char), typeof(int), typeof(uint), typeof(bool), typeof(DateTime), typeof(IPAddress) };
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static IList<byte> m_size = new byte[] { 8, 4, 8, 8, 1, 1, 2, 2, 2, 4, 4, 1, 8, 4 };
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static Encoding m_enc = Encoding.Default;

        public static int SizeBy(Type type)
        {
            int i = m_type.IndexOf(type);
            if (i < 0)
            {
                return 0;
            }
            return m_size[i];
        }

        public static object LongTo(Type type, long value)
        {
            unchecked
            {
                if (type == typeof(int))
                    return (int)value;
                if (type == typeof(uint))
                    return Convert.ToUInt32(value);
                if (type == typeof(long))
                    return value;
                if (type == typeof(ulong))
                    return (ulong)value;
                if (type == typeof(bool))
                    return value > 0;
                if (type == typeof(byte))
                    return Convert.ToByte(value);
                if (type == typeof(sbyte))
                    return Convert.ToSByte(value);
                if (type == typeof(short))
                    return Convert.ToInt16(value);
                if (type == typeof(ushort))
                    return Convert.ToUInt16(value);
                if (type == typeof(double))
                    return BitConverter.Int64BitsToDouble(value);
                if (type == typeof(float))
                    return BitConverter.ToSingle(BitConverter.GetBytes(value), 0);
                if (type == typeof(DateTime))
                    return new DateTime(value);
                if (type == typeof(IPAddress))
                    return new IPAddress(value);
                return null;
            }
        }

        public static long ToLong(Type type, byte[] buffer, int size)
        {
            long value = 0;
            unchecked
            {
                for (int i = 0; i < size; i++)
                {
                    if (i >= buffer.Length)
                        break;
                    value |= ((long)buffer[i] & 0xFF) << (i * 8);
                }
            }
            return value;
        }

        public static byte[] BinaryBy(Type type, long value, int size)
        {
            if (size < 0)
            {
                size = TypeTool.SizeBy(type);
            }
            byte[] buffer = new byte[size];
            unchecked
            {
                for (int i = 0; i < size; i++)
                    buffer[i] = (byte)((value >> i * 8) & 0xFF);
            }
            return buffer;
        }

        public static byte[] BinaryBy(Type type, double value)
        {
            if (type == typeof(float))
                return BitConverter.GetBytes(Convert.ToSingle(value));
            if (type == typeof(double))
                return BitConverter.GetBytes(Convert.ToDouble(value));
            return BufferExtension.EmptryBuffer;
        }

        public static byte[] BinaryBy(ulong value)
        {
            return TypeTool.BinaryBy(typeof(long), Convert.ToInt64(value), sizeof(ulong));
        }
    }
}
