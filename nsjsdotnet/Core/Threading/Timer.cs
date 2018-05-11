namespace nsjsdotnet.Core.Threading
{
    using System;

    public class Timer : IDisposable
    {
        private TimerScheduler _scheduler;
        private bool _enabled;
        private bool _disposed;
        public DateTime? _lasttime;
        private int _interval;

        public event EventHandler Tick;

        internal void DoTickEvent()
        {
            OnTick(EventArgs.Empty);
        }

        protected virtual void OnTick(EventArgs e)
        {
            if (this.Tick != null)
            {
                this.Tick(this, e);
            }
        }

        public TimerScheduler Scheduler
        {
            get
            {
                return _scheduler;
            }
        }

        public Timer(int interval, TimerScheduler scheduler) : this(scheduler)
        {
            this.Interval = interval;
        }

        public Timer(int interval) : this(interval, TimerScheduler.Default)
        {

        }

        public Timer() : this(TimerScheduler.Default)
        {

        }

        public Timer(TimerScheduler scheduler)
        {
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }
            _scheduler = scheduler;
        }

        ~Timer()
        {
            this.Dispose();
        }

        private void RequereOrThrowObjectDisposedException()
        {
            bool disposed = false;
            lock (this)
            {
                disposed = _disposed;
            }
            if (disposed)
            {
                throw new ObjectDisposedException(typeof(Timer).Name);
            }
        }

        public int Interval
        {
            get
            {
                return _interval;
            }
            set
            {
                RequereOrThrowObjectDisposedException();
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                lock (this)
                {
                    int original = value;
                    _interval = value;
                    if (original != value)
                    {
                        this.Enabled = (value > 0);
                    }
                }
            }
        }

        public bool Enabled
        {
            get
            {
                RequereOrThrowObjectDisposedException();
                lock (this)
                {
                    return _enabled;
                }
            }
            set
            {
                RequereOrThrowObjectDisposedException();
                lock (this)
                {
                    bool original = _enabled;
                    _enabled = value;
                    if (original != value)
                    {
                        if (value)
                        {
                            _lasttime = DateTime.Now;
                            _scheduler.Start(this);
                        }
                        else
                        {
                            _lasttime = null;
                            _scheduler.Stop(this);
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            this.Enabled = false;
            lock (this)
            {
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        public void Close()
        {
            this.Dispose();
        }

        public void Start()
        {
            this.Enabled = true;
        }

        public void Stop()
        {
            this.Enabled = false;
        }
    }
}
