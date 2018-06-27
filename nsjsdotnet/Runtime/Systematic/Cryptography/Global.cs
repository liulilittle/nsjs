namespace nsjsdotnet.Runtime.Systematic.Cryptography
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

            owner.Set("RC4", RC4.ClassTemplate);
            owner.Set("MD5", MD5.Current.ClassTemplate);
            owner.Set("SHA1", SHA1.Current.ClassTemplate);
            owner.Set("SHA256", SHA256.Current.ClassTemplate);
            owner.Set("SHA384", SHA384.Current.ClassTemplate);
            owner.Set("SHA512", SHA512.Current.ClassTemplate);

            owner.Set("AES", AES.Current.ClassTemplate);

            owner.Set("AES256CFB", AES256CFB.Current.ClassTemplate);
            owner.Set("AES256CBC", AES256CBC.Current.ClassTemplate);
            owner.Set("AES256CTS", AES256CTS.Current.ClassTemplate);
            owner.Set("AES256ECB", AES256ECB.Current.ClassTemplate);
            owner.Set("AES256OFB", AES256ECB.Current.ClassTemplate);

            owner.Set("AES192CBC", AES192CBC.Current.ClassTemplate);
            owner.Set("AES192CTS", AES192CTS.Current.ClassTemplate);
            owner.Set("AES192ECB", AES192ECB.Current.ClassTemplate);
            owner.Set("AES192CFB", AES192CFB.Current.ClassTemplate);
            owner.Set("AES192OFB", AES192OFB.Current.ClassTemplate);

            owner.Set("AES128CFB", AES192CBC.Current.ClassTemplate);
            owner.Set("AES128OFB", AES192CTS.Current.ClassTemplate);
            owner.Set("AES128CBC", AES192ECB.Current.ClassTemplate);
            owner.Set("AES128CTS", AES192CFB.Current.ClassTemplate);
            owner.Set("AES128ECB", AES192OFB.Current.ClassTemplate);
        }
    }
}
