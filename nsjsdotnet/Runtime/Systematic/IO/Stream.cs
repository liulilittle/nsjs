namespace nsjsdotnet.Runtime.Systematic.IO
{
    using global::System;
    using global::System.IO;
    using nsjsdotnet.Core;
    using BaseStream = global::System.IO.Stream;

    sealed class Stream
    {
        private static readonly NSJSFunctionCallback m_LengthProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Length);
        private static readonly NSJSFunctionCallback m_PositionProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Position);
        private static readonly NSJSFunctionCallback m_SeekProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Seek);
        private static readonly NSJSFunctionCallback m_ReadProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Read);
        private static readonly NSJSFunctionCallback m_WriteProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Write);
        private static readonly NSJSFunctionCallback m_FlushProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Flush);
        private static readonly NSJSFunctionCallback m_DisposeProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Dispose);
        private static readonly NSJSFunctionCallback m_ReadBytesProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(ReadBytes);
        private static readonly NSJSFunctionCallback m_CopyToProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(CopyTo);

        private static void CopyTo(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            BaseStream stream = NSJSKeyValueCollection.Get<BaseStream>(arguments.This);
            if (stream == null)
            {
                Throwable.ObjectDisposedException(arguments.VirtualMachine);
            }
            else
            {
                BaseStream destination = NSJSKeyValueCollection.Get<BaseStream>(arguments.Length > 0 ? arguments[0] as NSJSObject : null);
                if (destination == null)
                {
                    Throwable.ArgumentNullException(arguments.VirtualMachine);
                }
                else
                {
                    try
                    {
                        stream.CopyTo(destination);
                    }
                    catch (Exception e)
                    {
                        Throwable.Exception(arguments.VirtualMachine, e);
                    }
                }
            }
        }

        public static BaseStream Get(NSJSObject stream)
        {
            return NSJSKeyValueCollection.Get<BaseStream>(stream);
        }

        public static NSJSObject New(NSJSVirtualMachine machine, BaseStream stream)
        {
            if (machine == null || stream == null)
            {
                return null;
            }
            NSJSObject o = NSJSObject.New(machine);

            o.Set("CanWrite", stream.CanWrite);
            o.Set("CanSeek", stream.CanSeek);
            o.Set("CanRead", stream.CanRead);

            o.DefineProperty("Length", m_LengthProc, (NSJSFunctionCallback)null);
            o.DefineProperty("Position", m_PositionProc, m_PositionProc);
            o.Set("Seek", m_SeekProc);
            o.Set("CopyTo", m_CopyToProc);

            o.Set("Read", m_ReadProc);
            o.Set("Write", m_WriteProc);
            o.Set("Flush", m_FlushProc);
            o.Set("ReadBytes", m_ReadBytesProc);

            o.Set("Close", m_DisposeProc);
            o.Set("Dispose", m_DisposeProc);

            NSJSKeyValueCollection.Set(o, stream);
            return o;
        }

        private static void Dispose(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            BaseStream stream;
            NSJSKeyValueCollection.Release(arguments.This, out stream);
            if (stream != null)
            {
                stream.Dispose();
            }
        }

        private static void ReadBytes(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            BaseStream stream = NSJSKeyValueCollection.Get<BaseStream>(arguments.This);
            if (stream == null)
            {
                Throwable.ObjectDisposedException(arguments.VirtualMachine);
            }
            else
            {
                NSJSValue result = null;
                Exception exception = null;
                if (arguments.Length > 0)
                {
                    int max = ((arguments[0] as NSJSInt32)?.Value).GetValueOrDefault();
                    if (max >= 0)
                    {
                        try
                        {
                            byte[] ch = new byte[max];
                            int len = stream.Read(ch, 0, max);
                            result = NSJSUInt8Array.New(arguments.VirtualMachine, ch, len);
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
                    arguments.SetReturnValue(NSJSValue.Undefined(arguments.VirtualMachine));
                }
            }
        }

        private static void Read(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            BaseStream stream = NSJSKeyValueCollection.Get<BaseStream>(arguments.This);
            if (stream == null)
            {
                Throwable.ObjectDisposedException(arguments.VirtualMachine);
            }
            else
            {
                Exception exception = null;
                int? len = null;
                if (arguments.Length > 0)
                {
                    NSJSUInt8Array buffer = arguments[0] as NSJSUInt8Array;
                    if (buffer != null)
                    {
                        int? count = null;
                        int offset = 0;
                        switch (arguments.Length)
                        {
                            case 2:
                                count = ((arguments[1] as NSJSInt32)?.Value).GetValueOrDefault();
                                break;
                            case 3:
                                offset = ((arguments[1] as NSJSInt32)?.Value).GetValueOrDefault();
                                count = (arguments[2] as NSJSInt32)?.Value;
                                break;
                        }
                        if (offset < 0)
                        {
                            offset = 0;
                        }
                        if (count == null)
                        {
                            count = buffer.Length;
                        }
                        if ((offset + count) > buffer.Length)
                        {
                            count = (buffer.Length - offset);
                        }
                        if (offset < 0 || offset >= count)
                        {
                            len = 0;
                        }
                        else
                        {
                            try
                            {
                                byte[] cch = new byte[count.Value];
                                len = stream.Read(cch, offset, cch.Length);
                                if (len > 0)
                                {
                                    for (int i = offset; i < len; i++)
                                    {
                                        buffer[i] = cch[i];
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                exception = e;
                            }
                        }
                    }
                }
                if (exception != null)
                {
                    Throwable.Exception(arguments.VirtualMachine, exception);
                }
                else
                {
                    arguments.SetReturnValue(len.GetValueOrDefault());
                }
            }
        }

        private static void Write(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            BaseStream stream = NSJSKeyValueCollection.Get<BaseStream>(arguments.This);
            if (stream == null)
            {
                Throwable.ObjectDisposedException(arguments.VirtualMachine);
            }
            else
            {
                Exception exception = null;
                bool success = false;
                if (arguments.Length > 0)
                {
                    NSJSUInt8Array buffer = arguments[0] as NSJSUInt8Array;
                    if (buffer != null)
                    {
                        int? count = null;
                        int offset = 0;
                        switch (arguments.Length)
                        {
                            case 2:
                                count = (arguments[1] as NSJSInt32)?.Value;
                                break;
                            case 3:
                                offset = ((arguments[1] as NSJSInt32)?.Value).GetValueOrDefault();
                                count = (arguments[2] as NSJSInt32)?.Value;
                                break;
                        }
                        if (offset < 0)
                        {
                            offset = 0;
                        }
                        if (count == null)
                        {
                            count = buffer.Length;
                        }
                        if ((offset + count) > buffer.Length)
                        {
                            count = (buffer.Length - offset);
                        }
                        if (!(offset < 0 || offset >= count))
                        {
                            try
                            {
                                stream.Write(buffer.Buffer, offset, count.Value);
                            }
                            catch (Exception e)
                            {
                                exception = e;
                            }
                        }
                    }
                }
                if (exception != null)
                {
                    Throwable.Exception(arguments.VirtualMachine, exception);
                }
                else
                {
                    arguments.SetReturnValue(success);
                }
            }
        }

        private static void Flush(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            BaseStream stream = NSJSKeyValueCollection.Get<BaseStream>(arguments.This);
            if (stream == null)
            {
                Throwable.ObjectDisposedException(arguments.VirtualMachine);
            }
            else
            {
                Exception exception = null;
                bool success = false;
                try
                {
                    stream.Flush();
                }
                catch (Exception e)
                {
                    exception = e;
                }
                if (exception != null)
                {
                    Throwable.Exception(arguments.VirtualMachine, exception);
                }
                else
                {
                    arguments.SetReturnValue(success);
                }
            }
        }

        private static void Seek(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            BaseStream stream = NSJSKeyValueCollection.Get<BaseStream>(arguments.This);
            if (stream == null)
            {
                Throwable.ObjectDisposedException(arguments.VirtualMachine);
            }
            else
            {
                Exception exception = null;
                int offset = 0;
                int? result = null;
                if (stream != null)
                {
                    SeekOrigin origin = SeekOrigin.Begin;
                    if (arguments.Length > 0)
                    {
                        offset = ((arguments[0] as NSJSInt32)?.Value).GetValueOrDefault();
                    }
                    if (arguments.Length > 1)
                    {
                        origin = (SeekOrigin)((arguments[1] as NSJSInt32)?.Value).GetValueOrDefault();
                    }
                    try
                    {
                        result = unchecked((int)stream.Seek(offset, origin));
                    }
                    catch (Exception e)
                    {
                        exception = e;
                    }
                }
                if (exception != null)
                {
                    Throwable.Exception(arguments.VirtualMachine, exception);
                }
                else
                {
                    arguments.SetReturnValue(result.GetValueOrDefault());
                }
            }
        }

        private static void Position(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            BaseStream stream = NSJSKeyValueCollection.Get<BaseStream>(arguments.This);
            if (stream == null)
            {
                Throwable.ObjectDisposedException(arguments.VirtualMachine);
            }
            else if (arguments.Length <= 0)
            {
                arguments.SetReturnValue(unchecked((int)stream.Position));
            }
            else
            {
                NSJSInt32 value = arguments[0] as NSJSInt32;
                if (value == null)
                {
                    arguments.SetReturnValue(false);
                }
                else
                {
                    try
                    {
                        stream.Position = value.Value;
                        arguments.SetReturnValue(true);
                    }
                    catch (Exception e)
                    {
                        Throwable.Exception(arguments.VirtualMachine, e);
                    }
                }
            }
        }

        private static void Length(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            BaseStream stream = NSJSKeyValueCollection.Get<BaseStream>(arguments.This);
            if (stream == null)
            {
                Throwable.ObjectDisposedException(arguments.VirtualMachine);
            }
            else
            {
                arguments.SetReturnValue(unchecked((int)stream.Length));
            }
        }
    }
}
