using System;
using System.Collections.Generic;
using System.Linq;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode
{
    public static class EnumerableExt
    {
        public static DicWithDefault<T, int> CountValues<T>(this IEnumerable<T> s)
        {
            var res = new DicWithDefault<T, int>();
            foreach (var x in s)
                res[x]++;
            return res;
        }

        /// <summary>
        /// Finds the lengths of the sequences of items identical to the key parameter.
        /// </summary>
        /// <example>
        /// 1 1 1 3 3 4 1 1 2 1 4 1 1 1 4
        /// ^ ^ ^       ^ ^   ^   ^ ^ ^
        ///   3          2    1     3
        /// </example>
        public static IEnumerable<int> FindSequenceLengthsForKey<T>(this IEnumerable<T> s, T key) where T : IEquatable<T>
        {
            return s.FindSequenceLengths()
                .Where(seq1 => seq1.key.Equals(key))
                .Select(seq2 => seq2.length);
        }

        public static long Multiply(this IEnumerable<long> s)
        {
            return s.Aggregate(1L, (i1, i2) => i1 * i2);
        }

        /// <summary>
        /// Multiplies all elements in a sequence
        /// </summary>
        public static long Multiply(this IEnumerable<int> s)
        {
            return s.Aggregate(1L, (i1, i2) => i1 * i2);
        }

        /// <summary>
        /// Finds sequences of identical elements and counts the lengths of these sequences
        /// </summary>
        public static IEnumerable<(T key, int length)> FindSequenceLengths<T>(this IEnumerable<T> s) where T : IEquatable<T>
        {
            T last = default;
            var count = 0;
            foreach (var t in s)
            {
                if (!t.Equals(last))
                {
                    if (count > 0)
                        yield return (last, count);
                    count = 0;
                }

                last = t;
                count++;
            }

            yield return (last, count);

        }

        /// <summary>
        /// if the input is [a, b, c, d] the result will be [(a,b), (b,c), (c,d)]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s"></param>
        /// <returns></returns>
        public static IEnumerable<(T, T)> AsPairs<T>(this IEnumerable<T> s)
        {
            var prev = default(T);
            var first = true;
            foreach (var x in s)
            {
                if (!first)
                    yield return (prev, x);
                first = false;
                prev = x;
            }
        }

        public static IEnumerable<(T, T)> AsCombinations<T>(this IEnumerable<T> s)
        {
            var count = 0;
            foreach (var x in s)
            {
                count++;
                foreach (var y in s.Skip(count))
                    yield return (x, y);
            }
        }

        public static IEnumerable<(T, T)> AsCombinations<T>(this List<T> s)
        {
            for (int i = 0; i < s.Count; i++)
                for (int j = i + 1; j < s.Count; j++)
                    yield return (s[i], s[j]);
        }

        public static IEnumerable<int> groupSizes<T>(this IEnumerable<T> s) where T : IEquatable<T>
        {
            T last = default;
            var count = 0;
            foreach (var t in s)
            {
                if (!t.Equals(last))
                {
                    if (count > 0)
                        yield return count;
                    count = 0;
                }

                last = t;
                count++;
            }

            yield return count;
        }
    }


    /// <summary>
    /// Caches recursive computations. The recursive func needs to take a cache as its second parameter.
    /// </summary>
    /// <typeparam name="TRes"></typeparam>
    /// <typeparam name="TParam1"></typeparam>

    public class Cache<TRes, TParam1>
    {
        private readonly Dictionary<(Func<TParam1, Cache<TRes, TParam1>, TRes>, TParam1), TRes> _cache = new();
        public TRes Do(Func<TParam1, Cache<TRes, TParam1>, TRes> f, TParam1 param1)
        {
            if (_cache.TryGetValue((f, param1), out var res))
                return res;
            res = f(param1, this);
            _cache[(f, param1)] = res;
            return res;
        }
    }
    public class Cache2<TRes, TParam1>
    {
        private readonly Func<TParam1, Cache2<TRes, TParam1>, TRes> _f;

        public Cache2(Func<TParam1, Cache2<TRes, TParam1>, TRes> f)
        {
            _f = f;
        }

        private readonly Dictionary<TParam1, TRes> _cache = new();
        public TRes Do(TParam1 param1)
        {
            if (_cache.TryGetValue(param1, out var res))
                return res;
            res = _f(param1, this);
            _cache[param1] = res;
            return res;
        }
    }

    public class Cache22<TRes, TParam1, TParam2>
    {
        private readonly Func<TParam1, TParam2, Cache22<TRes, TParam1, TParam2>, TRes> _f;

        public Cache22(Func<TParam1, TParam2, Cache22<TRes, TParam1, TParam2>, TRes> f)
        {
            _f = f;
        }

        private readonly Dictionary<(TParam1, TParam2), TRes> _cache = new();
        public TRes Do(TParam1 param1, TParam2 param2)
        {
            if (_cache.TryGetValue((param1, param2), out var res))
                return res;
            res = _f(param1, param2, this);
            _cache[(param1, param2)] = res;
            return res;
        }
    }



}