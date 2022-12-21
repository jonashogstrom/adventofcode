using System;
using System.Diagnostics;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2022
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day08 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(21, 8, "Day08_test.txt")]
        [TestCase(1690, 535680, "Day08.txt")]
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
            var sw = Stopwatch.StartNew();
            var grid = source.ToSparseBuffer(-1, c => int.Parse(c.ToString()));
            LogAndReset("Parse", sw);
            var visibleCount = 0;
            var maxScenicScore = -1;
            foreach (var coord in grid.Keys)
            {
                var coveredCount = 0;
                var scenicScore = 1;
                foreach (var dir in Coord.NSWE)
                {
                    var t = coord.Move(dir);
                    var viewingDistance = 0;
                    while (grid.HasKey(t))
                    {
                        viewingDistance++;
                        if (grid[t] >= grid[coord])
                        {
                            coveredCount++;
                            break;
                        }
                        t = t.Move(dir);
                    }

                    scenicScore *= viewingDistance;
                }

                if (coveredCount < 4)
                {
                    visibleCount++;
                }

                maxScenicScore = Math.Max(maxScenicScore, scenicScore);
            }

            part1 = visibleCount;
            Part2Type part2 = maxScenicScore;
            LogAndReset("*1", sw);
            LogAndReset("*2", sw);

            return (part1, part2);
        }
    }
}