namespace nsjsdotnet.Core.Utilits
{
    using System;

    public static class Random
    {
        private static System.Random random = new System.Random();
        private static readonly object syncobj = new object();

        public static int NextInt32Value(int min, int max)
        {
            lock (syncobj)
            {
                int seed = random.Next(min, max);
                random = new System.Random(seed);
                return seed;
            }
        }

        public static double NextDoubleValue(int min, int max)
        {
            lock (syncobj)
            {
                double seed = random.NextDouble();
                random = new System.Random(Convert.ToInt32(Math.Round(seed)));
                return seed;
            }
        }

        public static int NextInt32Value()
        {
            lock (syncobj)
            {
                int seed = random.Next();
                random = new System.Random(seed);
                return seed;
            }
        }
    }
}
