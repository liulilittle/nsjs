namespace nsjsdotnet.Runtime.Systematic.Cryptography
{
    using global::System;
    using global::System.Text;
    using nsjsdotnet.Core;
    using HASHAlgorithm = global::System.Security.Cryptography.HashAlgorithm;
    using NSJSEncoding = nsjsdotnet.Runtime.Systematic.Text.Encoding;

    abstract class HashAlgorithm
    {
        public NSJSVirtualMachine.ExtensionObjectTemplate ClassTemplate
        {
            get;
            private set;
        }

        public HashAlgorithm()
        {
            NSJSVirtualMachine.ExtensionObjectTemplate owner = new NSJSVirtualMachine.ExtensionObjectTemplate();
            ClassTemplate = owner;
            owner.AddFunction("ComputeHashValue", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(ComputeHashValue));
            owner.AddFunction("ComputeHashString", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(ComputeHashString));
        }

        protected abstract HASHAlgorithm New();

        private void ComputeHashValueOrString(IntPtr info, bool hashValue)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            byte[] buffer = null;
            if (arguments.Length > 0)
            {
                buffer = (arguments[0] as NSJSUInt8Array)?.Buffer;
                if (buffer == null)
                {
                    string s = (arguments[0] as NSJSString).Value;
                    if (s != null)
                    {
                        Encoding encoding = NSJSEncoding.DefaultEncoding;
                        if (arguments.Length > 1)
                        {
                            encoding = NSJSEncoding.GetEncoding(arguments[1]);
                        }
                        buffer = encoding.GetBytes(s);
                    }
                }
            }
            if (buffer == null)
            {
                Throwable.ArgumentNullException(arguments.VirtualMachine);
            }
            else
            {
                try
                {
                    using (HASHAlgorithm hash = New())
                    {
                        buffer = hash.ComputeHash(buffer);
                        if (hashValue)
                        {
                            arguments.SetReturnValue(buffer);
                        }
                        else
                        {
                            string s = string.Empty;
                            for (int i = 0; i < buffer.Length; i++)
                            {
                                s += buffer[i].ToString("X2");
                            }
                            arguments.SetReturnValue(s);
                        }
                    }
                }
                catch (Exception e)
                {
                    Throwable.Exception(arguments.VirtualMachine, e);
                }
            }
        }

        private void ComputeHashString(IntPtr info)
        {
            ComputeHashValueOrString(info, false);
        }

        private void ComputeHashValue(IntPtr info)
        {
            ComputeHashValueOrString(info, true);
        }
    }
}
