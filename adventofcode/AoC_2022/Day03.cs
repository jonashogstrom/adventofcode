using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2022
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day03 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(157, 70, "Day03_test.txt")]
        [TestCase(7716, null, "Day03.txt")]
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
            var dupes = "";
            var sum = 0;
            foreach (var r in source)
            {
                var dupe = ' ';
                for (int i = 0; i < r.Length / 2; i++)
                {
                    var item = r[i];
                    for (int j = r.Length / 2; j < r.Length; j++)
                    {
                        if (item == r[j])
                        {
                            dupe = item;
                            break;
                        }
                    }
                }
                Assert.True(dupe != ' ');
                var value = GetVal(dupe);
                Log($"{r} => {dupe} => {value}");
                sum += value;

            }

            part1 = sum;
            LogAndReset("*1", sw);

            var group = new List<string>();
            sum = 0;
            foreach (var r in source)
            {
                group.Add(r);
                if (group.Count == 3)
                {
                    var badge = SolvePart2(group);
                    var value = GetVal(badge);
                    Log($"group => {badge} => {value}");
                    sum += value;
                    group.Clear();
                }
            }

            part2 = sum;
            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private char SolvePart2(List<string> group)
        {
            var dupes = group[0].ToHashSet().Intersect(group[1].ToHashSet()).Intersect(group[2].ToHashSet());
            return dupes.Single();
        }

        private int GetVal(char dupe)
        {
            if (dupe <= 'Z')
                return (dupe - 'A') + 27;
            return (dupe - 'a') + 1;
        }
    }
}