using System.Collections.Generic;

namespace AdventofCode.AoC_2020
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
            var floor = new SparseBuffer<char>(def);

            for (int row = 0; row < input.Length; row++)
            {
                for (int col = 0; col < input[row].Length; col++)
                {
                    var c = Coord.FromXY(col, row);
                    floor[c] = input[row][col];
                }
            }

            return floor;

        }
    }
}