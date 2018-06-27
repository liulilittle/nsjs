namespace nsjsdotnet.Runtime.Systematic.IO.Compression.LZ77
{
    using global::System;
    using nsjsdotnet.Core;
    using nsjsdotnet.Core.IO.Compression;

    class LZ77Algorithm
    {
        public NSJSVirtualMachine.ExtensionObjectTemplate ClassTemplate
        {
            get;
            private set;
        }

        private LZ77Auxiliary.LZ77Algorithm algorithm;

        public LZ77Algorithm(LZ77Auxiliary.LZ77Algorithm algorithm)
        {
            NSJSVirtualMachine.ExtensionObjectTemplate owner = new NSJSVirtualMachine.ExtensionObjectTemplate();
            ClassTemplate = owner;
            this.algorithm = algorithm;
            owner.Set("Decompress", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Decompress));
            owner.Set("Compress", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Compress));
        }

        private void Compress(IntPtr info)
        {
            DecompressOrCompress(info, false);
        }

        private void Decompress(IntPtr info)
        {
            DecompressOrCompress(info, true);
        }

        private void DecompressOrCompress(IntPtr info, bool decompress)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            byte[] result = null;
            Exception exception = null;
            if (arguments.Length > 0)
            {
                byte[] buffer = (arguments[0] as NSJSUInt8Array)?.Buffer;
                if (buffer != null)
                {
                    try
                    {
                        if (decompress)
                        {
                            result = LZ77Auxiliary.Decompress(buffer, algorithm);
                        }
                        else
                        {
                            result = LZ77Auxiliary.Compress(buffer, algorithm);
                        }
                    }
                    catch (Exception e)
                    {
                        exception = e;
                    }
                }
            }
            if (exception == null)
            {
                arguments.SetReturnValue(result);
            }
            else
            {
                Throwable.Exception(arguments.VirtualMachine, exception);
            }
        }
    }
}
