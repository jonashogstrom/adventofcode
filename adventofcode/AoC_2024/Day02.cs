using System;
using System.Diagnostics;
using NUnit.Framework;
using System.Collections.Generic;
using AdventofCode.Utils;
using System.Linq;

// 494 too low
// 511 not right

namespace AdventofCode.AoC_2024
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day02 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(2, 4, "Day02_test.txt")]
        [TestCase(463, 514, "Day02.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            LogLevel = resourceName.Contains("test") ? 20 : -1;
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            var sw = Stopwatch.StartNew();
            LogAndReset("Parse", sw);
            long part1 = 0;
            long part2 = 0;
            foreach (var report in source)
            {
                var levels = report.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
                if (IsSafeReport(levels))
                {
                    part1++;
                }
            }
            LogAndReset("*1", sw);

            foreach (var report in source)
            {
                var levels = report.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();

                var permuts = GetPermuts(levels);
                if (permuts.Any(IsSafeReport))
                    part2++;
            }
            LogAndReset("*2", sw);
            return (part1, part2);
        }

        private static IEnumerable<int[]> GetPermuts(int[] levels)
        {
            for (var i = 0; i < levels.Length; ++i)
            {
                yield return levels.Where((t, j) => j != i).ToArray();
            }
        }

        private bool IsSafeReport(int[] levels)
        {
            var sign = GetSign(levels[0], levels[1]);
            foreach (var p in levels.AsPairs())
            {
                var diff = Math.Abs(p.Item1 - p.Item2);
                if (diff is < 1 or > 3)
                    return false;
                if (GetSign(p.Item1, p.Item2) != sign) 
                    return false;
            }

            return true;
        }
        
        int GetSign(int v1, int v2)
        {
            return v1 - v2 > 0 ? -1 : 1;
        }
    }
}