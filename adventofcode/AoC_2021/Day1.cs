using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace AdventofCode.AoC_2021
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day1 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(7, 5, "Day1_test.txt")]
        [TestCase(1521, 1543, "Day1.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1 = 0;
            Part2Type part2 = 0;
            var sw = Stopwatch.StartNew();
            var ints = GetIntInput(source);
            LogAndReset("Parse", sw);

            for (int i = 1; i < ints.Length; i++)
                if (ints[i] > ints[i - 1])
                    part1++;

            LogAndReset("*1", sw);

            
            for (int i = 3; i < ints.Length; i++)
                if (ints[i] > ints[i - 3])
                    part2++;

            LogAndReset("*2", sw);
            return (part1, part2);
        }
    }
}