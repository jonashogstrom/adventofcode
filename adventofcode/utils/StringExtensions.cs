using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventofCode.Utils
{
    public static class StringExtensions
    {
        public static IEnumerable<IList<string>> AsGroups(this IEnumerable<string> strings)
        {
            var group = new List<string>();
            foreach (var s in strings)
            {
                if (string.IsNullOrEmpty(s))
                {
                    yield return group;
                    group = new List<string>();
                }
                else
                {
                    group.Add(s);
                }
            }

            yield return group;
        }

        public static SparseBuffer<char> ToSparseBuffer(this ICollection<string> input, char def = ' ')
        {
            return ToSparseBuffer(input, def, c => c);
        }

        public static SparseBuffer<T> ToSparseBuffer<T>(this ICollection<string> input, T def, Func<char, T> f)
        {
            var floor = new SparseBuffer<T>(def);

            for (int row = 0; row < input.Count; row++)
            {
                var r = input.ElementAt(row);
                for (int col = 0; col < r.Length; col++)
                {
                    var c = Coord.FromXY(col, row);
                    floor[c] = f(r[col]);
                }
            }

            return floor;
        }

        public static long ParseBin(this string s)
        {
            return Convert.ToInt64(s, 2);
        }
    }
}