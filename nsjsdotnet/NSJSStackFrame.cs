namespace nsjsdotnet
{
    using System;

    public unsafe class NSJSStackFrame
    {
        private static readonly IntPtr NULL = IntPtr.Zero;

        public int Column { get; private set; }

        public string FunctionName { get; private set; }

        public int LineNumber { get; private set; }

        public int ScriptId { get; private set; }

        public string ScriptName { get; private set; }

        public string ScriptNameOrSourceURL { get; private set; }

        public bool IsConstructor { get; private set; }

        public bool IsEval { get; private set; }

        public bool IsWasm { get; private set; }

        internal NSJSStackFrame(ref NSJSStructural.NSJSStackFrame stackFrame)
        {
            this.Column = stackFrame.Column;
            this.FunctionName = new string((sbyte*)stackFrame.FunctionName);
            NSJSMemoryManagement.Free(stackFrame.FunctionName);
            this.IsConstructor = stackFrame.IsConstructor;
            this.IsEval = stackFrame.IsEval;
            this.IsWasm = stackFrame.IsWasm;
            this.LineNumber = stackFrame.LineNumber;
            this.ScriptId = stackFrame.ScriptId;
            this.ScriptName = new string((sbyte*)stackFrame.ScriptName);
            NSJSMemoryManagement.Free(stackFrame.ScriptName);
            this.ScriptName = new string((sbyte*)stackFrame.ScriptNameOrSourceURL);
            NSJSMemoryManagement.Free(stackFrame.ScriptNameOrSourceURL);
            // memset
            stackFrame.Column = 0;
            stackFrame.FunctionName = NULL;
            stackFrame.IsConstructor = false;
            stackFrame.IsEval = false;
            stackFrame.IsWasm = false;
            stackFrame.LineNumber = 0;
            stackFrame.ScriptId = 0;
            stackFrame.ScriptName = NULL;
            stackFrame.ScriptNameOrSourceURL = NULL;
        }
    }
}
