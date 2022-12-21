using System;
using NUnit.Framework;

namespace AdventofCode.AoC_2020
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day3 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(7, 336, "Day3_test.txt")]
        [TestCase(200, 3737923200L, "Day3.txt")]
        public void Test1(Part1Type exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Log("Part1");
            var part1 = CheckSlope(source, 3, 1);
            Log("Part2");
            var part2 =
                CheckSlope(source, 1, 1) *
                CheckSlope(source, 3, 1) *
                CheckSlope(source, 5, 1) *
                CheckSlope(source, 7, 1) *
                CheckSlope(source, 1, 2);
            return (part1, part2);
        }

        private long CheckSlope(string[] source, int right, int down)
        {
            var width = source[0].Length;
            var pos = 0;
            var trees = 0;
            for (int row=0; row < source.Length; row += down)
            {
                if (source[row][pos % width] == '#')
                    trees++;
                pos += right;
            }
            Log($"Right: {right} Down: {down} Trees: {trees}");

            return trees;
        }
    }

    // var comp = new IntCodeComputer(source[0]);
    // comp.Execute();
    // var part1 = (int)comp.LastOutput;
    // return (part1, 0);

}