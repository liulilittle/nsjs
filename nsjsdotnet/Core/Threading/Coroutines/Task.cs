namespace nsjsdotnet.Core.Threading.Coroutines
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public sealed partial class Task
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly TaskFactory g_facotry = null;

        static Task()
        {
            Task.g_facotry = new TaskFactory(new TaskScheduler());
        }

        public static TaskFactory Factory
        {
            get
            {
                return g_facotry;
            }
        }

        public static Task Current
        {
            get
            {
                return g_facotry.Scheduler.Current;
            }
        }

        public static int? CurrentId
        {
            get
            {
                Task task = Task.Current;
                if (task == null)
                {
                    return null;
                }
                return task.Id;
            }
        }

        public static readonly object NULL = null;
    }

    public sealed partial class Task
    {
        public static object Sleep(int millisecondsTimeout)
        {
            if (millisecondsTimeout < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            return new WaitForMillisecounds(millisecondsTimeout);
        }

        public static void Abort(Task task)
        {
            if (task != null)
            {
                task.Abort();
            }
        }

        public static object Abort(int exit)
        {
            return new AbortCurrentCoroutine(exit);
        }

        public static object WaitAny(Task[] tasks, Action<int, Task> callback)
        {
            return Task.WaitAny(tasks, Timeout.Infinite, callback);
        }

        public static object WaitAny(Task[] tasks, int millisecondsTimeout, Action<int, Task> callback)
        {
            if (tasks == null || callback == null)
            {
                throw new ArgumentNullException();
            }
            if (tasks.Length <= 0 || millisecondsTimeout < Timeout.Infinite)
            {
                throw new ArgumentOutOfRangeException();
            }
            Task task = Task.Factory.StartNew(Task.WaitAny(millisecondsTimeout, tasks, callback));
            return task.Wait();
        }

        private static IEnumerator WaitAny(int millisecondsTimeout, Task[] tasks, Action<int, Task> callback)
        {
            int tick = 0, index = -1;
            int len = tasks.Length;
            Task task = null;
            for (int i = 0; i < len; i++)
            {
                Task ii = tasks[i];
                ii.WaitAsync(millisecondsTimeout, (token, state) =>
                {
                    index = (int)token;
                    task = tasks[index];
                }, i);
            }
            bool infinite = (millisecondsTimeout == Timeout.Infinite);
            while ((infinite ? true : tick++ < millisecondsTimeout))
            {
                if (index > -1)
                {
                    break;
                }
                yield return Task.Sleep(1);
            }
            callback(index, task);
            yield return index;
        }

        public static object WaitAll(Task[] tasks)
        {
            return Task.WaitAll(tasks, Timeout.Infinite);
        }

        public static object WaitAll(Task[] tasks, int millisecondsTimeout)
        {
            if (tasks == null)
            {
                throw new ArgumentNullException();
            }
            if (tasks.Length <= 0 || millisecondsTimeout < Timeout.Infinite)
            {
                throw new ArgumentOutOfRangeException();
            }
            Task task = Task.Factory.StartNew(Task.WaitAll(millisecondsTimeout, tasks));
            return task.Wait();
        }

        private static IEnumerator WaitAll(int millisecondsTimeout, Task[] tasks)
        {
            int tick = 0, count = 0;
            int len = tasks.Length;
            for (int i = 0; i < len; i++)
            {
                Task ii = tasks[i];
                ii.WaitAsync(millisecondsTimeout, (token, state) =>
                {
                    count++;
                }, i);
            }
            bool infinite = (millisecondsTimeout == Timeout.Infinite);
            while ((infinite ? true : tick++ < millisecondsTimeout))
            {
                if (count >= len)
                {
                    break;
                }
                yield return Task.Sleep(1);
            }
            yield return (count >= len);
        }
    }

    public sealed partial class Task
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IEnumerator g_stack;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public long g_btick;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public long g_etick;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool g_abort;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool g_suspend;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool g_running = true;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int g_id;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private WaitForMillisecounds g_sleep;

        public string Name
        {
            get;
            set;
        }

        public object Tag
        {
            get;
            set;
        }

        public object Token
        {
            get;
            set;
        }

        public object Result
        {
            get;
            set;
        }

        public long Elapsed
        {
            get
            {
                long elapsed = 0;
                if (g_etick > 0)
                {
                    elapsed = (g_etick - g_btick);
                }
                else
                {
                    elapsed = (Environment.TickCount - g_btick);
                }
                return (elapsed < 0 ? 0 : elapsed);
            }
        }

        public int Id
        {
            get
            {
                return g_id;
            }
        }

        internal Task(IEnumerator ator, int id)
        {
            g_btick = Environment.TickCount;
            g_id = id;
            g_stack = ator;
        }

        internal bool Wakeup()
        {
            try
            {
                if (this.g_abort)
                {
                    return (g_running = false);
                }
                if (this.g_suspend)
                {
                    return (g_running = true);
                }
                if (g_sleep == null)
                {
                    if (!g_stack.MoveNext())
                    {
                        return (g_running = false);
                    }
                    object state = g_stack.Current;
                    if (!(this.Sleep(state) | this.Abort(state)))
                    {
                        this.Result = state;
                    }
                }
                else
                {
                    if (!g_sleep.Wait())
                    {
                        return (g_running = true);
                    }
                    g_sleep = null;
                }
                return (g_running = true);
            }
            finally
            {
                if (!g_running)
                {
                    g_etick = Environment.TickCount;
                }
            }
        }

        public void Abort()
        {
            lock (this)
            {
                this.g_abort = true;
            }
        }

        public TaskState State
        {
            get
            {
                if (g_abort)
                {
                    return TaskState.kAborted;
                }
                if (g_suspend)
                {
                    return TaskState.kSuspended;
                }
                if (g_sleep != null)
                {
                    return TaskState.kSleeping;
                }
                if (!g_running)
                {
                    return TaskState.kStopped;
                }
                return TaskState.kRunning;
            }
        }

        public void Suspend()
        {
            lock (this)
            {
                this.g_suspend = true;
            }
        }

        public void Resume()
        {
            lock (this)
            {
                this.g_suspend = false;
            }
        }

        public object Wait()
        {
            return this.Wait(Timeout.Infinite);
        }

        public object Wait(int millisecondsTimeout)
        {
            return new WaitForCoroutine(this, millisecondsTimeout);
        }

        public void WaitAsync(Action<object, Task> callback)
        {
            this.WaitAsync(Timeout.Infinite, callback);
        }

        public void WaitAsync(int millisecondsTimeout, Action<object, Task> callback)
        {
            this.WaitAsync(millisecondsTimeout, callback, null);
        }

        public void WaitAsync(Action<object, Task> callback, object token)
        {
            this.WaitAsync(Timeout.Infinite, callback, token);
        }

        private IEnumerator WaitAsync(Action<object, Task> callback, int millisecondsTimeout, object token)
        {
            if (callback == null)
            {
                throw new ArgumentNullException();
            }
            yield return new WaitForCoroutine(this, millisecondsTimeout, callback, token);
        }

        public void WaitAsync(int millisecondsTimeout, Action<object, Task> callback, object token)
        {
            Task.Factory.StartNew(this.WaitAsync(callback, millisecondsTimeout, token));
        }

        private bool Sleep(object state)
        {
            WaitForMillisecounds ms = state as WaitForMillisecounds;
            if (ms != null)
            {
                g_sleep = ms;
                return true;
            }
            return false;
        }

        private bool Abort(object state)
        {
            AbortCurrentCoroutine abort = state as AbortCurrentCoroutine;
            if (abort != null)
            {
                g_abort = true;
                abort.Handle();
                return true;
            }
            return false;
        }
    }
}
