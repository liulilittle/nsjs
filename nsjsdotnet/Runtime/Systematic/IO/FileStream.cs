namespace nsjsdotnet.Runtime.Systematic.IO
{
    using global::System;
    using global::System.IO;
    using nsjsdotnet.Core;
    using FStream = global::System.IO.FileStream;

    sealed class FileStream
    {
        public static NSJSVirtualMachine.ExtensionObjectTemplate ClassTemplate
        {
            get;
            private set;
        }

        static FileStream()
        {
            NSJSVirtualMachine.ExtensionObjectTemplate owner = new NSJSVirtualMachine.ExtensionObjectTemplate();
            ClassTemplate = owner;
            owner.Set("New", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(New));
        }

        private static void New(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            string path = arguments.Length > 0 ? (arguments[0] as NSJSString)?.Value : null;
            FileMode mode = FileMode.Open;
            FileAccess access = FileAccess.ReadWrite;
            if (arguments.Length > 1)
            {
                NSJSInt32 n = arguments[1] as NSJSInt32;
                if (n != null)
                {
                    mode = (FileMode)n.Value;
                }
                n = arguments.Length > 2 ? arguments[2] as NSJSInt32 : null;
                if (n != null)
                {
                    access = (FileAccess)n.Value;
                }
            }
            Exception exception = null;
            FStream stream = null;
            try
            {
                stream = new FStream(path, mode, access);
            }
            catch (Exception e)
            {
                exception = e;
            }
            if (exception == null)
            {
                arguments.SetReturnValue(Stream.New(arguments.VirtualMachine, stream));
            }
            else
            {
                Throwable.Exception(arguments.VirtualMachine, exception);
            }
        }
    }
}
