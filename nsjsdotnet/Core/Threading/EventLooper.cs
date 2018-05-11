namespace nsjsdotnet.Core.Threading
{
    using nsjsdotnet.Core.Collection;
    using System;
    using System.Collections.Generic;
    using MT = System.Threading;

    public class EventLooper
    {
        private sealed class Event
        {
            public int uEvtId;
            public object lParam;
            public Action<object> lResult;
        }

        private LinkedList<Event> events = new LinkedList<Event>();
        private LinkedListIterator<Event> iterator;
        private Func<int, object, object> evtproc;
        private int threadid;
        private Locker locker;

        public EventLooper()
        {
            locker = new Locker();
            iterator = new LinkedListIterator<Event>(this, events);
        }

        private Event PeekEvent()
        {
            return locker.Synchronized(() =>
            {
                LinkedListIterator<Event> i = iterator++;
                Event message = i.Value;
                LinkedListNode<Event> node = i.Node;
                if (node != null)
                {
                    events.Remove(node);
                    iterator.Remove(node);
                }
                return message;
            });
        }

        public void PostEvent(int events, object param, Action<object> result)
        {
            locker.Synchronized(() =>
            {
                Event message = new Event();
                message.uEvtId = events;
                message.lParam = param;
                message.lResult = result;
                this.events.AddLast(message);
            });
        }

        public object SendEvent(int events, object param)
        {
            if (threadid == MT.Thread.CurrentThread.ManagedThreadId)
            {
                bool success = true;
                while (success)
                {
                    DoEvent(ref success);
                }
                return OnProc(events, param);
            }
            else
            {
                using (MT.AutoResetEvent signal = new MT.AutoResetEvent(false))
                {
                    object result = null;
                    PostEvent(events, param, (response) =>
                    {
                        result = response;
                        signal.Set();
                    });
                    while (!signal.WaitOne(1)) ;
                    return result;
                }
            }
        }

        protected virtual object OnProc(int events, object param)
        {
            return evtproc(events, param);
        }

        private object DoEvent(ref bool success)
        {
            Event message = PeekEvent();
            if (message == null)
            {
                success = false;
                return null;
            }
            success = true;
            object result = OnProc(message.uEvtId, message.lParam);
            Action<object> evt = message.lResult;
            if (evt != null)
            {
                evt(result);
            }
            return result;
        }

        public void Run(Func<int, object, object> proc)
        {
            if (proc == null)
            {
                throw new ArgumentNullException("proc");
            }
            evtproc = proc;
            threadid = MT.Thread.CurrentThread.ManagedThreadId;
            while (true)
            {
                bool success = false;
                DoEvent(ref success);
                if (!success)
                {
                    MT.Thread.Sleep(1);
                }
            }
        }
    }

}
