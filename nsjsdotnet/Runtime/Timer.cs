namespace nsjsdotnet.Runtime
{
    using global::System;
    using global::System.Collections.Concurrent;
    using global::System.Collections.Generic;
    using Interlocked = global::System.Threading.Interlocked;
    using TIMER = nsjsdotnet.Core.Threading.Timer;

    sealed class Timer
    {
        private static ConcurrentDictionary<int, TIMER> g_HandleTable = new ConcurrentDictionary<int, TIMER>();
        private static volatile int g_AutoincrementHandle = 0;
        private static NSJSFunctionCallback g_setTimeoutProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(setTimeout);
        private static NSJSFunctionCallback g_setIntervalProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(setInterval);
        private static NSJSFunctionCallback g_clearTimeoutProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(clearTimeout);
        private static NSJSFunctionCallback g_clearIntervalProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(clearInterval);

        public static void Initialization(NSJSVirtualMachine machine)
        {
            if (machine == null)
            {
                throw new ArgumentNullException("machine");
            }
            machine.AddFunction("setTimeout", g_setTimeoutProc);
            machine.AddFunction("setInterval", g_setIntervalProc);
            machine.AddFunction("clearTimeout", g_clearTimeoutProc);
            machine.AddFunction("clearInterval", g_clearIntervalProc);
        }

        private static void CloseTimer(int handle)
        {
            TIMER timer;
            if (g_HandleTable.TryRemove(handle, out timer))
            {
                timer.Interval = 0;
                timer.Enabled = false;
                timer.Dispose();
            }
        }

        private static void CloseTimer(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            CloseTimer(((arguments[0] as NSJSInt32)?.Value).GetValueOrDefault());
        }

        private static int AddTimer(TIMER timer)
        {
            int handle = 0;
            do
            {
                handle = Interlocked.Increment(ref g_AutoincrementHandle);
                if (handle < 0)
                {
                    handle = 0;
                }
            } while (!g_HandleTable.TryAdd(handle, timer)); // HANDLE碰撞
            return handle;
        }

        private static void SetTimer(IntPtr info, bool period)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            int handle = -1;
            if (arguments.Length > 1)
            {
                NSJSFunction callback = arguments[0] as NSJSFunction;
                NSJSInt32 millisec = arguments[1] as NSJSInt32;
                if (callback != null && millisec != null && millisec.Value >= 0)
                {
                    NSJSVirtualMachine machine = arguments.VirtualMachine;
                    callback.CrossThreading = true;
                    TIMER timer = new TIMER();
                    IList<NSJSValue> argv = new List<NSJSValue>();
                    for (int i = 2; i < arguments.Length; i++)
                    {
                        NSJSValue item = arguments[i];
                        item.CrossThreading = true;
                        argv.Add(item);
                    }
                    timer.Tick += (sender, e) =>
                    {
                        if (!period)
                        {
                            CloseTimer(handle);
                        }
                        machine.Join((sendert, statet) => callback.Call(argv));
                    };
                    handle = AddTimer(timer);
                    timer.Interval = millisec.Value;
                    timer.Enabled = true;
                }
            }
            arguments.SetReturnValue(handle);
        }

        private static void clearTimeout(IntPtr info)
        {
            CloseTimer(info);
        }

        private static void setTimeout(IntPtr info)
        {
            SetTimer(info, false);
        }

        private static void setInterval(IntPtr info)
        {
            SetTimer(info, true);
        }

        private static void clearInterval(IntPtr info)
        {
            CloseTimer(info);
        }
    }
}
