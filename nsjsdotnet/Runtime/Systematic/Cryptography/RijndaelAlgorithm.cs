namespace nsjsdotnet.Runtime.Systematic.Cryptography
{
    using global::System;
    using nsjsdotnet.Core;

    abstract class RijndaelAlgorithm
    {
        public NSJSVirtualMachine.ExtensionObjectTemplate ClassTemplate
        {
            get;
            private set;
        }

        private readonly NSJSFunctionCallback m_EncryptProc;
        private readonly NSJSFunctionCallback m_DecryptProc;
        private readonly NSJSFunctionCallback m_DisposeProc;

        public RijndaelAlgorithm()
        {
            NSJSVirtualMachine.ExtensionObjectTemplate owner = new NSJSVirtualMachine.ExtensionObjectTemplate();
            ClassTemplate = owner;
            ClassTemplate.Set("New", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(New));

            this.m_EncryptProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(this.Encrypt);
            this.m_DecryptProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(this.Decrypt);
            this.m_DisposeProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(this.Dispose);
        }

        private NSJSObject New(NSJSVirtualMachine machine, RijndaelCryptoServiceProvider provider)
        {
            NSJSObject o = NSJSObject.New(machine);
            o.Set("Encrypt", this.m_EncryptProc);
            o.Set("Decrypt", this.m_DecryptProc);
            o.Set("Dispose", this.m_DisposeProc);
            NSJSKeyValueCollection.Set(o, provider);
            return o;
        }

        private void New(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            NSJSValue result = null;
            Exception exception = null;
            if (arguments.Length > 1)
            {
                byte[] key = (arguments[0] as NSJSUInt8Array)?.Buffer;
                byte[] IV = (arguments[1] as NSJSUInt8Array)?.Buffer;
                if (key != null && IV != null)
                {
                    try
                    {
                        RijndaelCryptoServiceProvider provider = New(key, IV, arguments);
                        if (provider != null)
                        {
                            result = New(arguments.VirtualMachine, provider);
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

        protected virtual RijndaelCryptoServiceProvider New(byte[] Key, byte[] IV, NSJSFunctionCallbackInfo arguments)
        {
            return this.New(Key, IV);
        }

        protected abstract RijndaelCryptoServiceProvider New(byte[] Key, byte[] IV);

        private void Dispose(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            RijndaelCryptoServiceProvider provider;
            NSJSKeyValueCollection.Release<RijndaelCryptoServiceProvider>(arguments.This, out provider);
            if (provider != null)
            {
                provider.Dispose();
            }
        }

        private void EncryptOrDecrypt(IntPtr info, bool decrypt)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            RijndaelCryptoServiceProvider provider = NSJSKeyValueCollection.Get<RijndaelCryptoServiceProvider>(arguments.This);
            if (provider == null)
            {
                Throwable.ObjectDisposedException(arguments.VirtualMachine);
            }
            else
            {
                byte[] result = null;
                Exception exception = null;
                if (arguments.Length > 0)
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
                    Throwable.ArgumentNullException(arguments.VirtualMachine, exception);
                }
            }
        }
        
        private void Encrypt(IntPtr info)
        {
            EncryptOrDecrypt(info, false);
        }

        private void Decrypt(IntPtr info)
        {
            EncryptOrDecrypt(info, true);
        }
    }
}
