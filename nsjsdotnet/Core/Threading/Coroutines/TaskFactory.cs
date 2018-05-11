namespace nsjsdotnet.Core.Threading.Coroutines
{
    using System;
    using System.Collections;

    public sealed class TaskFactory
    {
        public TaskScheduler Scheduler
        {
            get;
            private set;
        }

        public TaskFactory(TaskScheduler scheduler)
        {
            if (scheduler == null)
            {
                throw new ArgumentNullException();
            }
            this.Scheduler = new TaskScheduler();
        }

        public Task StartNew(IEnumerator task)
        {
            if (task == null)
            {
                throw new ArgumentNullException();
            }
            return Scheduler.StartNew(task);
        }

        public Task Get(int id)
        {
            return Scheduler.GetTask(id);
        }
    }
}
