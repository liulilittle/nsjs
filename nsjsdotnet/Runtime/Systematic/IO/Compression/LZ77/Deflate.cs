namespace nsjsdotnet.Runtime.Systematic.IO.Compression.LZ77
{
    using nsjsdotnet.Core.IO.Compression;

    class Deflate : LZ77Algorithm
    {
        public static readonly Deflate Current = new Deflate();

        private Deflate() : base(LZ77Auxiliary.LZ77Algorithm.Deflate)
        {

        }
    }
}
