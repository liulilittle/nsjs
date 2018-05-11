namespace nsjsdotnet.Runtime.Systematic.Cryptography
{
    using global::System.Security.Cryptography;

    class SHA512 : HashAlgorithm
    {
        public static readonly SHA512 Current = new SHA512();

        protected override global::System.Security.Cryptography.HashAlgorithm New()
        {
            return SHA512CryptoServiceProvider.Create();
        }
    }
}
