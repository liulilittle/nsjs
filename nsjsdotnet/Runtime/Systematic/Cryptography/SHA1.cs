namespace nsjsdotnet.Runtime.Systematic.Cryptography
{
    using global::System.Security.Cryptography;

    class SHA1 : HashAlgorithm
    {
        public static readonly SHA1 Current = new SHA1();

        protected override global::System.Security.Cryptography.HashAlgorithm New()
        {
            return SHA1CryptoServiceProvider.Create();
        }
    }
}
