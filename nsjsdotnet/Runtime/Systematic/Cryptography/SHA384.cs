namespace nsjsdotnet.Runtime.Systematic.Cryptography
{
    using global::System.Security.Cryptography;

    class SHA384 : HashAlgorithm
    {
        public static readonly SHA384 Current = new SHA384();

        protected override global::System.Security.Cryptography.HashAlgorithm New()
        {
            return SHA384CryptoServiceProvider.Create();
        }
    }
}
