namespace nsjsdotnet.Runtime.Systematic.IO
{
    using global::System;
    using global::System.Text;
    using nsjsdotnet.Core;
    using nsjsdotnet.Core.IO;
    using System.IO;
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

            owner.Set("Exists", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Exists));
            owner.Set("Delete", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Delete));
            owner.Set("Move", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Move));
            owner.Set("Copy", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Copy));

            owner.Set("WriteAllBytes", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(WriteAllBytes));
            owner.Set("WriteAllText", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(WriteAllText));

            owner.Set("GetFileLength", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(GetFileLength));
            owner.Set("ReadAllText", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(ReadAllText));
            owner.Set("ReadAllBytes", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(ReadAllBytes));

            owner.Set("GetCreationTime", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(GetCreationTime));
            owner.Set("GetCreationTimeUtc", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(GetCreationTimeUtc));
            owner.Set("GetLastAccessTime", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(GetLastAccessTime));
            owner.Set("GetLastAccessTimeUtc", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(GetLastAccessTimeUtc));
            owner.Set("GetLastWriteTime", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(GetLastWriteTime));
            owner.Set("GetLastWriteTimeUtc", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(GetLastWriteTimeUtc));
            owner.Set("GetAttributes", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(GetAttributes));

            owner.Set("SetCreationTime", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(SetCreationTime));
            owner.Set("SetCreationTimeUtc", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(SetCreationTimeUtc));
            owner.Set("SetLastAccessTime", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(SetLastAccessTime));
            owner.Set("SetLastAccessTimeUtc", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(SetLastAccessTimeUtc));
            owner.Set("SetLastWriteTime", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(SetLastWriteTime));
            owner.Set("SetLastWriteTimeUtc", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(SetLastWriteTimeUtc));
            owner.Set("SetAttributes", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(SetAttributes));
        }

        private static void SetCreationTime(IntPtr info)
        {
            CallSetFileInfo(info, (arguments, path, datetime) => FILE.SetCreationTime(path, datetime));
        }

        private static void SetCreationTimeUtc(IntPtr info)
        {
            CallSetFileInfo(info, (arguments, path, datetime) => FILE.SetCreationTimeUtc(path, datetime));
        }

        private static void SetLastAccessTime(IntPtr info)
        {
            CallSetFileInfo(info, (arguments, path, datetime) => FILE.SetLastAccessTime(path, datetime));
        }

        private static void SetLastAccessTimeUtc(IntPtr info)
        {
            CallSetFileInfo(info, (arguments, path, datetime) => FILE.SetLastAccessTimeUtc(path, datetime));
        }

        private static void SetLastWriteTime(IntPtr info)
        {
            CallSetFileInfo(info, (arguments, path, datetime) => FILE.SetLastWriteTime(path, datetime));
        }

        private static void SetLastWriteTimeUtc(IntPtr info)
        {
            CallSetFileInfo(info, (arguments, path, datetime) => FILE.SetLastWriteTimeUtc(path, datetime));
        }

        private static void SetAttributes(IntPtr info)
        {
            CallSetFileInfo(info, (arguments, path, value) => FILE.SetAttributes(path, unchecked((FileAttributes)value)));
        }

        private static void GetCreationTime(IntPtr info)
        {
            CallGetFileInfo(info, (arguments, path) => arguments.SetReturnValue(FILE.GetCreationTime(path)));
        }

        private static void GetCreationTimeUtc(IntPtr info)
        {
            CallGetFileInfo(info, (arguments, path) => arguments.SetReturnValue(FILE.GetCreationTimeUtc(path)));
        }

        private static void GetLastAccessTime(IntPtr info)
        {
            CallGetFileInfo(info, (arguments, path) => arguments.SetReturnValue(FILE.GetLastAccessTime(path)));
        }

        private static void GetLastAccessTimeUtc(IntPtr info)
        {
            CallGetFileInfo(info, (arguments, path) => arguments.SetReturnValue(FILE.GetLastAccessTimeUtc(path)));
        }

        private static void GetLastWriteTime(IntPtr info)
        {
            CallGetFileInfo(info, (arguments, path) => arguments.SetReturnValue(FILE.GetLastWriteTime(path)));
        }

        private static void GetLastWriteTimeUtc(IntPtr info)
        {
            CallGetFileInfo(info, (arguments, path) => arguments.SetReturnValue(FILE.GetLastWriteTimeUtc(path)));
        }

        private static void GetAttributes(IntPtr info)
        {
            CallGetFileInfo(info, (arguments, path) => arguments.SetReturnValue(unchecked((int)FILE.GetAttributes(path))));
        }

        private static void CallSetFileInfo(IntPtr info, Action<NSJSFunctionCallbackInfo, string, int> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            CallGetFileInfo(info, (arguments, path) =>
            {
                NSJSInt32 n = arguments.Length > 0 ? arguments[0] as NSJSInt32 : null;
                if (n == null)
                {
                    Throwable.ArgumentNullException(arguments.VirtualMachine);
                }
                else
                {
                    callback(arguments, path, n.Value);
                }
            });
        }

        private static void CallSetFileInfo(IntPtr info, Action<NSJSFunctionCallbackInfo, string, DateTime> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            CallGetFileInfo(info, (arguments, path) =>
            {
                NSJSDateTime datetime = arguments.Length > 0 ? arguments[0] as NSJSDateTime : null;
                if (datetime == null)
                {
                    Throwable.ArgumentNullException(arguments.VirtualMachine);
                }
                else
                {
                    callback(arguments, path, datetime.Value);
                }
            });
        }

        private static void CallGetFileInfo(IntPtr info, Action<NSJSFunctionCallbackInfo, string> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            string path = arguments.Length > 0 ? (arguments[0] as NSJSString)?.Value : null;
            if (!FILE.Exists(path))
            {
                Throwable.FileNotFoundException(arguments.VirtualMachine);
            }
            else
            {
                callback(arguments, path);
            }
        }

        private static void GetFileLength(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            string path = arguments.Length > 0 ? (arguments[0] as NSJSString)?.Value : null;
            arguments.SetReturnValue(FileAuxiliary.GetFileLength64(path));
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
                    encodings = NSJSEncoding.GetEncoding(arguments[1] as NSJSObject);
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
                    encodings = NSJSEncoding.GetEncoding(arguments[2] as NSJSObject);
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
