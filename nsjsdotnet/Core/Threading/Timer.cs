namespace nsjsdotnet.Core.Threading
{
    using System;

    public class Timer : IDisposable
    {
        private TimerScheduler scheduler;
        private bool enabled;
        public DateTime? lasttime;
        private int interval;

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
                return scheduler;
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
            this.scheduler = scheduler;
        }

        ~Timer()
        {
            this.Dispose();
        }

        public int Interval
        {
            get
            {
                return interval;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                lock (this)
                {
                    int original = value;
                    interval = value;
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
                lock (this)
                {
                    return enabled;
                }
            }
            set
            {
                lock (this)
                {
                    bool original = enabled;
                    enabled = value;
                    if (original != value)
                    {
                        if (value)
                        {
                            lasttime = DateTime.Now;
                            scheduler.Start(this);
                        }
                        else
                        {
                            lasttime = null;
                            scheduler.Stop(this);
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            this.Enabled = false;
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
