namespace nsjsdotnet.Runtime.Systematic.IO
{
    using global::System;
    using global::System.IO;
    using nsjsdotnet.Core;
    using nsjsdotnet.Core.IO;
    using DIRECTORY = global::System.IO.Directory;

    static class Directory
    {
        public static NSJSVirtualMachine.ExtensionObjectTemplate ClassTemplate
        {
            get;
            private set;
        }

        static Directory()
        {
            NSJSVirtualMachine.ExtensionObjectTemplate owner = new NSJSVirtualMachine.ExtensionObjectTemplate();
            ClassTemplate = owner;

            owner.Set("Delete", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Delete));
            owner.Set("Move", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Move));
            owner.Set("Copy", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Copy));
            owner.Set("Exists", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Exists));
            owner.Set("CreateDirectory", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(CreateDirectory));

            owner.Set("GetFiles", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(GetFiles));
            owner.Set("GetDirectories", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(GetDirectories));
        }

        private static void CreateDirectory(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            string path = arguments.Length > 0 ? (arguments[0] as NSJSString).Value : null;
            bool success = false;
            Exception exception = null;
            if (!string.IsNullOrEmpty(path) && !DIRECTORY.Exists(path))
            {
                try
                {
                    DIRECTORY.CreateDirectory(path);
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

        private static void GetDirectories(IntPtr info)
        {
            GetFilesOrDirectories(info, false);
        }

        private static void GetFiles(IntPtr info)
        {
            GetFilesOrDirectories(info, true);
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
                if (DIRECTORY.Exists(path))
                {
                    try
                    {
                        DIRECTORY.Delete(path, true);
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
                    DIRECTORY.Move(sourceFileName.Value, destFileName.Value);
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

        private static void Copy(IntPtr info)
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
                NSJSString sourceDirName = arguments[0] as NSJSString;
                NSJSString destDirName = arguments[1] as NSJSString;
                if (sourceDirName == null || destDirName == null)
                {
                    break;
                }
                try
                {
                    success = DirectoryAuxiliary.Copy(sourceDirName.Value, destDirName.Value);
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

        private static void Exists(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            arguments.SetReturnValue(DIRECTORY.Exists(arguments.Length > 0 ? (arguments[0] as NSJSString).Value : null));
        }

        private static void GetFilesOrDirectories(IntPtr info, bool fileMode)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            string[] buffer = null;
            Exception exception = null;
            try
            {
                if (arguments.Length > 0)
                {
                    string path = (arguments[0] as NSJSString)?.Value;
                    if (DIRECTORY.Exists(path))
                    {
                        if (arguments.Length > 1)
                        {
                            string searchPattern = (arguments[1] as NSJSString)?.Value;
                            if (string.IsNullOrEmpty(searchPattern))
                            {
                                searchPattern = "*";
                            }
                            if (arguments.Length > 2)
                            {
                                int? searchOption = (arguments[2] as NSJSInt32)?.Value;
                                buffer = fileMode ?
                                    DIRECTORY.GetFiles(path, searchPattern, (SearchOption)searchOption.GetValueOrDefault()) :
                                    DIRECTORY.GetDirectories(path, searchPattern, (SearchOption)searchOption.GetValueOrDefault());
                            }
                            else
                            {
                                buffer = fileMode ?
                                    DIRECTORY.GetFiles(path, searchPattern) :
                                    DIRECTORY.GetDirectories(path, searchPattern);
                            }
                        }
                        else
                        {
                            buffer = fileMode ?
                                DIRECTORY.GetFiles(path) :
                                DIRECTORY.GetDirectories(path);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                exception = e;
            }
            if (exception != null)
            {
                Throwable.Exception(arguments.VirtualMachine, exception);
            }
            else if (buffer == null)
            {
                Throwable.DirectoryNotFoundException(arguments.VirtualMachine);
            }
            else
            {
                NSJSArray s = NSJSArray.New(arguments.VirtualMachine, buffer.Length);
                for (int i = 0; i < buffer.Length; i++)
                {
                    s[i] = NSJSString.New(arguments.VirtualMachine, buffer[i]);
                }
                arguments.SetReturnValue(s);
            }
        }
    }
}
