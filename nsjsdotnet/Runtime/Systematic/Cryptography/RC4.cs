namespace nsjsdotnet.Runtime.Systematic.Cryptography
{
    using global::System;
    using nsjsdotnet.Core;
    using RC4CSP = nsjsdotnet.Core.Cryptography.RC4;

    sealed class RC4
    {
        public static NSJSVirtualMachine.ExtensionObjectTemplate ClassTemplate
        {
            get;
            private set;
        }

        private static readonly NSJSFunctionCallback m_EncryptProc;
        private static readonly NSJSFunctionCallback m_DecryptProc;
        private static readonly NSJSFunctionCallback m_DisposeProc;

        static RC4()
        {
            NSJSVirtualMachine.ExtensionObjectTemplate owner = new NSJSVirtualMachine.ExtensionObjectTemplate();
            ClassTemplate = owner;
            owner.AddFunction("New", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(New));

            m_EncryptProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Encrypt);
            m_DecryptProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Decrypt);
            m_DisposeProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Dispose);
        }

        private static void EncryptOrDecrypt(IntPtr info, bool decrypt)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            RC4CSP provider = NSJSKeyValueCollection.Get<RC4CSP>(arguments.This);
            if (provider == null)
            {
                Throwable.ObjectDisposedException(arguments.VirtualMachine);
            }
            else
            {
                Exception exception = null;
                byte[] result = null;
                if (provider != null && arguments.Length > 0)
                {
                    byte[] buffer = (arguments[0] as NSJSUInt8Array)?.Buffer;
                    if (buffer != null)
                    {
                        int ofs = 0;
                        if (arguments.Length > 1)
                        {
                            ofs = ((arguments[1] as NSJSInt32)?.Value).GetValueOrDefault();
                        }
                        int count = buffer.Length;
                        if (arguments.Length > 2)
                        {
                            count = ((arguments[2] as NSJSInt32)?.Value).GetValueOrDefault();
                        }
                        try
                        {
                            if (decrypt)
                            {
                                result = provider.Decrypt(buffer, ofs, count);
                            }
                            else
                            {
                                result = provider.Encrypt(buffer, ofs, count);
                            }
                        }
                        catch (Exception e)
                        {
                            exception = e;
                        }
                    }
                }
                if (exception != null)
                {
                    Throwable.Exception(arguments.VirtualMachine, exception);
                }
                else if (result != null)
                {
                    arguments.SetReturnValue(result);
                }
                else
                {
                    Throwable.ArgumentNullException(arguments.VirtualMachine);
                }
            }
        }

        private static void Encrypt(IntPtr info)
        {
            EncryptOrDecrypt(info, false);
        }

        private static void Decrypt(IntPtr info)
        {
            EncryptOrDecrypt(info, true);
        }

        private static void Dispose(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            RC4CSP provider;
            NSJSKeyValueCollection.Release(arguments.This, out provider);
        }

        private static NSJSObject New(NSJSVirtualMachine machine, RC4CSP provider)
        {
            NSJSObject o = NSJSObject.New(machine);
            o.Set("Encrypt", m_EncryptProc);
            o.Set("Decrypt", m_DecryptProc);
            o.Set("Dispose", m_DisposeProc);
            NSJSKeyValueCollection.Set(o, provider);
            return o;
        }

        private static void New(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            NSJSValue result = null;
            string Key = null;
            if (arguments.Length > 0)
            {
                Key = (arguments[0] as NSJSString)?.Value;
                if (!string.IsNullOrEmpty(Key))
                {
                    byte[] SBox = arguments.Length > 1 ? (arguments[1] as NSJSUInt8Array)?.Buffer : null;
                    int MaxbitWidth = RC4CSP.DefaultMaxbitWidth;
                    if (arguments.Length > 2)
                    {
                        MaxbitWidth = ((arguments[2] as NSJSInt32)?.Value).GetValueOrDefault();
                    }
                    if (MaxbitWidth < 0)
                    {
                        MaxbitWidth = 0;
                    }
                    if (SBox == null)
                    {
                        SBox = RC4CSP.SBox(Key, MaxbitWidth);
                    }
                    result = New(arguments.VirtualMachine, new RC4CSP(Key, SBox));
                }
            }
            if (result != null)
            {
                arguments.SetReturnValue(result);
            }
            else if (Key != null)
            {
                Throwable.ArgumentException(arguments.VirtualMachine);
            }
            else
            {
                Throwable.ArgumentNullException(arguments.VirtualMachine);
            }
        }
    }
}
