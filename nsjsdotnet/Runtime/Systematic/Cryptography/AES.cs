namespace nsjsdotnet.Runtime.Systematic.Cryptography
{
    using global::System.Security.Cryptography;

    class AES : RijndaelAlgorithm
    {
        public static readonly AES Current = new AES();

        protected override RijndaelCryptoServiceProvider New(byte[] Key, byte[] IV, NSJSFunctionCallbackInfo arguments)
        {
            int[] pushs = new int[5];
            for (int i = 0; i < 5; i++)
            {
                NSJSInt32 int32 = arguments[i] as NSJSInt32;
                if (int32 == null)
                {
                    if (i < 3)
                    {
                        pushs[i] = 128;
                    }
                    else if (i < 4)
                    {
                        pushs[i] = 2; // PKCS7
                    }
                    else if (i < 5)
                    {
                        pushs[i] = 4; // CFB
                    }
                    continue;
                }
                pushs[i] = int32.Value;
            }
            return new RijndaelCryptoServiceProvider(Key, IV, pushs[0], pushs[1], pushs[2], (PaddingMode)pushs[3], (CipherMode)pushs[4]);
        }

        protected override RijndaelCryptoServiceProvider New(byte[] Key, byte[] IV)
        {
            throw new global::System.NotImplementedException();
        }
    }

    class AES256CFB : RijndaelAlgorithm
    {
        public static readonly AES256CFB Current = new AES256CFB();

        protected override RijndaelCryptoServiceProvider New(byte[] Key, byte[] IV)
        {
            return new RijndaelCryptoServiceProvider(Key, IV, 256, 256, 128, PaddingMode.PKCS7, CipherMode.CFB);
        }
    }

    class AES256CBC : RijndaelAlgorithm
    {
        public static readonly AES256CBC Current = new AES256CBC();

        protected override RijndaelCryptoServiceProvider New(byte[] Key, byte[] IV)
        {
            return new RijndaelCryptoServiceProvider(Key, IV, 256, 256, 128, PaddingMode.PKCS7, CipherMode.CBC);
        }
    }

    class AES256CTS : RijndaelAlgorithm
    {
        public static readonly AES256CTS Current = new AES256CTS();

        protected override RijndaelCryptoServiceProvider New(byte[] Key, byte[] IV)
        {
            return new RijndaelCryptoServiceProvider(Key, IV, 256, 256, 128, PaddingMode.PKCS7, CipherMode.CTS);
        }
    }

    class AES256ECB : RijndaelAlgorithm
    {
        public static readonly AES256ECB Current = new AES256ECB();

        protected override RijndaelCryptoServiceProvider New(byte[] Key, byte[] IV)
        {
            return new RijndaelCryptoServiceProvider(Key, IV, 256, 256, 128, PaddingMode.PKCS7, CipherMode.ECB);
        }
    }

    class AES256OFB : RijndaelAlgorithm
    {
        public static readonly AES256OFB Current = new AES256OFB();

        protected override RijndaelCryptoServiceProvider New(byte[] Key, byte[] IV)
        {
            return new RijndaelCryptoServiceProvider(Key, IV, 256, 256, 128, PaddingMode.PKCS7, CipherMode.OFB);
        }
    }

    class AES192CBC : RijndaelAlgorithm
    {
        public static readonly AES192CBC Current = new AES192CBC();

        protected override RijndaelCryptoServiceProvider New(byte[] Key, byte[] IV)
        {
            return new RijndaelCryptoServiceProvider(Key, IV, 256, 192, 128, PaddingMode.PKCS7, CipherMode.CBC);
        }
    }

    class AES192CTS : RijndaelAlgorithm
    {
        public static readonly AES192CTS Current = new AES192CTS();

        protected override RijndaelCryptoServiceProvider New(byte[] Key, byte[] IV)
        {
            return new RijndaelCryptoServiceProvider(Key, IV, 256, 192, 128, PaddingMode.PKCS7, CipherMode.CTS);
        }
    }

    class AES192ECB : RijndaelAlgorithm
    {
        public static readonly AES192ECB Current = new AES192ECB();

        protected override RijndaelCryptoServiceProvider New(byte[] Key, byte[] IV)
        {
            return new RijndaelCryptoServiceProvider(Key, IV, 256, 192, 128, PaddingMode.PKCS7, CipherMode.ECB);
        }
    }

    class AES192CFB : RijndaelAlgorithm
    {
        public static readonly AES192CFB Current = new AES192CFB();

        protected override RijndaelCryptoServiceProvider New(byte[] Key, byte[] IV)
        {
            return new RijndaelCryptoServiceProvider(Key, IV, 256, 192, 128, PaddingMode.PKCS7, CipherMode.CFB);
        }
    }

    class AES192OFB : RijndaelAlgorithm
    {
        public static readonly AES192OFB Current = new AES192OFB();

        protected override RijndaelCryptoServiceProvider New(byte[] Key, byte[] IV)
        {
            return new RijndaelCryptoServiceProvider(Key, IV, 256, 192, 128, PaddingMode.PKCS7, CipherMode.OFB);
        }
    }

    class AES128CFB : RijndaelAlgorithm
    {
        public static readonly AES128CFB Current = new AES128CFB();

        protected override RijndaelCryptoServiceProvider New(byte[] Key, byte[] IV)
        {
            return new RijndaelCryptoServiceProvider(Key, IV, 256, 128, 128, PaddingMode.PKCS7, CipherMode.CFB);
        }
    }

    class AES128OFB : RijndaelAlgorithm
    {
        public static readonly AES128OFB Current = new AES128OFB();

        protected override RijndaelCryptoServiceProvider New(byte[] Key, byte[] IV)
        {
            return new RijndaelCryptoServiceProvider(Key, IV, 256, 128, 128, PaddingMode.PKCS7, CipherMode.OFB);
        }
    }

    class AES128CBC : RijndaelAlgorithm
    {
        public static readonly AES128CBC Current = new AES128CBC();

        protected override RijndaelCryptoServiceProvider New(byte[] Key, byte[] IV)
        {
            return new RijndaelCryptoServiceProvider(Key, IV, 256, 128, 128, PaddingMode.PKCS7, CipherMode.CBC);
        }
    }

    class AES128CTS : RijndaelAlgorithm
    {
        public static readonly AES128CTS Current = new AES128CTS();

        protected override RijndaelCryptoServiceProvider New(byte[] Key, byte[] IV)
        {
            return new RijndaelCryptoServiceProvider(Key, IV, 256, 128, 128, PaddingMode.PKCS7, CipherMode.CTS);
        }
    }

    class AES128ECB : RijndaelAlgorithm
    {
        public static readonly AES128ECB Current = new AES128ECB();

        protected override RijndaelCryptoServiceProvider New(byte[] Key, byte[] IV)
        {
            return new RijndaelCryptoServiceProvider(Key, IV, 256, 128, 128, PaddingMode.PKCS7, CipherMode.ECB);
        }
    }
}
