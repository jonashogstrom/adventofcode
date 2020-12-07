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
    }
}