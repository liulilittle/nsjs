namespace nsjsdotnet
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public class NSJSKeyValueCollection
    {
        private static readonly ConcurrentDictionary<NSJSVirtualMachine, ConcurrentDictionary<int, object>> kvtables =
            new ConcurrentDictionary<NSJSVirtualMachine, ConcurrentDictionary<int, object>>();
        private const int NULL = 0;

        public static bool Set(NSJSValue key, object value)
        {
            if (key == null)
            {
                return false;
            }
            return Set(key.VirtualMachine, key.GetHashCode(), value);
        }

        public static bool Set(NSJSVirtualMachine machine, int key, object value)
        {
            if (key == NULL || value == null)
            {
                return false;
            }
            return GetTable(machine).AddOrUpdate(key, value, (i, oldValue) => value) == value;
        }

        public static IEnumerable<NSJSVirtualMachine> GetAllVirtualMachine()
        {
            return kvtables.Keys;
        }

        public static int GetCount()
        {
            return kvtables.Count;
        }

        private static ConcurrentDictionary<int, object> GetTable(NSJSVirtualMachine machine)
        {
            if (machine == null)
            {
                throw new ArgumentNullException("machine");
            }
            if (machine.IsDisposed)
            {
                throw new ObjectDisposedException("machine");
            }
            ConcurrentDictionary<int, object> dictionary;
            lock (kvtables)
            {
                if (!kvtables.TryGetValue(machine, out dictionary))
                {
                    dictionary = new ConcurrentDictionary<int, object>();
                    machine.Disposed += (sender, e) => ReleaseAll(machine);
                    kvtables.TryAdd(machine, dictionary);
                }
            }
            return dictionary;
        }

        public static object Get(NSJSValue key)
        {
            if (key == null)
            {
                return null;
            }
            return Get(key.VirtualMachine, key.GetHashCode());
        }

        public static object Get(NSJSVirtualMachine machine, int key)
        {
            if (key == NULL)
            {
                return null;
            }
            ConcurrentDictionary<int, object> table = GetTable(machine);
            object value;
            table.TryGetValue(key, out value);
            return value;
        }

        public static TValue Get<TValue>(NSJSValue key)
        {
            if (key == null)
            {
                return default(TValue);
            }
            return Get<TValue>(key.VirtualMachine, key.GetHashCode());
        }

        public static TValue Get<TValue>(NSJSVirtualMachine machine, int key)
        {
            object obj = Get(machine, key);
            if (obj == null)
            {
                return default(TValue);
            }
            if (typeof(TValue).IsInstanceOfType(obj))
            {
                return (TValue)obj;
            }
            return default(TValue);
        }

        public static bool Release<TValue>(NSJSVirtualMachine machine, int key, out TValue value)
        {
            object o;
            Release(machine, key, out o);
            if (o == null)
            {
                value = default(TValue);
                return false;
            }
            if (typeof(TValue).IsInstanceOfType(o))
            {
                value = default(TValue);
                return false;
            }
            value = (TValue)o;
            return true;
        }

        public static bool Release<TValue>(NSJSValue key, out TValue value)
        {
            if (key == null)
            {
                value = default(TValue);
                return false;
            }
            return Release<TValue>(key.VirtualMachine, key.GetHashCode(), out value);
        }

        public static bool Release(NSJSValue key, out object value)
        {
            value = null;
            if (key == null)
            {
                return false;
            }
            return Release(key.VirtualMachine, key.GetHashCode(), out value);
        }

        public static bool Release(NSJSVirtualMachine machine, int key)
        {
            if (machine == null)
            {
                return false;
            }
            object value;
            return Release(machine, key, out value);
        }

        public static bool Release(NSJSValue key)
        {
            if (key == null)
            {
                return false;
            }
            object value;
            return Release(key, out value);
        }

        public static bool Release(NSJSVirtualMachine machine, int key, out object value)
        {
            if (key == NULL)
            {
                value = null;
                return false;
            }
            ConcurrentDictionary<int, object> table = GetTable(machine);
            return table.TryRemove(key, out value);
        }

        public static IEnumerable<int> GetAllKey(NSJSVirtualMachine machine)
        {
            ConcurrentDictionary<int, object> table = GetTable(machine);
            return table.Keys;
        }

        public static IEnumerable<object> GetAllValue(NSJSVirtualMachine machine)
        {
            ConcurrentDictionary<int, object> table = GetTable(machine);
            return table.Values;
        }

        public static int GetCount(NSJSVirtualMachine machine)
        {
            return GetTable(machine).Count;
        }

        public static void ReleaseAll(NSJSVirtualMachine machine)
        {
            lock (kvtables)
            {
                ConcurrentDictionary<int, object> dictionary;
                if (kvtables.TryGetValue(machine, out dictionary))
                {
                    dictionary.Clear();
                    kvtables.TryRemove(machine, out dictionary);
                }
            }
        }

        public static void ReleaseAll()
        {
            kvtables.Clear();
        }
    }
}
