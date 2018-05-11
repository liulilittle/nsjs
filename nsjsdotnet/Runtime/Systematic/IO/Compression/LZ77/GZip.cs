namespace nsjsdotnet.Runtime.Systematic.IO.Compression.LZ77
{
    using nsjsdotnet.Core.IO.Compression;

    class GZip : LZ77Algorithm
    {
        public static readonly GZip Current = new GZip();

        private GZip() : base(LZ77Auxiliary.LZ77Algorithm.GZip)
        {

        }
    }
}
