namespace nsjsdotnet
{
    using System;
    using System.Runtime.InteropServices;

    unsafe class NSJSStructural
    {
        public const int MAXSTACKFRAMECOUNT = 100;
        public const string NSJSVMLINKLIBRARY = "nsjs.dll";

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct NSJSExceptionInfo
        {
            public bool NowIsWrong;
            public int ErrorLevel;
            public int EndColumn;
            public int EndPosition;
            public int LineNumber;
            public long ResourceColumnOffset;
            public long ResourceLineOffset;
            public char* ResourceName;
            public char* SourceMapUrl;
            public long ScriptId;
            public char* ScriptResourceName;
            public char* SourceLine;
            public int StartColumn;
            public int StartPosition;
            public bool IsSharedCrossOrigin;
            public char* ExceptionMessage;
            public char* StackTrace;

            public void Reset()
            {
                fixed (NSJSExceptionInfo* exception = &this) // memset
                {
                    exception->EndColumn = 0;
                    exception->EndPosition = 0;
                    exception->ErrorLevel = 0;
                    exception->ExceptionMessage = null;
                    exception->IsSharedCrossOrigin = false;
                    exception->LineNumber = 0;
                    exception->ResourceColumnOffset = 0;
                    exception->ResourceLineOffset = 0;
                    exception->ResourceName = null;
                    exception->ScriptId = 0;
                    exception->ScriptResourceName = null;
                    exception->SourceLine = null;
                    exception->SourceMapUrl = null;
                    exception->StartColumn = 0;
                    exception->StartPosition = 0;
                    exception->StackTrace = null;
                }
            }

            public static NSJSExceptionInfo* New()
            {
                NSJSExceptionInfo* exception = (NSJSExceptionInfo*)NSJSMemoryManagement.Alloc(sizeof(NSJSExceptionInfo));
                if (exception == null)
                {
                    throw new InvalidOperationException("exception");
                }
                if (exception->NowIsWrong)
                {
                    exception->NowIsWrong = false;
                }
                return exception;
            }

            public static void Free(NSJSExceptionInfo* exception)
            {
                if (exception != null)
                {
                    NSJSMemoryManagement.Free(exception);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct NSJSStackFrame
        {
            public int Column;
            public char* FunctionName;
            public int LineNumber;
            public int ScriptId;
            public char* ScriptName;
            public char* ScriptNameOrSourceURL;
            public bool IsConstructor;
            public bool IsEval;
            public bool IsWasm;

            public void Reset()
            {
                // memset
                this.Column = 0;
                this.FunctionName = null;
                this.IsConstructor = false;
                this.IsEval = false;
                this.IsWasm = false;
                this.LineNumber = 0;
                this.ScriptId = 0;
                this.ScriptName = null;
                this.ScriptNameOrSourceURL = null;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct NSJSStackTrace
        {
            public int Count;
            public NSJSStackFrame* Frame;

            public static void Free(NSJSStackTrace* stacktrace)
            {
                if (stacktrace != null)
                {
                    NSJSStackFrame* stackframes = stacktrace->Frame;
                    if (stackframes != null)
                    {
                        NSJSMemoryManagement.Free(stackframes);
                    }
                    NSJSMemoryManagement.Free(stacktrace);
                }
            }

            public static NSJSStackTrace* New()
            {
                NSJSStackTrace* stacktrace = (NSJSStackTrace*)NSJSMemoryManagement.Alloc(sizeof(NSJSStackTrace));
                if (stacktrace == null)
                {
                    throw new InvalidOperationException("stacktrace");
                }
                stacktrace->Count = 0;
                int cb = sizeof(NSJSStackFrame) * MAXSTACKFRAMECOUNT;
                stacktrace->Frame = (NSJSStackFrame*)NSJSMemoryManagement.Alloc(cb);
                if (stacktrace->Frame == null)
                {
                    throw new InvalidOperationException("stackframes");
                }
                return stacktrace;
            }
        }
    }
}
