using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using NUnit.Framework;

namespace AdventofCode.AoC_2020
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day9 : TestBaseClass<Part1Type, Part2Type>
    {
        private int _preamble;
        public bool Debug { get; set; }

        [Test]
        [TestCase(127, 62, "Day9_test.txt", 5)]
        [TestCase(88311122, 13549369, "Day9.txt", 25)]
        [Repeat(5)]
        public void Test1(Part1Type exp1, Part2Type? exp2, string resourceName, int preamble)
        {
            _preamble = preamble;
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            var sw = Stopwatch.StartNew();
            var data = GetLongInput(source);
            LogAndReset("Parse", sw);
            var part1 = FindNonSum(data, _preamble);

            LogAndReset("*1", sw);
            var part2 = FindWeekness(data, part1);

            LogAndReset("*2", sw);
            return (part1, part2);
        }

        private static long FindWeekness(long[] data, long part1)
        {
            int p;
            for (int i = 0; i < data.Length; i++)
            {
                var sum = 0L;
                p = i;
                var min = long.MaxValue;
                var max = long.MinValue;
                while (sum < part1)
                {
                    sum = sum + data[p];
                    min = Math.Min(min, data[p]);
                    max = Math.Max(max, data[p]);
                    p++;
                }

                if (sum == part1)
                {
                    return min + max;
                }
            }

            return -1;
        }

        private static long FindNonSum(long[] data, int preamble)
        {
            var p = preamble;
            while (p < data.Length)
            {
                var start = p - preamble;
                var ok = false;
                for (var i = start; i < p - 1; i++)
                for (var j = i + 1; j < p; j++)
                {
                    if (data[p] == data[i] + data[j])
                        ok = true;
                }

                if (!ok)
                {
                    return data[p];
                }

                p++;
            }

            return -1;
        }
    }
}

