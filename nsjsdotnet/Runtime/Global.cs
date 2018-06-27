namespace nsjsdotnet.Runtime
{
    using nsjsdotnet.Core;
    using nsjsdotnet.Core.IO;
    using nsjsdotnet.Core.Net;
    using System;
    using System.IO;
    using System.Windows.Forms;

    static class Global
    {
        private static NSJSFunctionCallback g_AlertProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Alert);
        private static NSJSFunctionCallback g_UsingProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Using);
        private static NSJSFunctionCallback g_RequireProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Require);

        private static void Using(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            NSJSObject self = arguments.Length > 0 ? arguments[0] as NSJSObject : null;
            NSJSFunction function = arguments.Length > 1 ? arguments[1] as NSJSFunction : null;
            if (function != null)
            {
                function.Call(self);
            }
            if (self != null)
            {
                NSJSFunction close = self.Get("Dispose") as NSJSFunction;
                if (close == null)
                {
                    close = self.Get("Close") as NSJSFunction;
                }
                if (close != null)
                {
                    close.Call();
                }
            }
        }

        public const int MAX_PATH = 260;
        public static readonly IntPtr NULL = IntPtr.Zero;

        private static void Alert(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            string text = ValueAuxiliary.ToString(arguments.Length > 0 ? arguments[0] : null) ?? string.Empty;
            string caption = ValueAuxiliary.ToString(arguments.Length > 1 ? arguments[1] : null) ?? string.Empty;
            int buttons = ValueAuxiliary.ToInt32(arguments.Length > 2 ? arguments[2] : null);
            int icon = ValueAuxiliary.ToInt32(arguments.Length > 3 ? arguments[3] : null);
            DialogResult result = MessageBox.Show(text, caption, (MessageBoxButtons)buttons, (MessageBoxIcon)icon);
            arguments.SetReturnValue(unchecked((int)result));
        }

        public static void Initialization(NSJSVirtualMachine machine)
        {
            if (machine == null)
            {
                throw new ArgumentNullException("machine");
            }
            Timer.Initialization(machine);
            var extension = machine.GetExtension();
            extension.Set("console", Console.ClassTemplate);
            extension.Set("alert", g_AlertProc);
            extension.Set("using", g_UsingProc);
            extension.Set("require", g_RequireProc);
            extension.Set("System", NSJSVirtualMachine.GetSystemTemplate());
        }

        private static void Require(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            NSJSString rawUri = arguments.Length > 0 ? arguments[0] as NSJSString : null;
            if (rawUri == null || rawUri.DateType != NSJSDataType.kString)
            {
                arguments.SetReturnValue(false);
            }
            else
            {
                NSJSVirtualMachine machine = arguments.VirtualMachine;
                NSJSObject global = machine.Global;
                string path = rawUri.Value;
                string source;
                if (string.IsNullOrEmpty(path))
                {
                    arguments.SetReturnValue(false);
                }
                else
                {
                    int index = path.IndexOf('#');
                    if (index > -1)
                    {
                        path = path.Substring(0, index);
                    }
                    index = path.IndexOf('?');
                    if (index > -1)
                    {
                        path = path.Substring(0, index);
                    }
                    if (!FileAuxiliary.TryReadAllText(path, out source) && !HttpAuxiliary.TryReadAllText(path, out source))
                    {
                        arguments.SetReturnValue(false);
                    }
                    else
                    {
                        if (File.Exists(path))
                        {
                            path = Path.GetFullPath(path);
                        }
                        arguments.SetReturnValue(machine.Run(source, path));
                    }
                }
            }
        }
    }
}
