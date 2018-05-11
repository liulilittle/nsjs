namespace nsjsdotnet.Core.Linq
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public static class Enumerable
    {
        public static bool IsNullOrEmpty(this IEnumerable enumerable)
        {
            if (enumerable == null)
                return true;
            return IsNullOrEmpty(enumerable.GetEnumerator());
        }

        public static bool IsNullOrEmpty(this IEnumerator enumerator)
        {
            if (enumerator == null)
                return true;
            return !enumerator.MoveNext();
        }

        public static IEnumerable<T> EmptyArray<T>()
        {
            return new HashSet<T>();
        }

        public static bool Any(this IEnumerator enumerator)
        {
            return !IsNullOrEmpty(enumerator);
        }

        public static bool Any(this IEnumerable enumerable)
        {
            if (enumerable == null)
                return false;
            return Any(enumerable.GetEnumerator());
        }

        public static T FirstOfDefault<T>(this IEnumerator<T> enumerator, Func<T, bool> equals)
        {
            if (enumerator == null)
            {
                return default(T);
            }
            if (equals == null)
            {
                if (!enumerator.MoveNext())
                {
                    return default(T);
                }
                return enumerator.Current;
            }
            else
            {
                while (enumerator.MoveNext())
                {
                    if (equals(enumerator.Current))
                    {
                        return enumerator.Current;
                    }
                }
            }
            return default(T);
        }

        public static T FirstOrDefault<T>(this IEnumerable<T> enumerable, Func<T, bool> equals)
        {
            if (enumerable == null)
            {
                return default(T);
            }
            return FirstOfDefault(enumerable.GetEnumerator(), equals);
        }

        public static T FirstOrDefault<T>(this IEnumerable<T> enumerable)
        {
            return FirstOrDefault(enumerable, null);
        }

        public static T FirstOrDefault<T>(this IEnumerator<T> enumerator)
        {
            return FirstOfDefault(enumerator, null);
        }

        public static IEnumerable<T> Where<T>(this IEnumerator<T> enumerator, Func<T, bool> predicate)
        {
            IList<T> s = new List<T>();
            if (enumerator != null)
            {
                while (enumerator.MoveNext())
                {
                    if (predicate == null || predicate(enumerator.Current))
                    {
                        s.Add(enumerator.Current);
                    }
                }
            }
            return s;
        }

        public static IEnumerable<T> Where<T>(this IEnumerator<T> enumerator)
        {
            return Where<T>(enumerator, null);
        }

        public static IEnumerable<T> Where<T>(this IEnumerable<T> enumerable)
        {
            return Where(enumerable, null);
        }

        public static IEnumerable<T> Where<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            if (enumerable == null)
                return new List<T>();
            return Where(enumerable.GetEnumerator(), predicate);
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerator<TSource> enumerator, Func<TSource, TResult> selector)
        {
            IList<TResult> results = new List<TResult>();
            if (enumerator == null)
            {
                return results;
            }
            while (enumerator.MoveNext())
            {
                if (selector == null)
                    results.Add(default(TResult));
                else
                    results.Add(selector(enumerator.Current));
            }
            return results;
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> enumerable, Func<TSource, TResult> selector)
        {
            if (enumerable == null)
            {
                return new List<TResult>();
            }
            return Select<TSource, TResult>(enumerable.GetEnumerator(), selector);
        }

        public static List<T> ToList<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
                return new List<T>();
            return new List<T>(enumerable);
        }

        public static List<T> ToList<T>(this IEnumerator<T> enumerator)
        {
            List<T> s = new List<T>();
            if (enumerator != null)
            {
                while (enumerator.MoveNext())
                {
                    s.Add(enumerator.Current);
                }
            }
            return s;
        }

        public static T[] ToArray<T>(this IEnumerable<T> enumerable)
        {
            return ToList<T>(enumerable).ToArray();
        }

        public static T[] ToArray<T>(this IEnumerator<T> enumerator)
        {
            return ToList(enumerator).ToArray();
        }

        public static int Count<T>(this IEnumerator<T> enumerator)
        {
            if (enumerator == null)
                return 0;
            int count = 0;
            while (enumerator.MoveNext())
                count++;
            return count;
        }

        public static int Count<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
                return 0;
            ICollection<T> collection = enumerable as ICollection<T>;
            if (collection != null)
                return collection.Count;
            return Count(enumerable.GetEnumerator());
        }

        public static IEnumerable<T> Reverse<T>(this IEnumerator<T> enumerator)
        {
            List<T> s = new List<T>();
            if (enumerator == null)
                return s;
            while (enumerator.MoveNext())
            {
                T current = enumerator.Current;
                s.Insert(0, current);
            }
            return s;
        }

        public static IEnumerable<T> Reverse<T>(this IEnumerable<T> enumerable)
        {
            return Reverse(enumerable);
        }

        public static bool Contains<T>(this IEnumerator<T> enumerator, Func<T, bool> equals)
        {
            if (enumerator == null || equals == null)
            {
                return false;
            }
            while (enumerator.MoveNext())
            {
                if (equals(enumerator.Current))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool Contains<T>(this IEnumerable<T> enumerable, Func<T, bool> equals)
        {
            if (enumerable == null)
            {
                return false;
            }
            return Contains(enumerable.GetEnumerator(), equals);
        }
    }
}