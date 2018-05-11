namespace nsjsdotnet.Core.Threading.Coroutines
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public static class Parallel
    {
        public static object ForEach<T>(IList<T> source, Action<T> body)
        {
            if (source == null || body == null)
            {
                throw new ArgumentNullException();
            }
            int len = source.Count;
            if (len > 0)
            {
                return Parallel.For(0, len, (index) =>
                {
                    body(source[index]);
                });
            }
            return Task.NULL;
        }

        public static object For(int fromInclusive, int toExclusive, Action<int> body)
        {
            if (body == null)
            {
                throw new ArgumentNullException();
            }
            if (fromInclusive >= toExclusive)
            {
                throw new IndexOutOfRangeException();
            }
            Task task = Task.Factory.StartNew(Parallel.For(body, fromInclusive, toExclusive));
            return task.Wait();
        }

        private static IEnumerator For(Action<int> body, int fromInclusive, int toExclusive)
        {
            int count = fromInclusive;
            for (; fromInclusive < toExclusive; fromInclusive++)
            {
                Task task = Task.Factory.StartNew(Parallel.Invoke(body, fromInclusive));
                task.WaitAsync((token, state) => count++);
            }
            while (count < toExclusive)
            {
                yield return Task.Sleep(1);
            }
        }

        private static IEnumerator Invoke<T>(Action<T> body, T value)
        {
            body(value);
            yield return Task.NULL;
        }
    }
}
