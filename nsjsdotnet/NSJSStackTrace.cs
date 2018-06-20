namespace nsjsdotnet
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Diagnostics;

    public unsafe class NSJSStackTrace : IEnumerable<NSJSStackFrame>
    {
        public const int MaxFrameCount = NSJSStructural.MAXSTACKFRAMECOUNT;

        private static readonly IntPtr NULL = IntPtr.Zero;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly NSJSStackFrame[] frames = null;

        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, SetLastError = false)]
        private static extern int nsjs_stacktrace_getcurrent(IntPtr isolate, 
            ref NSJSStructural.NSJSStackTrace stacktrace);

        public int FrameCount
        {
            get;
            private set;
        }

        public NSJSStackFrame GetFrame(int index)
        {
            return this.frames[index];
        }

        public NSJSStackFrame[] GetFrames()
        {
            return this.frames;
        }

        public IEnumerator<NSJSStackFrame> GetEnumerator()
        {
            NSJSStackFrame[] s = this.frames;
            int count = s == null ? 0 : s.Length;
            for (int i = 0; i < s.Length; i++)
            {
                yield return s[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public override string ToString()
        {
            string s = null;
            foreach (NSJSStackFrame frame in this)
            {
                if (frame == null)
                {
                    continue;
                }
                s += frame.ToString() + "\r\n";
            }
            if (s != null && s.Length >= 2)
            {
                s = s.Remove(s.Length - 2);
            }
            return s;
        }

        internal NSJSStackTrace(NSJSStructural.NSJSStackTrace* stacktrace)
        {
            if (stacktrace == null)
            {
                throw new ArgumentNullException("stacktrace");
            }
            int count = stacktrace->Count;
            if (count > MaxFrameCount)
            {
                count = MaxFrameCount;
            }
            this.frames = new NSJSStackFrame[count];
            for (int i = 0; i < count; i++)
            {
                frames[i] = new NSJSStackFrame(&stacktrace->Frame[i]);
            }
            stacktrace->Count = 0;
            this.FrameCount = count;
        }

        public static NSJSStackTrace GetCurrent(NSJSVirtualMachine machine)
        {
            if (machine == null)
            {
                throw new ArgumentNullException("machine");
            }
            IntPtr isolate = machine.Isolate;
            if (isolate == NULL)
            {
                throw new InvalidOperationException("isolate");
            }
            int count = nsjs_stacktrace_getcurrent(isolate, ref *machine.stacktrace);
            if (count <= 0)
            {
                machine.stacktrace->Count = 0;
            }
            return new NSJSStackTrace(machine.stacktrace);
        }
    }
}
