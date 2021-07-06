using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static System.String;

namespace Bumbershoot.Utilities.Helpers
{
    public static class EnumerableHelper
    {
        public static string StringJoin(this IEnumerable<object> values, string separator = ", ")
        {
            if (values == null) return null;
            var array = values.Select(x => x.ToString()).ToArray();
            return Join(separator, array);
        }


        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> values, Action<T> call)
        {
            if (values == null) return null;
            foreach (var value in values) call(value);
            return values;
        }

        public static object LookupValidValue<T>(this IEnumerable<T> values, string call)
        {
            if (call == null) throw new ArgumentNullException(nameof(call));
            var valueTuples = values.Select(x => new Tuple<T, string>(x, x.ToString())).ToArray();
            var firstOrDefault = valueTuples.FirstOrDefault(x =>
                String.Equals(x.Item2, call, StringComparison.CurrentCultureIgnoreCase));
            if (firstOrDefault != null) return firstOrDefault.Item1;
            return null;
        }

        public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this Task<List<T>> task)
        {
            var systemEvents = await task;
            foreach (var systemEvent in systemEvents)
            {
                yield return systemEvent;
            }
        }

        public static async IAsyncEnumerable<T2> OfType<T, T2>(this IAsyncEnumerable<T> iterator)
        {
            await foreach (var value in iterator)
            {
                if (value is T2 typed) yield return typed;
            }
        }

        public static async Task<List<T>> ToList<T>(this IAsyncEnumerable<T> iterator, CancellationToken cancellationToken)
        {
            List<T> list = new List<T>();
            await foreach (var value in iterator.WithCancellation(cancellationToken))
            {
                list.Add(value);
            }
            return list;
        }

        public static async IAsyncEnumerable<T2> Select<T, T2>(this IAsyncEnumerable<T> iterator, Func<T, T2> map)
        {
            await foreach (var value in iterator)
            {
                yield return map(value);
            }
        }
    }


    public static class LevenshteinDistance
    {
        /// <summary>
        /// Compute the distance between two strings.
        /// </summary>
        public static int Compute(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }
    }

}