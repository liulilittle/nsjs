namespace nsjsdotnet.Runtime.Systematic.Cryptography
{
    using global::System;
    using global::System.Security.Cryptography;

    class RijndaelCryptoServiceProvider : IDisposable
    {
        private RijndaelManaged AES;
        private byte[] IV;
        private byte[] Key;
        private bool disposed;

        public RijndaelCryptoServiceProvider(byte[] Key, byte[] IV, int BlockSize, int KeySize,
            int FeedbackSize, PaddingMode PaddingMode, CipherMode CipherMode)
        {
            this.AES = new RijndaelManaged();
            this.AES.BlockSize = BlockSize;
            this.AES.KeySize = KeySize;
            this.AES.FeedbackSize = FeedbackSize;
            this.AES.Padding = PaddingMode;
            this.AES.Mode = CipherMode;
            this.IV = IV;
            this.Key = Key;
            this.AES.IV = this.IV;
        }

        public void Dispose()
        {
            lock (this)
            {
                if (!this.disposed)
                {
                    this.disposed = true;
                    try
                    {
                        this.AES.Dispose();
                    }
                    catch(Exception) { }
                }
            }
        }

        public byte[] Encrypt(byte[] buffer, int ofs, int len)
        {
            using (ICryptoTransform encryptor = AES.CreateEncryptor(this.Key, this.IV))
            {
                return encryptor.TransformFinalBlock(buffer, ofs, len);
            }
        }

        public byte[] Decrypt(byte[] buffer, int ofs, int len)
        {
            using (ICryptoTransform decryptor = AES.CreateDecryptor(this.Key, this.IV))
            {
                return decryptor.TransformFinalBlock(buffer, ofs, len);
            }
        }
    }
}
