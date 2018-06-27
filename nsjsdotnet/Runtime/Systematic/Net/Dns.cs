namespace nsjsdotnet.Runtime.Systematic.Net
{
    using global::System;
    using global::System.Net;
    using nsjsdotnet.Core;
    using DNS = global::System.Net.Dns;

    static class Dns
    {
        public static NSJSVirtualMachine.ExtensionObjectTemplate ClassTemplate
        {
            get;
            private set;
        }

        static Dns()
        {
            NSJSVirtualMachine.ExtensionObjectTemplate owner = new NSJSVirtualMachine.ExtensionObjectTemplate();
            ClassTemplate = owner;
            owner.Set("GetHostEntry", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(GetHostEntry));
            owner.Set("GetHostName", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(GetHostName));
            owner.Set("GetHostAddresses", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(GetHostAddresses));
        }

        private static void GetHostEntry(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            string hostNameOrAddress = arguments.Length > 0 ? (arguments[0] as NSJSString)?.Value : null;
            try
            {
                if (hostNameOrAddress == null)
                {
                    Throwable.ArgumentNullException(arguments.VirtualMachine);
                }
                else if (hostNameOrAddress.Length <= 0)
                {
                    Throwable.ArgumentException(arguments.VirtualMachine);
                }
                else
                {
                    IPHostEntry host = DNS.GetHostEntry(hostNameOrAddress);
                    arguments.SetReturnValue(ObjectAuxiliary.ToObject(arguments.VirtualMachine, host));
                }
            }
            catch (Exception e)
            {
                Throwable.Exception(arguments.VirtualMachine, e);
            }
        }

        private static void GetHostName(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            try
            {
                arguments.SetReturnValue(DNS.GetHostName());
            }
            catch (Exception e)
            {
                Throwable.Exception(arguments.VirtualMachine, e);
            }
        }

        private static void GetHostAddresses(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            string hostNameOrAddress = arguments.Length > 0 ? (arguments[0] as NSJSString)?.Value : null;
            try
            {
                if (hostNameOrAddress == null)
                {
                    Throwable.ArgumentNullException(arguments.VirtualMachine);
                }
                else if (hostNameOrAddress.Length <= 0)
                {
                    Throwable.ArgumentException(arguments.VirtualMachine);
                }
                else
                {
                    IPAddress[] addresses = DNS.GetHostAddresses(hostNameOrAddress);
                    arguments.SetReturnValue(ArrayAuxiliary.ToArray(arguments.VirtualMachine, addresses));
                }
            }
            catch (Exception e)
            {
                Throwable.Exception(arguments.VirtualMachine, e);
            }
        }
    }
}
