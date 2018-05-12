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

        public override string ToString()
        {
            string s = string.Format("    at {0}", this.FunctionName);
            bool parentheses = !string.IsNullOrEmpty(this.FunctionName);
            if (parentheses)
            {
                s += " (";
            }
            string scriptname = this.ScriptName;
            if (string.IsNullOrEmpty(scriptname))
            {
                scriptname = "anonymous";
            }
            s += string.Format("<{0}>:{1}:{2}", scriptname, this.LineNumber, this.Column);
            if (parentheses)
            {
                s += ')';
            }
            return s;
        }

        internal NSJSStackFrame(NSJSStructural.NSJSStackFrame* stackframe)
        {
            if (stackframe == null)
            {
                throw new ArgumentNullException("stackframe");
            }
            this.Column = stackframe->Column;
            this.FunctionName = new string((sbyte*)stackframe->FunctionName);
            NSJSMemoryManagement.Free(stackframe->FunctionName);
            this.IsConstructor = stackframe->IsConstructor;
            this.IsEval = stackframe->IsEval;
            this.IsWasm = stackframe->IsWasm;
            this.LineNumber = stackframe->LineNumber;
            this.ScriptId = stackframe->ScriptId;
            this.ScriptName = new string((sbyte*)stackframe->ScriptName);
            NSJSMemoryManagement.Free(stackframe->ScriptName);
            this.ScriptNameOrSourceURL = new string((sbyte*)stackframe->ScriptNameOrSourceURL);
            NSJSMemoryManagement.Free(stackframe->ScriptNameOrSourceURL);
            // memset
            stackframe->Reset();
        }
    }
}
