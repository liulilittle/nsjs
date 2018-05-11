namespace nsjsdotnet.Runtime.Systematic.Cryptography
{
    using global::System.Security.Cryptography;

    class MD5 : HashAlgorithm
    {
        public static readonly MD5 Current = new MD5();

        protected override global::System.Security.Cryptography.HashAlgorithm New()
        {
            return MD5CryptoServiceProvider.Create();
        }
    }
}
