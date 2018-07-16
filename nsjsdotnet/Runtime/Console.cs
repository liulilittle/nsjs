namespace nsjsdotnet.Runtime
{
    using System;
    using nsjsdotnet.Core;

    static class Console
    {
        public static NSJSVirtualMachine.ExtensionObjectTemplate ClassTemplate
        {
            get;
            private set;
        }

        public static readonly NSJSConsoleHandler DefaultHandler = new DefaultConsoleHandler();

        private class DefaultConsoleHandler : NSJSConsoleHandler
        {

        }

        static Console()
        {
            NSJSVirtualMachine.ExtensionObjectTemplate owner = new NSJSVirtualMachine.ExtensionObjectTemplate();
            ClassTemplate = owner;
            owner.Set("title", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(title));
            owner.Set("error", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(error));
            owner.Set("assert", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(assert));
            owner.Set("log", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(log));
            owner.Set("printf", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(printf));
            owner.Set("println", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(println));
            owner.Set("sprintf", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(sprintf));
            owner.Set("sprintln", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(sprintln));
            owner.Set("system", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(system));
            owner.Set("clear", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(clear));
            owner.Set("message", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(message));
        }

        private static NSJSConsoleHandler GetConsoleHandler(NSJSFunctionCallbackInfo arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }
            NSJSVirtualMachine machine = arguments.VirtualMachine;
            return machine.ConsoleHandler;
        }

        private static void title(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            NSJSConsoleHandler handler = GetConsoleHandler(arguments);
            if (arguments.Length <= 0)
            {
                arguments.SetReturnValue(handler.GetTitle(arguments));
            }
            else
            {
                handler.SetTitle(arguments, (arguments[0] as NSJSString)?.Value);
            }
        }

        private static void assert(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            if (arguments.Length > 0)
            {
                bool condition = ValueAuxiliary.ToBoolean(arguments[0]);
                string message = ValueAuxiliary.ToString(arguments.Length > 1 ? arguments[1] : null) ?? string.Empty;
                string detailMessage = ValueAuxiliary.ToString(arguments.Length > 1 ? arguments[1] : null) ?? string.Empty;

                NSJSConsoleHandler handler = GetConsoleHandler(arguments);
                handler.Assert(arguments, condition, message, detailMessage);
            }
        }

        private static void error(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            if (arguments.Length > 0)
            {
                NSJSConsoleHandler handler = GetConsoleHandler(arguments);
                handler.Throw(arguments, arguments[0]);
            }
        }

        private static void clear(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            NSJSConsoleHandler handler = GetConsoleHandler(arguments);
            handler.Clear(arguments);
        }

        private static void message(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            if (arguments.Length > 0)
            {
                NSJSConsoleHandler handler = GetConsoleHandler(arguments);
                handler.OnMessage(new NSJSMessage(arguments, arguments[0]));
            }
        }

        private static void internalprintstring(IntPtr info, int mode)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            if (arguments.Length > 0)
            {
                NSJSConsoleHandler handler = GetConsoleHandler(arguments);
                if (mode == 0)
                {
                    handler.Log(arguments);
                }
                else if (mode == 1)
                {
                    handler.PrintString(arguments);
                }
                else if (mode == 2)
                {
                    handler.PrintLine(arguments);
                }
            }
        }

        private static void sprintf(IntPtr info)
        {
            internalsprintstring(info, false);
        }

        private static void sprintln(IntPtr info)
        {
            internalsprintstring(info, true);
        }

        private static void internalsprintstring(IntPtr info, bool newline)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            string contents = null;
            if (arguments.Length > 0)
            {
                NSJSConsoleHandler handler = GetConsoleHandler(arguments);
                contents = newline ? handler.FormatLine(arguments) : handler.FormatString(arguments);
            }
            arguments.SetReturnValue(contents ?? string.Empty);
        }

        private static void log(IntPtr info)
        {
            internalprintstring(info, 0);
        }

        private static void printf(IntPtr info)
        {
            internalprintstring(info, 1);
        }

        private static void println(IntPtr info)
        {
            internalprintstring(info, 2);
        }

        private static void system(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            if (arguments.Length > 0)
            {
                NSJSString cmd = arguments[0] as NSJSString;
                string message = cmd.Value;
                if (!string.IsNullOrEmpty(message))
                {
                    NSJSConsoleHandler handler = GetConsoleHandler(arguments);
                    handler.SystemCall(arguments, message);
                }
            }
        }
    }
}
