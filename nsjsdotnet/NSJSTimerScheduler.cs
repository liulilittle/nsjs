namespace nsjsdotnet
{
    using nsjsdotnet.Core.Threading;
    using System;
    using System.Collections.Concurrent;

    public static class NSJSTimerScheduler
    {
        private static readonly ConcurrentDictionary<NSJSVirtualMachine, TimerScheduler> schedulers
            = new ConcurrentDictionary<NSJSVirtualMachine, TimerScheduler>();

        public static Timer New()
        {
            return new Timer();
        }

        private static TimerScheduler GetScheduler(NSJSVirtualMachine machine)
        {
            if (machine == null)
            {
                throw new ArgumentNullException("machine");
            }
            if (machine.IsDisposed)
            {
                throw new ObjectDisposedException("machine");
            }
            return schedulers.GetOrAdd(machine, (key) =>
            {
                TimerScheduler scheduler = new TimerScheduler();
                machine.Disposed += (sender, e) => Close(machine);
                return scheduler;
            });
        }

        public static bool Close(NSJSVirtualMachine machine)
        {
            TimerScheduler scheduler = null;
            if (machine == null)
            {
                return false;
            }
            if (!schedulers.TryRemove(machine, out scheduler))
            {
                return false;
            }
            scheduler.Dispose();
            return true;
        }

        public static Timer New(NSJSVirtualMachine machine)
        {
            TimerScheduler scheduler = GetScheduler(machine);
            return new Timer(scheduler);
        }
    }
}
