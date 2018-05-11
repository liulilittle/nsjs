namespace nsjsdotnet.Core
{
    using nsjsdotnet.Core.Linq;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Net;
    using System.Net.Mail;
    using System.Text;

    public static class ArrayAuxiliary
    {
        private static T[] ToBuffer<T>(IEnumerable s)
        {
            if (s != null)
            {
                if (s is T[])
                {
                    return (T[])s;
                }
                else if (s is List<T>)
                {
                    List<T> list = (List<T>)s;
                    return list.ToArray();
                }
                else if (s is IEnumerable<T>)
                {
                    IEnumerable<T> it = (IEnumerable<T>)s;
                    List<T> list = new List<T>();
                    foreach (T i in it)
                    {
                        list.Add(i);
                    }
                    return list.ToArray();
                }
            }
            return new T[0];
        }

        public static NSJSValue ToArray(NSJSVirtualMachine machine, Type element, IList s)
        {
            if (machine == null)
            {
                return null;
            }
            int count = s == null ? 0 : s.Count;
            NSJSArray array = null;
            if (element == typeof(byte))
            {
                array = NSJSUInt8Array.New(machine, ToBuffer<byte>(s));
            }
            else if (element == typeof(sbyte))
            {
                array = NSJSInt8Array.New(machine, ToBuffer<sbyte>(s));
            }
            else if (element == typeof(short))
            {
                array = NSJSInt16Array.New(machine, ToBuffer<short>(s));
            }
            else if (element == typeof(ushort))
            {
                array = NSJSUInt16Array.New(machine, ToBuffer<ushort>(s));
            }
            else if (element == typeof(int))
            {
                array = NSJSInt32Array.New(machine, ToBuffer<int>(s));
            }
            else if (element == typeof(uint))
            {
                array = NSJSUInt32Array.New(machine, ToBuffer<uint>(s));
            }
            else if (element == typeof(float))
            {
                array = NSJSFloat32Array.New(machine, ToBuffer<float>(s));
            }
            else if (element == typeof(double))
            {
                array = NSJSFloat64Array.New(machine, ToBuffer<double>(s));
            }
            else
            {
                array = NSJSArray.New(machine, count);
                for (int i = 0; i < count; i++)
                {
                    array[i] = ObjectAuxiliary.ToObject(machine, s[i]);
                }
            }
            if (array == null)
            {
                return NSJSValue.Null(machine);
            }
            return array;
        }

        public static NSJSValue ToArray(NSJSVirtualMachine machine, IEnumerable<IDbDataParameter> parameters)
        {
            if (machine == null)
            {
                return null;
            }
            int count = parameters.Count();
            NSJSArray results = NSJSArray.New(machine, count);
            int index = 0;
            parameters.FirstOrDefault(i =>
            {
                if (i == null)
                {
                    return false;
                }
                results[index++] = ObjectAuxiliary.ToObject(machine, i);
                return false;
            });
            return results;
        }

        public static NSJSValue ToArray(NSJSVirtualMachine machine, DataTable dataTable)
        {
            if (machine == null)
            {
                return null;
            }
            DataRowCollection rows = null;
            int count = dataTable == null ? 0 : (rows = dataTable.Rows).Count;
            NSJSArray results = NSJSArray.New(machine, count);
            if (count <= 0)
            {
                return results;
            }
            IDictionary<string, int> columns = new Dictionary<string, int>();
            foreach (DataColumn column in dataTable.Columns)
            {
                columns.Add(column.ColumnName, column.Ordinal);
            }
            for (int i = 0; i < count; i++)
            {
                NSJSObject item = NSJSObject.New(machine);
                DataRow row = rows[i];
                results[i] = item;
                foreach (KeyValuePair<string, int> column in columns)
                {
                    object value = row[column.Value];
                    item.Set(column.Key, value.As(machine));
                }
            }
            return results;
        }

        public static void Copy(IList<short> sourceArray, NSJSInt16Array destinationArray, int length)
        {
            ArrayAuxiliary.Copy(sourceArray, 0, destinationArray, 0, length);
        }

        public static void Copy(IList<ushort> sourceArray, NSJSUInt16Array destinationArray, int length)
        {
            ArrayAuxiliary.Copy(sourceArray, 0, destinationArray, 0, length);
        }

        public static void Copy(IList<sbyte> sourceArray, NSJSInt8Array destinationArray, int length)
        {
            ArrayAuxiliary.Copy(sourceArray, 0, destinationArray, 0, length);
        }

        public static void Copy(IList<byte> sourceArray, NSJSUInt8Array destinationArray, int length)
        {
            ArrayAuxiliary.Copy(sourceArray, 0, destinationArray, 0, length);
        }

        public static void Copy(IList<int> sourceArray, NSJSInt32Array destinationArray, int length)
        {
            ArrayAuxiliary.Copy(sourceArray, 0, destinationArray, 0, length);
        }

        public static void Copy(IList<uint> sourceArray, NSJSUInt32Array destinationArray, int length)
        {
            ArrayAuxiliary.Copy(sourceArray, 0, destinationArray, 0, length);
        }

        public static void Copy(IList<float> sourceArray, NSJSFloat32Array destinationArray, int length)
        {
            ArrayAuxiliary.Copy(sourceArray, 0, destinationArray, 0, length);
        }

        public static void Copy(IList<double> sourceArray, NSJSFloat64Array destinationArray, int length)
        {
            ArrayAuxiliary.Copy(sourceArray, 0, destinationArray, 0, length);
        }

        public static void Copy(IList<short> sourceArray, int sourceIndex, NSJSInt16Array destinationArray, int destinationIndex, int length)
        {
            if (sourceArray == null)
            {
                throw new ArgumentNullException("sourceArray");
            }
            if (destinationArray == null)
            {
                throw new ArgumentNullException("destinationArray");
            }
            for (int i = 0; sourceIndex < length; sourceIndex++, i++)
            {
                destinationArray[destinationIndex + i] = sourceArray[sourceIndex];
            }
        }

        public static void Copy(IList<ushort> sourceArray, int sourceIndex, NSJSUInt16Array destinationArray, int destinationIndex, int length)
        {
            if (sourceArray == null)
            {
                throw new ArgumentNullException("sourceArray");
            }
            if (destinationArray == null)
            {
                throw new ArgumentNullException("destinationArray");
            }
            for (int i = 0; sourceIndex < length; sourceIndex++, i++)
            {
                destinationArray[destinationIndex + i] = sourceArray[sourceIndex];
            }
        }

        public static void Copy(IList<uint> sourceArray, int sourceIndex, NSJSUInt32Array destinationArray, int destinationIndex, int length)
        {
            if (sourceArray == null)
            {
                throw new ArgumentNullException("sourceArray");
            }
            if (destinationArray == null)
            {
                throw new ArgumentNullException("destinationArray");
            }
            for (int i = 0; sourceIndex < length; sourceIndex++, i++)
            {
                destinationArray[destinationIndex + i] = sourceArray[sourceIndex];
            }
        }

        public static void Copy(IList<int> sourceArray, int sourceIndex, NSJSInt32Array destinationArray, int destinationIndex, int length)
        {
            if (sourceArray == null)
            {
                throw new ArgumentNullException("sourceArray");
            }
            if (destinationArray == null)
            {
                throw new ArgumentNullException("destinationArray");
            }
            for (int i = 0; sourceIndex < length; sourceIndex++, i++)
            {
                destinationArray[destinationIndex + i] = sourceArray[sourceIndex];
            }
        }

        public static void Copy(IList<byte> sourceArray, int sourceIndex, NSJSUInt8Array destinationArray, int destinationIndex, int length)
        {
            if (sourceArray == null)
            {
                throw new ArgumentNullException("sourceArray");
            }
            if (destinationArray == null)
            {
                throw new ArgumentNullException("destinationArray");
            }
            for (int i = 0; sourceIndex < length; sourceIndex++, i++)
            {
                destinationArray[destinationIndex + i] = sourceArray[sourceIndex];
            }
        }

        public static void Copy(IList<sbyte> sourceArray, int sourceIndex, NSJSInt8Array destinationArray, int destinationIndex, int length)
        {
            if (sourceArray == null)
            {
                throw new ArgumentNullException("sourceArray");
            }
            if (destinationArray == null)
            {
                throw new ArgumentNullException("destinationArray");
            }
            for (int i = 0; sourceIndex < length; sourceIndex++, i++)
            {
                destinationArray[destinationIndex + i] = sourceArray[sourceIndex];
            }
        }

        public static void Copy(IList<float> sourceArray, int sourceIndex, NSJSFloat32Array destinationArray, int destinationIndex, int length)
        {
            if (sourceArray == null)
            {
                throw new ArgumentNullException("sourceArray");
            }
            if (destinationArray == null)
            {
                throw new ArgumentNullException("destinationArray");
            }
            for (int i = 0; sourceIndex < length; sourceIndex++, i++)
            {
                destinationArray[destinationIndex + i] = sourceArray[sourceIndex];
            }
        }

        public static void Copy(IList<double> sourceArray, int sourceIndex, NSJSFloat64Array destinationArray, int destinationIndex, int length)
        {
            if (sourceArray == null)
            {
                throw new ArgumentNullException("sourceArray");
            }
            if (destinationArray == null)
            {
                throw new ArgumentNullException("destinationArray");
            }
            for (int i = 0; sourceIndex < length; sourceIndex++, i++)
            {
                destinationArray[destinationIndex + i] = sourceArray[sourceIndex];
            }
        }

        public static string[] ToStringArray(NSJSValue value)
        {
            List<string> s = ToStringList(value) as List<string>;
            if (s == null)
            {
                return null;
            }
            return s.ToArray();
        }

        public static IList<string> ToStringList(NSJSValue value)
        {
            if (value == null)
            {
                return null;
            }
            IList<string> s = new List<string>();
            try
            {
                NSJSArray arrays = value as NSJSArray;
                if (arrays != null)
                {
                    foreach (NSJSValue i in arrays)
                    {
                        if (i == null)
                        {
                            continue;
                        }
                        s.Add(ValueAuxiliary.ToString(i));
                    }
                }
            }
            catch (Exception) { }
            return s;
        }

        public static int Fill(NSJSValue source, AttachmentCollection destination)
        {
            return Fill(source, destination, (s, index) => ObjectAuxiliary.ToAttachment(s[index]));
        }

        public static int Fill<T>(NSJSValue source, ICollection<T> destination, Func<NSJSArray, int, T> converter)
        {
            NSJSArray s = source as NSJSArray;
            int count = 0;
            if (destination == null)
            {
                return count;
            }
            if (s == null)
            {
                return count;
            }
            if (converter == null)
            {
                return count;
            }
            int len = s.Length;
            for (int i = 0; i < len; i++)
            {
                T item = converter(s, count);
                if (item == null)
                {
                    continue;
                }
                destination.Add(item);
                count++;
            }
            return count;
        }

        public static int Fill(NSJSValue source, MailAddressCollection destination)
        {
            int count = 0;
            if (destination == null)
            {
                return count;
            }
            NSJSString addresss = source as NSJSString;
            if (addresss != null)
            {
                destination.Add(addresss.Value);
                return count;
            }
            NSJSArray s = source as NSJSArray;
            if (s == null)
            {
                return count;
            }
            int len = s.Length;
            for (int i = 0; i < len; i++)
            {
                MailAddress address = ObjectAuxiliary.ToMailAddress(s[i]);
                if (address == null)
                {
                    continue;
                }
                destination.Add(address);
                count++;
            }
            return count;
        }

        public static int Fill(NSJSValue source, CookieCollection destination)
        {
            NSJSArray s = source as NSJSArray;
            int count = 0;
            if (s == null || destination == null)
            {
                return count;
            }
            int len = s.Length;
            for (int i = 0; i < len; i++)
            {
                Cookie cookie = ObjectAuxiliary.ToCookie(s[i]);
                if (cookie == null)
                {
                    continue;
                }
                destination.Add(cookie);
                count++;
            }
            return count;
        }

        public static NSJSValue ToArray(NSJSVirtualMachine machine, CookieCollection cookies)
        {
            if (machine == null)
            {
                return null;
            }
            int count = cookies == null ? 0 : cookies.Count;
            NSJSArray s = NSJSArray.New(machine, count);
            if (cookies != null)
            {
                int index = 0;
                foreach (Cookie cookie in cookies)
                {
                    s[index++] = ObjectAuxiliary.ToObject(machine, cookie);
                }
            }
            return s;
        }

        public static NSJSValue ToArray(NSJSVirtualMachine machine, IEnumerable<KeyValuePair<string, string>> s)
        {
            if (machine == null)
            {
                return null;
            }
            int count = Enumerable.Count(s);
            NSJSArray array = NSJSArray.New(machine, count);
            int index = 0;
            s.FirstOrDefault(kv =>
            {
                array[index++] = ObjectAuxiliary.ToObject(machine, kv);
                return false;
            });
            return array;
        }

        public static NSJSValue ToArray(NSJSVirtualMachine machine, IEnumerable<EncodingInfo> encodings)
        {
            if (machine == null)
            {
                return null;
            }
            int count = Enumerable.Count(encodings);
            NSJSArray array = NSJSArray.New(machine, count);
            int index = 0;
            encodings.FirstOrDefault(address =>
            {
                array[index++] = ObjectAuxiliary.ToObject(machine, address);
                return false;
            });
            return array;
        }

        public static NSJSValue ToArray(NSJSVirtualMachine machine, IEnumerable<IPAddress> addresses)
        {
            if (machine == null)
            {
                return null;
            }
            int count = Enumerable.Count(addresses);
            NSJSArray array = NSJSArray.New(machine, count);
            int index = 0;
            addresses.FirstOrDefault(address =>
            {
                array[index++] = ObjectAuxiliary.ToObject(machine, address);
                return false;
            });
            return array;
        }

        public static NSJSValue ToArray(NSJSVirtualMachine machine, IEnumerable<IPEndPoint> endpoints)
        {
            if (machine == null)
            {
                return null;
            }
            int count = Enumerable.Count(endpoints);
            NSJSArray array = NSJSArray.New(machine, count);
            int index = 0;
            endpoints.FirstOrDefault(address =>
            {
                array[index++] = ObjectAuxiliary.ToObject(machine, address);
                return false;
            });
            return array;
        }

        public static NSJSValue ToArray(NSJSVirtualMachine machine, IEnumerable<string> s)
        {
            if (machine == null)
            {
                return null;
            }
            int count = Enumerable.Count(s);
            NSJSArray array = NSJSArray.New(machine, count);
            int index = 0;
            s.FirstOrDefault(item =>
            {
                NSJSValue value = null;
                if (item == null)
                {
                    value = NSJSValue.Null(machine);
                }
                else
                {
                    value = NSJSString.New(machine, item);
                }
                array[index++] = value;
                return false;
            });
            return array;
        }
    }
}
