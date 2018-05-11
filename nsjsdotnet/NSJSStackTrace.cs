namespace nsjsdotnet
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public unsafe class NSJSStackTrace : IEnumerable<NSJSStackFrame>
    {
        public const int MaxFrameCount = NSJSStructural.MAXSTACKFRAMECOUNT;

        private static readonly IntPtr NULL = IntPtr.Zero;

        private readonly NSJSStackFrame[] frames = null;

        [DllImport("nsjs.dll", SetLastError = false)]
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
            for (int i = 0; i < s.Length; i++)
            {
                yield return s[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        internal NSJSStackTrace(ref NSJSStructural.NSJSStackTrace stackTrace)
        {
            int count = stackTrace.Count;
            if (count > MaxFrameCount)
            {
                count = MaxFrameCount;
            }
            this.frames = new NSJSStackFrame[count];
            for (int i = 0; i < count; i++)
            {
                frames[i] = new NSJSStackFrame(ref stackTrace.Frame[i]);
            }
            stackTrace.Count = 0;
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
            int count = nsjs_stacktrace_getcurrent(isolate, ref machine.exception.StackTrace);
            if (count <= 0)
            {
                machine.exception.StackTrace.Count = 0;
            }
            return new NSJSStackTrace(ref machine.exception.StackTrace);
        }
    }
}
