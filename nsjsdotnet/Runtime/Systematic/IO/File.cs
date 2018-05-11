namespace nsjsdotnet.Runtime.Systematic.IO
{
    using global::System;
    using global::System.Text;
    using nsjsdotnet.Core;
    using FILE = global::System.IO.File;
    using NSJSEncoding = nsjsdotnet.Runtime.Systematic.Text.Encoding;

    static class File
    {
        public static NSJSVirtualMachine.ExtensionObjectTemplate ClassTemplate
        {
            get;
            private set;
        }

        static File()
        {
            NSJSVirtualMachine.ExtensionObjectTemplate owner = new NSJSVirtualMachine.ExtensionObjectTemplate();
            ClassTemplate = owner;

            owner.AddFunction("Exists", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Exists));
            owner.AddFunction("Delete", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Delete));
            owner.AddFunction("Move", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Move));
            owner.AddFunction("Copy", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Copy));

            owner.AddFunction("WriteAllBytes", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(WriteAllBytes));
            owner.AddFunction("WriteAllText", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(WriteAllText));

            owner.AddFunction("ReadAllText", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(ReadAllText));
            owner.AddFunction("ReadAllBytes", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(ReadAllBytes));
        }

        private static void ReadAllBytes(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            string path = arguments.Length > 0 ? (arguments[0] as NSJSString)?.Value : null;
            byte[] buffer = null;
            Exception exception = null;
            if (FILE.Exists(path))
            {
                try
                {
                    buffer = FILE.ReadAllBytes(path);
                }
                catch (Exception e)
                {
                    exception = e;
                }
            }
            if (buffer != null)
            {
                arguments.SetReturnValue(buffer);
            }
            else if (exception == null)
            {
                Throwable.FileNotFoundException(arguments.VirtualMachine);
            }
            else
            {
                Throwable.Exception(arguments.VirtualMachine, exception);
            }
        }

        private static void ReadAllText(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            string path = arguments.Length > 0 ? (arguments[0] as NSJSString)?.Value : null;
            string contents = null;
            Exception exception = null;
            if (FILE.Exists(path))
            {
                Encoding encodings = NSJSEncoding.DefaultEncoding;
                if (arguments.Length > 1)
                {
                    encodings = NSJSEncoding.GetEncoding(arguments[1]);
                }
                try
                {
                    contents = FILE.ReadAllText(path, encodings);
                }
                catch (Exception e)
                {
                    exception = e;
                }
            }
            if (contents != null)
            {
                arguments.SetReturnValue(contents);
            }
            else if (exception == null)
            {
                Throwable.FileNotFoundException(arguments.VirtualMachine);
            }
            else
            {
                Throwable.Exception(arguments.VirtualMachine, exception);
            }
        }

        private static void WriteAllText(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            bool success = false;
            Exception exception = null;
            if (arguments.Length > 1)
            {
                string path = (arguments[0] as NSJSString)?.Value;
                string contents = (arguments[1] as NSJSString)?.Value;
                Encoding encodings = NSJSEncoding.DefaultEncoding;
                if (arguments.Length > 2)
                {
                    encodings = NSJSEncoding.GetEncoding(arguments[2]);
                }
                try
                {
                    FILE.WriteAllText(path, contents, encodings);
                    success = true;
                }
                catch (Exception e)
                {
                    exception = e;
                }
            }
            if (exception == null)
            {
                arguments.SetReturnValue(success);
            }
            else
            {
                Throwable.Exception(arguments.VirtualMachine, exception);
            }
        }

        private static void Exists(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            NSJSString path = arguments.Length > 0 ? arguments[0] as NSJSString : null;
            if (path == null)
            {
                arguments.SetReturnValue(false);
            }
            else
            {
                arguments.SetReturnValue(FILE.Exists(path.Value));
            }
        }

        private static void Delete(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            NSJSString rawUri = arguments.Length > 0 ? arguments[0] as NSJSString : null;
            bool success = false;
            Exception exception = null;
            if (rawUri != null)
            {
                string path = rawUri.Value;
                if (FILE.Exists(path))
                {
                    try
                    {
                        FILE.Delete(path);
                        success = true;
                    }
                    catch (Exception e)
                    {
                        exception = e;
                        success = false;
                    }
                }
            }
            if (exception == null)
            {
                arguments.SetReturnValue(success);
            }
            else
            {
                Throwable.Exception(arguments.VirtualMachine, exception);
            }
        }

        private static void Move(IntPtr info)
        {
            MoveOrCopy(info, false);
        }

        private static void Copy(IntPtr info)
        {
            MoveOrCopy(info, true);
        }

        private static void MoveOrCopy(IntPtr info, bool copyMode)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            bool success = false;
            Exception exception = null;
            do
            {
                if (arguments.Length <= 0)
                {
                    break;
                }
                NSJSString sourceFileName = arguments[0] as NSJSString;
                NSJSString destFileName = arguments[1] as NSJSString;
                if (sourceFileName == null || destFileName == null)
                {
                    break;
                }
                try
                {
                    if (copyMode)
                    {
                        FILE.Copy(sourceFileName.Value, destFileName.Value);
                    }
                    else
                    {
                        FILE.Move(sourceFileName.Value, destFileName.Value);
                    }
                    success = true;
                }
                catch (Exception e)
                {
                    exception = e;
                    success = false;
                }
            } while (false);
            if (exception == null)
            {
                arguments.SetReturnValue(success);
            }
            else
            {
                Throwable.Exception(arguments.VirtualMachine, exception);
            }
        }

        private static void WriteAllBytes(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            NSJSString url = arguments.Length > 0 ? arguments[0] as NSJSString : null;
            NSJSUInt8Array buffer = arguments.Length > 1 ? arguments[1] as NSJSUInt8Array : null;
            bool success = false;
            Exception exception = null;
            if (url != null)
            {
                string path = url.Value;
                byte[] bytes = (buffer == null ? BufferExtension.EmptryBuffer : buffer.Buffer);
                try
                {
                    FILE.WriteAllBytes(path, bytes);
                    success = true;
                }
                catch (Exception e)
                {
                    exception = e;
                    success = false;
                }
            }
            if (exception == null)
            {
                arguments.SetReturnValue(success);
            }
            else
            {
                Throwable.Exception(arguments.VirtualMachine, exception);
            }
        }
    }
}
