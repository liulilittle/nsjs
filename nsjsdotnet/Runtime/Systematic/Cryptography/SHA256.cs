namespace nsjsdotnet.Runtime.Systematic.Cryptography
{
    using global::System.Security.Cryptography;

    class SHA256 : HashAlgorithm
    {
        public static readonly SHA256 Current = new SHA256();

        protected override global::System.Security.Cryptography.HashAlgorithm New()
        {
            return SHA256CryptoServiceProvider.Create();
        }
    }
}
