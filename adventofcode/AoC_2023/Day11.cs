using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2023
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day11 : TestBaseClass<Part1Type, Part2Type>
    {
        private int _factor;
        public bool Debug { get; set; }

        [Test]
        [TestCase(374, 374, "Day11_test.txt", 2)]
        [TestCase(374, 1030, "Day11_test.txt", 10)]
        [TestCase(374, 8410, "Day11_test.txt", 100)]
        [TestCase(9418609, 593821230983, "Day11.txt", 1000000)]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName, int factor)
        {
            _factor = factor;
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

            var map = source.ToSparseBuffer('.');

            LogAndReset("Parse", sw);

            part1 = ComputeDistance(map, 2);

            LogAndReset("*1", sw);
            part2 = ComputeDistance(map, _factor);

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private long ComputeDistance(SparseBuffer<char> map, int factor)
        {
            var galaxies = map.Keys;
            var filledRows = galaxies.Select(k => k.Row).ToHashSet();
            var filledCols = galaxies.Select(k => k.Col).ToHashSet();

            var rowSpacing = CalculateExpansion(factor, filledRows, map.Bottom);
            var colSpacing = CalculateExpansion(factor, filledCols, map.Right);

            var expandedGalaxies = galaxies.Select(x => new Coord(x.Row + rowSpacing[x.Row], x.Col + colSpacing[x.Col])).ToList();

            return expandedGalaxies.AsCombinations().Sum(g => (long)g.Item1.Dist(g.Item2));
        }

        private static List<int> CalculateExpansion(int factor, HashSet<int> filled, int max)
        {
            var accumulatedExpansion = 0;
            var expansion = new List<int>();
            for (int i = 0; i <= max; i++)
            {
                if (!filled.Contains(i))
                    accumulatedExpansion += factor - 1;
                expansion.Add(accumulatedExpansion);
            }

            return expansion;
        }
    }
}