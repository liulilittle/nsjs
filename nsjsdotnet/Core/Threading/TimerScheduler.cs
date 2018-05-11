namespace nsjsdotnet.Core.Threading
{
    using nsjsdotnet.Core.Collection;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public sealed class TimerScheduler : IDisposable
    {
        private static readonly TimerScheduler _default = new TimerScheduler();

        private readonly Thread _mta;
        private bool _disposed;
        private readonly LinkedList<Timer> _s;
        private LinkedListIterator<Timer> _i;
        private readonly IDictionary<Timer, LinkedListNode<Timer>> _m;

        public static TimerScheduler Default
        {
            get
            {
                return _default;
            }
        }

        public int Id
        {
            get
            {
                return _mta.ManagedThreadId;
            }
        }

        ~TimerScheduler()
        {
            this.Dispose();
        }

        public TimerScheduler()
        {
            _m = new Dictionary<Timer, LinkedListNode<Timer>>();
            _s = new LinkedList<Timer>();
            _i = new LinkedListIterator<Timer>(this, _s);
            _mta = new Thread(() =>
            {
                while (!_disposed)
                {
                    LinkedListNode<Timer> n = _i++.Node;
                    do
                    {
                        if (n == null)
                            break;
                        Timer t = n.Value;
                        if (t == null || !t.Enabled)
                            break;
                        DateTime? lt = t._lasttime;
                        if (lt == null)
                            break;
                        TimeSpan ts = (DateTime.Now - lt.Value);
                        if (ts.TotalMilliseconds < t.Interval)
                            break;
                        t._lasttime = DateTime.Now;
                        t.DoTickEvent();
                    } while (false);
                    Waitable.usleep(25);
                }
            });
            _mta.IsBackground = true;
            _mta.Start();
        }

        internal bool Start(Timer timer)
        {
            lock (this)
            {
                if (timer == null)
                    return false;
                if (_m.ContainsKey(timer))
                    return false;
                LinkedListNode<Timer> n = _s.AddLast(timer);
                _m.Add(timer, n);
                return true;
            }
        }

        internal bool Stop(Timer timer)
        {
            lock (this)
            {
                if (timer == null)
                    return false;
                LinkedListNode<Timer> n;
                if (!_m.TryGetValue(timer, out n))
                    return false;
                _s.Remove(n);
                _m.Remove(timer);
                _i.Remove(n);
                return true;
            }
        }

        public void Close()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            lock (this)
            {
                _disposed = true;
                _s.Clear();
                _m.Clear();
            }
            GC.SuppressFinalize(this);
        }
    }

}
