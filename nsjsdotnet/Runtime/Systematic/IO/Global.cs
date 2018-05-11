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
            owner.AddObject("File", File.ClassTemplate);
            owner.AddObject("Text", Text.ClassTemplate);
            owner.AddObject("FileStream", FileStream.ClassTemplate);
            owner.AddObject("Directory", Directory.ClassTemplate);
            owner.AddObject("MemoryStream", MemoryStream.ClassTemplate);
        }
    }
}
