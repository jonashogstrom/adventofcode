using System;
using System.Diagnostics;
using NUnit.Framework;
using System.Collections.Generic;
using AdventofCode.Utils;
using System.Linq;
using System.Text.RegularExpressions;


namespace AdventofCode.AoC_2024
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    // 111972528 too high
    [TestFixture]
    partial class Day03 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(161, null, "Day03_test.txt")]
        [TestCase(161, 48, "Day03_test2.txt")]
        [TestCase(174561379, 106921067, "Day03.txt")]
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

            // parse input here

            LogAndReset("Parse", sw);

            var r = MyRegex();

            foreach (var line in source)
            {
                var m = r.Matches(line);
                foreach (Match match in m)
                {
                    part1 += int.Parse(match.Groups[1].Value) * int.Parse(match.Groups[2].Value);
                }
            }

            LogAndReset("*1", sw);

            var r2 = MyRegex2();
            var sum = true;
            foreach (var line in source)
            {
                var m = r2.Matches(line);
                foreach (Match match in m)
                {
                    if (match.Groups[0].Value == "do()")
                        sum = true;
                    else if (match.Groups[0].Value == "don't()")
                        sum = false;
                    else if (sum)
                        part2 += int.Parse(match.Groups[2].Value) * int.Parse(match.Groups[3].Value);
                }
            }

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        [GeneratedRegex(@"mul\((\d+),(\d+)\)")]
        private static partial Regex MyRegex();

        [GeneratedRegex(@"(mul\((\d+),(\d+)\)|do\(\)|don't\(\))")]
        private static partial Regex MyRegex2();
    }
}