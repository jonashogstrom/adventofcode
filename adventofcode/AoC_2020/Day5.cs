using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2020
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day5 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(357, null, "Day5_test.txt")]
        [TestCase(880, 731, "Day5.txt")]
        [Repeat(5)]
        public void Test1(Part1Type exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            var high = 0L;
            var low = long.MaxValue;
            var allSeats = new bool[2<<source[0].Length];
            foreach (var s in source)
            {
                var seat = 0;
                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] == 'B' || s[i] == 'R')
                        seat += 1 << (s.Length - i-1);
                }

                allSeats[seat] = true;
                high = Math.Max(high, seat);
                low = Math.Min(low, seat);
            }
            var part2 = 0L;
            for (var i= low+1; i<high; i++)
            {
                if (!allSeats[i])
                {
                    part2 = i;
                    break;
                }
            }
            return (high, part2);
        }
    }
}

// var comp = new IntCodeComputer(source[0]);
// comp.Execute();
// var part1 = (int)comp.LastOutput;
// return (part1, 0);
