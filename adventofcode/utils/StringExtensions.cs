using System;
using System.Collections.Generic;

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

        public static SparseBuffer<char> ToSparseBuffer(this string[] input, char def = ' ')
        {
            return ToSparseBuffer<char>(input, def, c => c);
        }

        public static SparseBuffer<T> ToSparseBuffer<T>(this string[] input, T def, Func<char, T> f)
        {
            var floor = new SparseBuffer<T>(def);

            for (int row = 0; row < input.Length; row++)
            {
                for (int col = 0; col < input[row].Length; col++)
                {
                    var c = Coord.FromXY(col, row);
                    floor[c] = f(input[row][col]);
                }
            }

            return floor;
        }
    }
}