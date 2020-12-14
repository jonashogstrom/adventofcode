using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using NUnit.Framework;

namespace AdventofCode.AoC_2020
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day13 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(295, 1068781, "Day13_test.txt")]
        [TestCase(2845, null, "Day13.txt")]
        // [TestCase(null, 3, "3")]
        [Repeat(5)]

        [TestCase(null, 9, "3,5")]
        [TestCase(null, 3, "3,x,5")]
        [TestCase(null, 12, "3,x,x,5")]

        [TestCase(null, 54, "3,5,7")]

        [TestCase(null, 3417, "17,x,13,19")]
        [TestCase(null, 754018, "67,7,59,61")]
        [TestCase(null, 779210, "67,x,7,59,61")]
        [TestCase(null, 1261476, "67,7,x,59,61")]
        [TestCase(null, 1202161486, "1789,37,47,1889")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            var part1 = 0;
            var part2 = 0L;

            var sw = Stopwatch.StartNew();

            var time = source.Length == 2 ? int.Parse(source[0]) : -1;
            var buses = source.Last().Split(',').Select(s => s == "x" ? -1 : int.Parse(s)).ToList();

            LogAndReset("Parse", sw);


            var first = int.MaxValue;
            var selectedbus = -1;
            foreach (var bus in buses.Where(b => b != -1))
            {
                var x = time / bus;
                var next = (x + 1) * bus;
                if (next < first)
                {
                    first = next;
                    selectedbus = bus;
                }
            }

            part1 = selectedbus * (first - time);
            LogAndReset("*1", sw);

            var busNumbers = new System.Collections.Generic.List<int>();
            var busOffsets = new System.Collections.Generic.List<int>();
            for (int i = 0; i < buses.Count; i++)
            {
                if (buses[i] != -1)
                {
                    busNumbers.Add(buses[i]);
                    busOffsets.Add(i);
                }
            }

            var factors = new System.Collections.Generic.List<long>();

            for (var i = 0; i < busNumbers.Count - 1; i++)
            {
                var init = 0L;
                var xx = 1L;
                for (var x = 0; x < factors.Count; x++)
                {
                    xx *= busNumbers[x];
                    init += factors[x] * xx;
                }

                xx *= busNumbers[i];
                var t = 1L;
                while ((init + t * xx + busOffsets[i + 1]) % busNumbers[i + 1] != 0)
                {
                    t += 1;
                }
                part2 = init + t * xx;
                factors.Add(t);
            }

            LogAndReset("*2", sw);

            return (part1, part2);
        }
    }
}
