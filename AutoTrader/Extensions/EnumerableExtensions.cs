using System;
using System.Collections.Generic;

namespace AutoTrader.Extensions
{
    public static class EnumerableExtensions
    {
        private static TSource ExtremaBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TKey, TKey, int> comparer)
        {
            using (var e = source.GetEnumerator())
            {
                if (!e.MoveNext()) throw new InvalidOperationException("sequence contains no elements");

                var current = e.Current;
                var currentKey = keySelector(current);
                while (e.MoveNext())
                {
                    var next = e.Current;
                    var nextKey = keySelector(next);
                    if (comparer(currentKey, nextKey) < 0)
                    {
                        current = next;
                        currentKey = nextKey;
                    }
                }

                return current;
            }
        }

        private static List<TSource> ExtremaByAll<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, Func<TKey, TKey, int> comparer)
        {
            using (var e = source.GetEnumerator())
            {
                if (!e.MoveNext()) throw new InvalidOperationException("sequence contains no elements");

                var list = new List<TSource>();

                var current = e.Current;
                var currentKey = keySelector(current);
                list.Add(current);
                while (e.MoveNext())
                {
                    var next = e.Current;
                    var nextKey = keySelector(next);
                    var compare = comparer(currentKey, nextKey);
                    if (compare == 0)
                    {
                        list.Add(next);
                    }
                    else if (compare < 0)
                    {
                        list.Clear();
                        list.Add(next);
                        currentKey = nextKey;
                    }
                }

                return list;
            }
        }


        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

            return MaxBy(source, keySelector, Comparer<TKey>.Default);
        }

        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector,
            IComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));

            return ExtremaBy(source, keySelector, comparer.Compare);
        }

        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector,
            Func<TKey, TKey, int> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));

            return ExtremaBy(source, keySelector, comparer);
        }

        public static List<TSource> MaxByAll<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

            return MaxByAll(source, keySelector, Comparer<TKey>.Default);
        }

        public static List<TSource> MaxByAll<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));

            return ExtremaByAll(source, keySelector, comparer.Compare);
        }

        public static List<TSource> MaxByAll<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, Func<TKey, TKey, int> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));

            return ExtremaByAll(source, keySelector, comparer);
        }

        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

            return MinBy(source, keySelector, Comparer<TKey>.Default);
        }

        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector,
            IComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));

            return ExtremaBy(source, keySelector, (x, y) => -comparer.Compare(x, y));
        }

        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector,
            Func<TKey, TKey, int> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));

            return ExtremaBy(source, keySelector, (x, y) => -comparer(x, y));
        }

        public static List<TSource> MinByAll<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

            return MinByAll(source, keySelector, Comparer<TKey>.Default);
        }

        public static List<TSource> MinByAll<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));

            return ExtremaByAll(source, keySelector, (x, y) => -comparer.Compare(x, y));
        }

        public static List<TSource> MinByAll<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, Func<TKey, TKey, int> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));

            return ExtremaByAll(source, keySelector, (x, y) => -comparer(x, y));
        }
    }
}