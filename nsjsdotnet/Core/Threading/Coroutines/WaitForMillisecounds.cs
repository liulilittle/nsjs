namespace nsjsdotnet.Core.Threading.Coroutines
{
    using System;
    using NT = System.Threading;

    class WaitForMillisecounds
    {
        protected int Timeout;

        protected int NowTick;

        public WaitForMillisecounds(int millisecondsTimeout)
        {
            this.Timeout = millisecondsTimeout;
            this.NowTick = Environment.TickCount; // NativeMethods.time();
        }

        public virtual bool Wait()
        {
            int timeout = this.Timeout;
            if (timeout == NT.Timeout.Infinite)
            {
                return false;
            }
            int tick = Environment.TickCount;
            int min = (tick - this.NowTick);
            return (min >= this.Timeout);
        }
    }
}
