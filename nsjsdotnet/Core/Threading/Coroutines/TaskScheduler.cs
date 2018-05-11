namespace nsjsdotnet.Core.Threading.Coroutines
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Diagnostics;

    public sealed class TaskScheduler : IEnumerable
    {
        private const int MAX_TASKS = 10000;
        private const int MAX_ERASE = 10000;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IList<Task> g_tasks = new List<Task>(MAX_TASKS);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IList<Task> g_erase = new List<Task>(MAX_ERASE);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Thread g_loop = null;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private object g_look = new object();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Task g_task = null;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool g_exit = false;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int g_idc = 0;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ApartmentState g_state = ApartmentState.MTA;

        private void LoopNow()
        {
            while (!g_exit)
            {
                lock (g_look)
                {
                    int len = g_tasks.Count;
                    for (int i = 0; i < len; i++)
                    {
                        g_task = g_tasks[i];
                        if (!g_task.Wakeup())
                        {
                            g_erase.Add(g_task);
                        }
                    }
                    g_task = null;
                    len = g_erase.Count;
                    for (int i = 0; i < len; i++)
                    {
                        Task task = g_erase[i];
                        g_tasks.Remove(task);
                    }
                    g_erase.Clear();
                }
                Waitable.usleep(1);
            }
            lock (g_look)
            {
                g_tasks.Clear();
                g_erase.Clear();
            }
        }

        internal Task StartNew(IEnumerator ator)
        {
            lock (g_look)
            {
                Task task = new Task(ator, ++g_idc);
                g_tasks.Add(task);
                return task;
            }
        }

        internal Task GetTask(int id)
        {
            lock (g_look)
            {
                int len = g_tasks.Count;
                for (int i = 0; i < len; i++)
                {
                    Task task = g_tasks[i];
                    if (task.Id == len)
                    {
                        return task;
                    }
                }
                return null;
            }
        }

        internal Task Current
        {
            get
            {
                return g_task;
            }
        }

        public int Count
        {
            get
            {
                return g_tasks.Count;
            }
        }

        public ApartmentState ApartmentState
        {
            get
            {
                return g_state;
            }
        }

        public static void Run(IEnumerator main)
        {
            TaskScheduler.Run(main, ApartmentState.STA);
        }

        public static void Run(IEnumerator main, ApartmentState state)
        {
            if (main == null)
            {
                throw new EntryPointNotFoundException("main");
            }
            TaskFactory factory = Task.Factory;
            TaskScheduler scheduler = factory.Scheduler;
            scheduler.StartNew(main);
            scheduler.g_state = state;
            scheduler.g_exit = false;
            if (state != ApartmentState.MTA)
            {
                scheduler.LoopNow();
            }
            else
            {
                scheduler.g_loop = new Thread(scheduler.LoopNow);
                scheduler.g_loop.IsBackground = false;
                scheduler.g_loop.Priority = ThreadPriority.Highest;
                scheduler.g_loop.Start();
            }
        }

        public static void Exit()
        {
            TaskFactory factory = Task.Factory;
            TaskScheduler scheduler = factory.Scheduler;
            scheduler.g_exit = true;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (g_look)
            {
                int len = g_tasks.Count;
                for (int i = 0; i < len; i++)
                {
                    yield return g_tasks[i];
                }
            }
        }
    }
}
