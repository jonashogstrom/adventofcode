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
        [TestCase(7716, 2973, "Day03.txt")]
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
            var sum = 0;
            foreach (var r in source)
            {
                var p1 = r.Substring(0, r.Length / 2);
                var p2 = r.Substring(r.Length / 2);
                var dupes = FindDupes(p1, p2);
                var dupe = dupes.Single();
                var value = GetVal(dupe);
                Log($"{r} => {dupe} => {value}");
                sum += value;

            }

            part1 = sum;
            LogAndReset("*1", sw);

            sum = 0;
            IEnumerable<char> temp = null;
            for (var i=0; i<source.Length; i++)
            {
                if (temp == null)
                    temp = source[i];
                else
                {
                    temp = FindDupes(temp, source[i]);
                }
                if (i%3==2)
                {
                    var badge = temp.Single();
                    var value = GetVal(badge);
                    Log($"group => {badge} => {value}");
                    sum += value;
                    temp = null;
                }
            }

            part2 = sum;
            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private IEnumerable<char> FindDupes(IEnumerable<char> s1, IEnumerable<char> s2)
        {
            return s1.Intersect(s2);
        }

        private int GetVal(char dupe)
        {
            if (dupe <= 'Z')
                return (dupe - 'A') + 27;
            return (dupe - 'a') + 1;
        }
    }
}