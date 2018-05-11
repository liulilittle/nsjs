namespace nsjsdotnet
{
    using System;
    using System.Runtime.InteropServices;

    unsafe class NSJSStructural
    {
        public const int MAXSTACKFRAMECOUNT = 100;

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
            public IntPtr ResourceName;
            public IntPtr SourceMapUrl;
            public long ScriptId;
            public IntPtr ScriptResourceName;
            public IntPtr SourceLine;
            public int StartColumn;
            public int StartPosition;
            public bool IsSharedCrossOrigin;
            public IntPtr ExceptionMessage;
            public NSJSStackTrace StackTrace;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct NSJSStackFrame
        {
            public int Column;
            public IntPtr FunctionName;
            public int LineNumber;
            public int ScriptId;
            public IntPtr ScriptName;
            public IntPtr ScriptNameOrSourceURL;
            public bool IsConstructor;
            public bool IsEval;
            public bool IsWasm;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct NSJSStackTrace
        {
            public int Count;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAXSTACKFRAMECOUNT)]
            public NSJSStackFrame[] Frame;
        }
    }
}
