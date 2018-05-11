namespace nsjsdotnet.Core.Threading
{
    using System;
    using System.Threading;

    public class Locker
    {
        private readonly object signal;
        private static readonly Locker locker = new Locker();

        public Locker() : this(new object())
        {

        }

        public Locker(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            this.signal = obj;
            this.RetryCount = 3;
            this.EnterTimeout = 800;
        }

        public static Locker Default
        {
            get
            {
                return locker;
            }
        }

        public int RetryCount
        {
            get;
            set;
        }

        public int EnterTimeout
        {
            get;
            set;
        }

        protected virtual object Signal
        {
            get
            {
                return signal;
            }
        }

        public virtual TResult Synchronized<TResult>(Func<TResult> f)
        {
            return Synchronized(f, null);
        }

        public virtual TResult Synchronized<TResult>(Func<TResult> f, Action<Exception> error)
        {
            if (f == null)
            {
                throw new ArgumentNullException("f");
            }
            Exception exception = null;
            int count = 0;
            bool localTaken = false;
            TResult result = default(TResult);
            while (count++ < this.RetryCount)
            {
                Monitor.TryEnter(this.signal, this.EnterTimeout, ref localTaken);
                if (localTaken)
                {
                    try
                    {
                        result = f();
                    }
                    catch (Exception e)
                    {
                        exception = e;
                    }
                    Monitor.Exit(this.signal);
                    break;
                }
                if (exception != null)
                {
                    if (error == null)
                    {
                        throw exception;
                    }
                    error(exception);
                    break;
                }
            }
            if (!localTaken && error != null)
            {
                error(new SynchronizationLockException());
            }
            return result;
        }

        public void Synchronized(Action a)
        {
            Synchronized(a, null);
        }

        public void Synchronized(Action a, Action<Exception> error)
        {
            if (a == null)
            {
                throw new ArgumentNullException("a");
            }
            Synchronized<object>(() =>
            {
                a();
                return default(object);
            }, error);
        }
    }
}
