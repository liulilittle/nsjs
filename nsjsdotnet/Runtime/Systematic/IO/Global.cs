namespace nsjsdotnet.Runtime.Systematic.IO
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
            owner.Set("File", File.ClassTemplate);
            owner.Set("FileStream", FileStream.ClassTemplate);
            owner.Set("Directory", Directory.ClassTemplate);
            owner.Set("MemoryStream", MemoryStream.ClassTemplate);
        }
    }
}
