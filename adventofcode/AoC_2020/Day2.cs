using System;
using NUnit.Framework;

namespace AdventofCode.AoC_2020
{
    using Part1Type = Int32;
    using Part2Type = Int32;

    [TestFixture]
    class Day2 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(-1, null, "Day2_test.txt")]
        [TestCase(-1, null, "Day2.txt")]
        public void Test1(Part1Type exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            // var comp = new IntCodeComputer(source[0]);
            // comp.Execute();
            // var part1 = (int)comp.LastOutput;
            // return (part1, 0);
            return (0, 0);
        }
    }
}