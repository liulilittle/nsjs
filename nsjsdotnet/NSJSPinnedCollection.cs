namespace nsjsdotnet
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Timer = nsjsdotnet.Core.Threading.Timer;

    public static class NSJSPinnedCollection
    {
        private static readonly IntPtr NULL = IntPtr.Zero;
        private static IDictionary<IntPtr, PinnedContext> contexts = new ConcurrentDictionary<IntPtr, PinnedContext>();
        private static readonly Timer maintenanceTimer; 

        private class PinnedContext
        {
            public GCHandle Handle;
            public Delegate Value;
            public IntPtr Address;
            public long LifeCycle;
            public DateTime PinnedTime = DateTime.Now;
        }

        static NSJSPinnedCollection()
        {
            maintenanceTimer = NSJSTimerScheduler.New();
            maintenanceTimer.Tick += (sender, e) =>
            {
                foreach (PinnedContext context in contexts.Values)
                {
                    if (context == null || context.LifeCycle == Infinite)
                    {
                        continue;
                    }
                    if (((DateTime.Now - context.PinnedTime).TotalMilliseconds) > context.LifeCycle)
                    {
                        Release(context.Address);
                    }
                }
            };
            maintenanceTimer.Interval = 100;
            maintenanceTimer.Enabled = true;
        }

        public const int Infinite = -1;

        public static int MaintenanceInterval
        {
            get
            {
                return maintenanceTimer.Interval;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                maintenanceTimer.Interval = value;
            }
        }

        public static int ReleaseAll()
        {
            int count = 0;
            foreach (IntPtr handle in contexts.Keys)
            {
                if (Release(handle))
                {
                    count++;
                }
            }
            return count;
        }

        public static int Count
        {
            get
            {
                return contexts.Count;
            }
        }

        public static IEnumerable<IntPtr> GetAllHandle()
        {
            return contexts.Keys;
        }

        public static IEnumerable<Delegate> GetAllValue()
        {
            foreach (PinnedContext context in contexts.Values)
            {
                if (context == null)
                {
                    continue;
                }
                yield return context.Value;
            }
        }

        public static bool IsPinned(Delegate d)
        {
            if (d == null)
            {
                return false;
            }
            return contexts.ContainsKey(GetCookie(d));
        }

        public static bool IsPinned(IntPtr cookie)
        {
            return contexts.ContainsKey(cookie);
        }

        public static T Pinned<T>(T d)
        {
            return Pinned<T>(d, Infinite);
        }

        public static T Pinned<T>(T d, TimeSpan lifeCycle)
        {
            return Pinned<T>(d, Convert.ToInt64(lifeCycle.TotalMilliseconds));
        }

        public static T Pinned<T>(T d, long lifeCycle)
        {
            if (d == null)
            {
                throw new ArgumentNullException("d");
            }
            if (!(d is Delegate))
            {
                throw new ArgumentException("Is not a delegate");
            }
            return (T)(object)Pinned((Delegate)(object)d, lifeCycle);
        }

        public static Delegate Pinned(Delegate d)
        {
            return Pinned(d, Infinite);
        }

        public static Delegate Pinned(Delegate d, TimeSpan lifeCycle)
        {
            return Pinned(d, Convert.ToInt64(lifeCycle.TotalMilliseconds));
        }

        public static Delegate Pinned(Delegate d, long lifeCycle)
        {
            if (d == null)
            {
                throw new ArgumentNullException("d");
            }
            PinnedContext context = new PinnedContext();
            context.Handle = GCHandle.Alloc(d);
            context.Address = GetCookie(d);
            context.Value = d;
            context.LifeCycle = lifeCycle;
            contexts.Add(context.Address, context);
            return d;
        }

        public static bool Release(IntPtr cookie)
        {
            if (cookie == NULL)
            {
                return true;
            }
            PinnedContext context;
            if (!contexts.TryGetValue(cookie, out context))
            {
                return false;
            }
            if (!contexts.Remove(cookie))
            {
                return false;
            }
            GCHandle gch = context.Handle;
            if (!gch.IsAllocated)
            {
                return false;
            }
            try
            {
                gch.Free();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool Release(Delegate d)
        {
            if (d == null)
            {
                return false;
            }
            return Release(GetCookie(d));
        }

        public static IntPtr GetCookie(Delegate d)
        {
            if (d == null)
            {
                throw new ArgumentNullException("d");
            }
            return NSJSFunction.DelegateToFunctionPtr(d);
        }
    }
}
