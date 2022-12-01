using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2022
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day01 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(24000, 45000, "Day01_test.txt")]
        [TestCase(66616, 199172, "Day01.txt")]
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

            var calories = new List<int>();
            var groups = source.AsGroups();
            foreach (var g in groups)
            {
                calories.Add(g.AsInt().Sum());
            }

            var orderedCalories = calories.OrderByDescending(c=>c);
            part1 = orderedCalories.First();

            LogAndReset("*1", sw);
            part2 = orderedCalories.Take(3).Sum();


            LogAndReset("*2", sw);

            return (part1, part2);
        }
    }
}