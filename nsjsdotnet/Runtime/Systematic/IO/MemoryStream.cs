namespace nsjsdotnet.Runtime.Systematic.IO
{
    using global::System;
    using MEMStream = global::System.IO.MemoryStream;

    sealed class MemoryStream
    {
        public static NSJSVirtualMachine.ExtensionObjectTemplate ClassTemplate
        {
            get;
            private set;
        }

        static MemoryStream()
        {
            NSJSVirtualMachine.ExtensionObjectTemplate owner = new NSJSVirtualMachine.ExtensionObjectTemplate();
            ClassTemplate = owner;
            owner.Set("New", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(New));
        }

        private static void New(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            NSJSInt32 capacity = arguments.Length > 0 ? arguments[0] as NSJSInt32 : null;
            MEMStream ms = null;
            if (capacity != null)
            {
                ms = new MEMStream(capacity.Value);
            }
            else
            {
                ms = new MEMStream();
            }
            arguments.SetReturnValue(NSJSValue.UndefinedMerge(arguments.VirtualMachine, Stream.New(arguments.VirtualMachine, ms)));
        }
    }
}
