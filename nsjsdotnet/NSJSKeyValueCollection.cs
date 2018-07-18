namespace nsjsdotnet
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;

    public class NSJSKeyValueCollection 
    {
        private static readonly ConcurrentDictionary<NSJSVirtualMachine, ConcurrentDictionary<int, object>> kvtables =
            new ConcurrentDictionary<NSJSVirtualMachine, ConcurrentDictionary<int, object>>();
        private const int NULL = 0;
        private static volatile int seedofobjectid = 0;
        private const string RUNTIME_OBJECTID_PROPERTYKEY = "#____nsjsdotnet_framework_internalfield_objectid";

        public static bool Set(NSJSObject obj, object value)
        {
            if (obj == null)
            {
                return false;
            }
            NSJSVirtualMachine machine = obj.VirtualMachine;
            ConcurrentDictionary<int, object> dd = GetTable(machine);
            lock (dd)
            {
                int key = GetObjectIdentity(obj);
                if (dd.ContainsKey(key))
                {
                    dd[key] = value;
                }
                else
                {
                    key = GetObjectIdentity(dd);
                    dd.TryAdd(key, value);
                    SetObjectIdentity(obj, key);
                }
                return true;
            }
        }

        protected static int GetObjectIdentity(NSJSObject obj)
        {
            if (obj == null)
            {
                return 0;
            }
            IntPtr handle = obj.GetPropertyAndReturnHandle(RUNTIME_OBJECTID_PROPERTYKEY);
            if (handle == null)
            {
                return 0;
            }
            return new NSJSInt32(handle, obj.VirtualMachine).Value;
        }

        protected static int GetObjectIdentity(ConcurrentDictionary<int, object> objects)
        {
            int key = 0;
            do
            {
                key = Interlocked.Increment(ref seedofobjectid);
            } while (key == 0 || objects.ContainsKey(key));
            return key;
        }

        protected static bool SetObjectIdentity(NSJSObject obj, int key)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            return obj.Set(RUNTIME_OBJECTID_PROPERTYKEY, key);
        }

        public static IEnumerable<NSJSVirtualMachine> GetAllVirtualMachine()
        {
            return kvtables.Keys;
        }

        public static int GetCount()
        {
            return kvtables.Count;
        }

        public static bool Contains(NSJSObject obj)
        {
            if (obj == null)
            {
                return false;
            }
            ConcurrentDictionary<int, object> d = GetTable(obj.VirtualMachine);
            if (d == null)
            {
                return false;
            }
            return d.ContainsKey(GetObjectIdentity(obj));
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
            return kvtables.GetOrAdd(machine, (key) =>
            {
                ConcurrentDictionary<int, object> dd = new ConcurrentDictionary<int, object>();
                machine.Disposed += (sender, e) => ReleaseAll(machine);
                kvtables.TryAdd(machine, dd);
                return dd;
            });
        }

        public static object Get(NSJSObject obj)
        {
            if (obj == null)
            {
                return null;
            }
            NSJSVirtualMachine machine = obj.VirtualMachine;
            ConcurrentDictionary<int, object> dd = GetTable(machine);
            int key = GetObjectIdentity(obj);
            object value;
            if (dd.TryGetValue(key, out value))
            {
                return value;
            }
            return default(object);
        }

        public static TValue Get<TValue>(NSJSObject obj)
        {
            if (obj == null)
            {
                return default(TValue);
            }
            object result = Get(obj);
            if (result == null || !typeof(TValue).IsInstanceOfType(result))
            {
                return default(TValue);
            }
            return (TValue)result;
        }

        public static bool Release<TValue>(NSJSObject obj, out TValue value)
        {
            if (obj == null)
            {
                value = default(TValue);
                return false;
            }
            object result;
            bool success = Release(obj, out result);
            if (!success)
            {
                result = default(TValue);
            }
            value = (TValue)result;
            return success;
        }

        public static bool Release(NSJSObject obj, out object value)
        {
            if (obj == null)
            {
                value = null;
                return false;
            }
            NSJSVirtualMachine machine = obj.VirtualMachine;
            ConcurrentDictionary<int, object> dd = GetTable(machine);
            int key = GetObjectIdentity(obj);
            return dd.TryRemove(key, out value);
        }

        public static bool Release(NSJSObject obj)
        {
            if (obj == null)
            {
                return false;
            }
            object value;
            return Release(obj, out value);
        }

        public static IEnumerable<object> GetAllValue(NSJSVirtualMachine machine)
        {
            ConcurrentDictionary<int, object> dd = GetTable(machine);
            return dd.Values;
        }

        public static int GetCount(NSJSVirtualMachine machine)
        {
            ConcurrentDictionary<int, object> dd = GetTable(machine);
            return dd.Count;
        }

        public static void ReleaseAll(NSJSVirtualMachine machine)
        {
            ConcurrentDictionary<int, object> dd;
            if (kvtables.TryRemove(machine, out dd))
            {
                dd.Clear(); ;
            }
        }

        public static void ReleaseAll()
        {
            kvtables.Clear();
        }
    }
}
