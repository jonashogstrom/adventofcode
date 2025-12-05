using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2025
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    // wrong: 156678203344395
    // wrong: 156678203344390
    
    [TestFixture]
    class Day05 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(3, 14, "<day>_test.txt")]
        [TestCase(840, null, "<day>.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            LogLevel = resourceName.Contains("test") ? 20 : -1;
            var source = GetResource2(ref resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1 = 0;
            Part2Type part2 = 0;
            var sw = Stopwatch.StartNew();

            // parse input here
            var groups = source.AsGroups();
            var ranges = groups.First().Select(s => s.Split('-').Select(long.Parse).ToArray()).ToList();
            var ingredients = groups.ElementAt(1).Select(long.Parse).ToArray();

            LogAndReset("Parse", sw);

            foreach(var i in ingredients)
                if (IsFresh(ranges, i))
                    part1++;
            // solve part 1 here

            LogAndReset("*1", sw);

            var orderedRanges = ranges.OrderBy(r => r[0]).ToList();
            var currentRange = orderedRanges.First();
            LogLevel = 20;
            foreach (var r in orderedRanges.Skip(1))
            {
                if (r[0] <= currentRange[1])
                {
                    var newEnd = Math.Max(r[1], currentRange[1]);
                    Log(()=>$"Merging {f(currentRange)} and {f(r)} => {currentRange[0]}..{newEnd}");
                    currentRange[1] = newEnd;
                }
                else
                {
                    Log(()=>$"Consuming {f(currentRange)}", 0);
                    part2 += currentRange[1] - currentRange[0]+1;
                    currentRange = r;
                }
            }
            part2 += currentRange[1] - currentRange[0]+1;
            // solve part 2 here

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private string f(long[] currentRange)
        {
            return $"{currentRange[0]}..{currentRange[1]}";
        }

        private bool IsFresh(List<long[]> ranges, long l)
        {
            foreach(var r in ranges)
                if (l >= r[0] && l <= r[1])
                    return true;
            return false;
        }
    }
}
