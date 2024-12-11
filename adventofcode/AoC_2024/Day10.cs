using System;
using System.Diagnostics;
using NUnit.Framework;
using System.Collections.Generic;
using AdventofCode.Utils;
using System.Linq;


namespace AdventofCode.AoC_2024
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day10 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(36, 81, "Day10_test.txt")]
        [TestCase(1, null, "Day10_test0.txt")]
        [TestCase(2, null, "Day10_test1.txt")]
        [TestCase(4, null, "Day10_test2.txt")]
        [TestCase(3, null, "Day10_test3.txt")]
        [TestCase(1, 3, "Day10_test4.txt")]
        [TestCase(538, null, "Day10.txt")]
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

            var map = source.ToSparseBufferStr(-1, c => c == '.' ? -1 : int.Parse(c.ToString()));
            var trailheads = map.Keys.Where(k => map[k] == 0).ToArray();

            LogAndReset("Parse", sw);

            foreach (var t in trailheads)
            {
                var res = FindPeaks(map, t);
                part1 += res.Item1;
                part2 += res.Item2;
            }

            // solve part 1 here

            LogAndReset("*1", sw);

            // solve part 2 here

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private (long, long) FindPeaks(SparseBufferStr<int> map, CoordStr start)
        {
            var i = 0;
            var positions = new DicWithDefault<CoordStr, int> { [start] = 1 };
            while (i < 9)
            {
                var newPositions = new DicWithDefault<CoordStr, int>();
                foreach (var p in positions.Keys)
                foreach (var n in p.GenAdjacent4())
                    if (map.InsideBounds(n) && map[n] == i + 1)
                        newPositions[n] += positions[p];
                positions = newPositions;
                i++;
            }

            return (positions.Keys.Count(), positions.Values.Sum());
        }
    }
}