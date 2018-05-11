namespace nsjsdotnet.Core.Threading
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    public unsafe class SpinLock // TASLock
    {
        private const long MAXITERATIONCOUNT = int.MaxValue;
        public const long Infinite = -1;

        private long signal = 0x00; // 原子信号
        private int threadid = 0x00; // 加锁线程
        private int refcount = 0x00; // 引用计数

        public void Enter(bool* localTaken)
        {
            Enter(localTaken, MAXITERATIONCOUNT);
        }

        public void Enter(ref bool localTaken)
        {
            Enter(ref localTaken, MAXITERATIONCOUNT);
        }

        public int EnterThreadId
        {
            get
            {
                return threadid;
            }
        }

        public TResult Synchronized<TResult>(Func<TResult> d)
        {
            if (d == null)
            {
                throw new ArgumentNullException("d");
            }
            try
            {
                bool localTaken = false;
                this.Enter(&localTaken, Infinite);
                if (!localTaken)
                {
                    throw new InvalidOperationException("localTaken");
                }
                return d();
            }
            finally
            {
                this.Exit();
            }
        }

        public void Synchronized(Action a)
        {
            if (a == null)
            {
                throw new ArgumentNullException("a");
            }
            try
            {
                bool localTaken = false;
                this.Enter(&localTaken, Infinite);
                if (!localTaken)
                {
                    throw new InvalidOperationException("localTaken");
                }
                a();
            }
            finally
            {
                this.Exit();
            }
        }

        public void Enter(ref bool localTaken, long iterations)
        {
            fixed (bool* pLocalTaken = &localTaken)
            {
                Enter(pLocalTaken, iterations);
            }
        }

        public void Enter(bool* localTaken, long iterations)
        {
            Enter(localTaken, &iterations, null);
        }

        public void Enter(bool* localTaken, int timeval)
        {
            Enter(localTaken, null, &timeval);
        }

        public void Enter(ref bool localTaken, int timeval)
        {
            fixed (bool* pLocalTaken = &localTaken)
            {
                Enter(pLocalTaken, timeval);
            }
        }

        private void Enter(bool* localTaken, long* iterations, int* timeval)
        {
            if (localTaken == null)
            {
                throw new ArgumentNullException("localTaken");
            }
            if (*localTaken)
            {
                throw new ArgumentException("localTaken");
            }
            int threadid = Thread.CurrentThread.ManagedThreadId;
            if (threadid == Interlocked.CompareExchange(ref this.threadid, 0x00, 0x00))
            {
                *localTaken = true;
            }
            else
            {
                Stopwatch sw = null;
                if (timeval != null)
                {
                    sw = new Stopwatch();
                    sw.Start();
                }
                long count = 0;
                while (!*localTaken)
                {
                    if (iterations != null && (*iterations != Infinite && ++count > *iterations))
                    {
                        break;
                    }
                    if (timeval != null && (*timeval != Infinite && sw.ElapsedMilliseconds >= *timeval))
                    {
                        break;
                    }
                    if (Interlocked.Increment(ref signal) == 0x01) // 获取到锁信号
                    {
                        *localTaken = true;
                        Interlocked.Exchange(ref this.threadid, threadid);
                    }
                }
                if (sw != null)
                {
                    sw.Stop();
                }
            }
            if (*localTaken)
            {
                Interlocked.Increment(ref refcount);
            }
        }

        public void Exit()
        {
            if (Thread.CurrentThread.ManagedThreadId != Interlocked.CompareExchange(ref threadid, 0x00, 0x00))
            {
                throw new InvalidOperationException();
            }
            if (Interlocked.Decrement(ref refcount) <= 0x00)
            {
                Interlocked.Exchange(ref threadid, 0x00);
                Interlocked.Exchange(ref signal, 0x00);
            }
        }
    }
}
