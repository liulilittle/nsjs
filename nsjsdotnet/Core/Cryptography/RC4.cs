namespace nsjsdotnet.Core.Cryptography
{
    using global::System;
    using nsjsdotnet.Core;

    public unsafe class RC4
    {
        private int MaxbitWidth;

        public const int DefaultMaxbitWidth = 256;

        private byte[] VK; // s-box
        private string Key;

        public RC4(string Key, byte[] SBox) : this(Key, SBox, DefaultMaxbitWidth)
        {

        }

        public RC4(string Key, byte[] SBox, int MaxbitWidth)
        {
            if (string.IsNullOrEmpty(Key))
            {
                throw new ArgumentException("key");
            }
            if (MaxbitWidth < 0)
            {
                throw new ArgumentOutOfRangeException("MaxbitWidth");
            }
            this.MaxbitWidth = MaxbitWidth;
            this.Key = Key;
            this.VK = SBox ?? throw new ArgumentNullException("SBox");
        }

        public static byte[] SBox(string key)
        {
            return SBox(key, DefaultMaxbitWidth);
        }

        public static byte[] SBox(string key, int MaxbitWidth)
        {
            byte[] box = new byte[MaxbitWidth];
            for (int i = 0; i < MaxbitWidth; i++)
            {
                box[MaxbitWidth - (i + 1)] = (byte)i;
            }
            for (int i = 0, j = 0; i < MaxbitWidth; i++)
            {
                j = (j + box[i] + key[i % key.Length]) % MaxbitWidth;
                byte b = box[i];
                box[i] = box[j];
                box[j] = b;
            }
            return box;
        }

        private void Inversion(string key, byte[] sbox, byte* num, int len)
        {
            byte[] vk = new byte[sbox.Length];
            BufferExtension.BlockCopy(sbox, vk, vk.Length);
            for (int i = 0, low = 0, high = 0, mid; i < len; i++)
            {
                low = (low + key.Length) % MaxbitWidth;
                high = (high + vk[i % MaxbitWidth]) % MaxbitWidth;

                byte b = vk[low];
                vk[low] = vk[high];
                vk[high] = b;

                mid = (vk[low] + vk[high]) % MaxbitWidth;
                num[i] ^= (byte)vk[mid];
            }
        }

        private byte[] CopyBuffer(byte[] value, int ofs, int len)
        {
            if ((ofs + len) > value.Length)
            {
                len = (value.Length - ofs);
            }
            if (ofs < 0 || ofs >= len)
            {
                len = 0;
            }
            byte[] cch = new byte[len];
            BufferExtension.BlockCopy(value, ofs, cch);
            return cch;
        }

        public virtual byte[] Encrypt(byte[] value, int ofs, int len)
        {
            return Decrypt(value, ofs, len);
        }

        public virtual byte[] Decrypt(byte[] value, int ofs, int len)
        {
            byte[] buffer = CopyBuffer(value, ofs, len);
            fixed (byte* pinned = buffer)
            {
                Inversion(Key, VK, &pinned[ofs], len);
                return buffer;
            }
        }
    }
}
