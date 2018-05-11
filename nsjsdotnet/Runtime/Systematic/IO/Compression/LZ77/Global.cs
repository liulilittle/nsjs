﻿namespace nsjsdotnet.Runtime.Systematic.IO.Compression.LZ77
{
    static class Global
    {
        public static NSJSVirtualMachine.ExtensionObjectTemplate GlobalTemplate
        {
            get;
            private set;
        }

        static Global()
        {
            NSJSVirtualMachine.ExtensionObjectTemplate owner = new NSJSVirtualMachine.ExtensionObjectTemplate();
            GlobalTemplate = owner;

            owner.AddObject("GZip", GZip.Current.ClassTemplate);
            owner.AddObject("Deflate", Deflate.Current.ClassTemplate);
        }
    }
}