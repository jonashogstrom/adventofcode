using System;
using System.Collections.Generic;

namespace AdventofCode
{
    public static class EnumerableExt
    {
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
}