namespace nsjsdotnet.Core.Threading.Coroutines
{
    using System;

    sealed class WaitForCoroutine : WaitForMillisecounds
    {
        private Task Task;

        private Action<object, Task> Callback;

        private object Token;

        public WaitForCoroutine(Task task, int millisecondsTimeout)
            : base(millisecondsTimeout)
        {
            this.Task = task;
        }

        public WaitForCoroutine(Task task, int millisecondsTimeout, Action<object, Task> callback, object token)
            : this(task, millisecondsTimeout)
        {
            this.Callback = callback;
            this.Token = token;
        }

        public override bool Wait()
        {
            bool completed = false;
            Task task = this.Task;
            TaskState state = task.State;
            if (state == TaskState.kStopped)
            {
                completed = true;
            }
            if (!completed)
            {
                completed = base.Wait();
            }
            if (completed && this.Callback != null)
            {
                this.Callback(this.Token, this.Task);
            }
            return completed;
        }
    }
}
