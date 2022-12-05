using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2022
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day04 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(2, 4, "Day04_test.txt")]
        [TestCase(532, 854, "Day04.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            LogLevel = resourceName.Contains("test") ? 20 : -1;
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1 = 0;
            Part2Type part2 = 0;
            var sw = Stopwatch.StartNew();
            LogAndReset("Parse", sw);
            var count = 0;
            var c2 = 0;
            foreach (var pair in source)
            {
                var data = pair.Split(',', '-').Select(s => int.Parse(s)).ToArray();
                var r1 = new Range(data[0], data[1]);
                var r2 = new Range(data[2], data[3]);
                if (FullyContain(r1, r2) || FullyContain(r2, r1))
                    count++;
                if (!(r1.Start > r2.Stop || r1.Stop < r2.Start))
                    c2++;
            }

            part1 = count;
            part2 = c2;
            LogAndReset("*1", sw);
            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private bool FullyContain(Range r1, Range r2)
        {
            return r1.Start <= r2.Start && r1.Stop >= r2.Stop;
        }
    }
    public class Range
    {
        public int Start { get; }
        public int Stop { get; }

        public Range(int start, int stop)
        {
            Start = start;
            Stop = stop;
        }
    }
}