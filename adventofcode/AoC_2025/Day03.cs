using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2025
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day03 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(357, 3121910778619, "<day>_test.txt")]
        [TestCase(17158, 170449335646486, "<day>.txt")]
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

            LogAndReset("Parse", sw);

            foreach (var bank in source)
            {
                part1 += GetLargest2(bank, 2);

                part2 += GetLargest2(bank, 12);
            }

            LogAndReset("*1", sw);

            // solve part 2 here

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private long GetLargest2(string bank, int len)
        {
            var value = 0L;
            var pos = 0;
            var batts = bank.Select(c => int.Parse(c.ToString())).ToArray();

            var mustKeep = len - 1;
            for (var i = 0; i < len; i++)
            {
                var highestNum = -1;
                var highestPos = -1;
                for (var p = pos; p < bank.Length - mustKeep; p++)
                {
                    if (batts[p] > highestNum)
                    {
                        highestNum = batts[p];
                        highestPos = p;
                    }
                }

                value = value * 10 + highestNum;
                pos = highestPos + 1;
                mustKeep--;
            }

            Assert.That(value.ToString().Length, Is.EqualTo(len));
            Log($"{bank} => {value}");
            return value;
        }

        private long GetLargest2(string bank)
        {
            var max = 0;
            var batts = bank.Select(c => int.Parse(c.ToString())).ToArray();
            for (int i1 = 0; i1 < batts.Length; i1++)
            for (int i2 = i1 + 1; i2 < batts.Length; i2++)
                if (i1 != i2)
                {
                    var val = batts[i1] * 10 + batts[i2];
                    max = Math.Max(max, val);
                }

            Log($"{bank} => {max}");
            return max;
        }
    }
}