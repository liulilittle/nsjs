namespace nsjsdotnet
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text;

    public enum NSJSErrorKind : int
    {
        kError = 0,
        kRangeError = 1,
        kReferenceError = 2,
        kSyntaxError = 3,
        kTypeError = 4,
    }
    
    public class NSJSUnhandledExceptionEventArgs : EventArgs
    {
        public NSJSException Exception { get; private set; }

        public NSJSUnhandledExceptionEventArgs(NSJSException exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }
            this.Exception = exception;
        }
    }

    public unsafe class NSJSException : Exception
    {
        private static readonly IntPtr NULL = IntPtr.Zero;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string exception_message;

        [DllImport("nsjs.dll", SetLastError = false)]
        private static extern void nsjs_exception_throw_value(IntPtr isolate, IntPtr value);

        [DllImport("nsjs.dll", SetLastError = false)]
        private static extern void nsjs_exception_throw_error(IntPtr isolate, byte* value, NSJSErrorKind kind);

        public int ErrorLevel { get; private set; }

        public int EndColumn { get; private set; }

        public int EndPosition { get; private set; }

        public int LineNumber { get; private set; }

        public long ResourceColumnOffset { get; private set; }

        public long ResourceLineOffset { get; private set; }

        public string ResourceName { get; private set; }

        public string SourceMapUrl { get; private set; }

        public long ScriptId { get; private set; }

        public string ScriptResourceName { get; private set; }

        public string SourceLine { get; private set; }

        public int StartColumn { get; private set; }

        public int StartPosition { get; private set; }

        public bool IsSharedCrossOrigin { get; private set; }

        public string JavaScriptStackTrace { get; private set; }

        public override string Message
        {
            get
            {
                if (string.IsNullOrEmpty(this.exception_message))
                {
                    return null; 
                }
                return "Uncaught " + this.exception_message;
            }
        }
         
        public NSJSVirtualMachine VirtualMachine { get; private set; }

        internal static NSJSException From(NSJSVirtualMachine machine, NSJSStructural.NSJSExceptionInfo* exception)
        {
            if (exception == null || machine == null)
            {
                return null;
            }
            if (!exception->NowIsWrong)
            {
                return null;
            }
            exception->NowIsWrong = false;
            return new NSJSException(machine, exception);
        }

        public NSJSException(NSJSVirtualMachine machine) : this(machine, (string)null)
        {

        }

        public NSJSException(NSJSVirtualMachine machine, string message)
        {
            if (machine == null)
            {
                throw new ArgumentNullException("machine");
            }
            if (string.IsNullOrEmpty(message))
            {
                message = this.GetType().Name;
            }
            this.VirtualMachine = machine;
            this.exception_message = message;
        }

        private NSJSException(NSJSVirtualMachine machine, NSJSStructural.NSJSExceptionInfo* exception) 
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }
            this.VirtualMachine = machine ?? throw new ArgumentNullException("machine");
            this.EndColumn = exception->EndColumn;
            this.EndPosition = exception->EndPosition;
            this.ErrorLevel = exception->ErrorLevel;
            this.exception_message = new string((sbyte*)exception->ExceptionMessage);
            NSJSMemoryManagement.Free(exception->ExceptionMessage);
            this.IsSharedCrossOrigin = exception->IsSharedCrossOrigin;
            this.LineNumber = exception->LineNumber;
            this.ResourceColumnOffset = exception->ResourceColumnOffset;
            this.ResourceLineOffset = exception->ResourceLineOffset;
            this.ResourceName = new string((sbyte*)exception->ResourceName);
            NSJSMemoryManagement.Free(exception->ResourceName);
            this.ScriptId = exception->ScriptId;
            this.ScriptResourceName = new string((sbyte*)exception->ScriptResourceName);
            NSJSMemoryManagement.Free(exception->ScriptResourceName);
            this.SourceLine = new string((sbyte*)exception->SourceLine);
            NSJSMemoryManagement.Free(exception->SourceLine);
            this.SourceMapUrl = new string((sbyte*)exception->SourceMapUrl);
            NSJSMemoryManagement.Free(exception->SourceMapUrl);
            this.StartColumn = exception->StartColumn;
            this.StartPosition = exception->StartPosition;
            string s = new string((sbyte*)exception->StackTrace);
            if (s != null)
            {
                int index = s.IndexOf('\n');
                if (index > -1)
                {
                    s = s.Substring(index + 1);
                }
                s = s.Replace("\n", "\r\n");
            }
            this.JavaScriptStackTrace = s;
            NSJSMemoryManagement.Free(exception->StackTrace);
            // memeset
            exception->Reset();
        }

        protected internal void Raise()
        {
            NSJSVirtualMachine machine = this.VirtualMachine;
            machine.Abort();
            if (machine.AutomaticallyPrintException)
            {
                this.PrintStackTrace();
            }
            if (!machine.HasUnhandledExceptionHandler())
            {
                throw this;
            }
        }

        public static void Throw(NSJSException exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }
            NSJSVirtualMachine machine = exception.VirtualMachine;
            Throw(machine, exception.exception_message, NSJSErrorKind.kError);
        }

        public static void Throw(NSJSValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            nsjs_exception_throw_value(value.Isolate, value.Handle);
        }

        public static void Throw(NSJSVirtualMachine machine, string message)
        {
            Throw(machine, message, NSJSErrorKind.kError);
        }

        public static void Throw(NSJSVirtualMachine machine, string message, NSJSErrorKind kind)
        {
            if (machine == null)
            {
                throw new ArgumentNullException("machine");
            }
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            IntPtr isolate = machine.Isolate;
            if (isolate == NULL)
            {
                throw new InvalidOperationException("isolate");
            }
            if (!Enum.IsDefined(typeof(NSJSErrorKind), kind))
            {
                throw new NotSupportedException("kind");
            }
            byte[] cch = Encoding.UTF8.GetBytes(message);
            if (cch.Length <= 0)
            {
                cch = new byte[] { 0 };
            }
            fixed (byte* s = cch)
            {
                nsjs_exception_throw_error(isolate, s, kind);
            }
        }

        public static string GetDefaultFormatStackTrace(NSJSException exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }
            StringBuilder contents = new StringBuilder();
            contents.AppendLine(string.Format("{0} {1}({2}:{3})", exception.Message, exception.ResourceName, exception.LineNumber, exception.StartColumn));
            contents.AppendFormat("    ErrorLevel: {0}\r\n", exception.ErrorLevel);
            contents.AppendFormat("    StartColumn: {0}\r\n", exception.StartColumn);
            contents.AppendFormat("    StartPosition: {0}\r\n", exception.StartPosition);
            contents.AppendFormat("    EndColumn: {0}\r\n", exception.EndColumn);
            contents.AppendFormat("    EndPosition: {0}\r\n", exception.EndPosition);
            contents.AppendFormat("    LineNumber: {0}\r\n", exception.LineNumber);
            contents.AppendFormat("    ResourceColumnOffset: {0}\r\n", exception.ResourceColumnOffset);
            contents.AppendFormat("    ResourceLineOffset: {0}\r\n", exception.ResourceLineOffset);
            contents.AppendFormat("    IsSharedCrossOrigin: {0}\r\n", exception.IsSharedCrossOrigin);
            contents.AppendFormat("    ResourceName: {0}\r\n", exception.ResourceName);
            contents.AppendFormat("    ScriptId: {0}\r\n", exception.ScriptId);
            contents.AppendFormat("    ScriptResourceName: {0}\r\n", exception.ScriptResourceName);
            contents.AppendFormat("    SourceLine: {0}\r\n", exception.SourceLine);
            contents.AppendFormat("    SourceMapUrl: {0}\r\n", exception.SourceMapUrl);
            contents.AppendLine("-----------------------------------------------------");
            contents.AppendFormat("JavaScript StackTrace\r\n{0}\r\n", exception.JavaScriptStackTrace);
            contents.AppendLine("-----------------------------------------------------");
            contents.AppendLine(".NET StackTrace\r\n" + (exception.StackTrace ?? new StackTrace(1).ToString()));
            return contents.ToString();
        }

        public virtual void PrintStackTrace()
        {
            Console.WriteLine(GetDefaultFormatStackTrace(this));
        }
    }
}
