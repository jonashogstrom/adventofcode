using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Gaming.Input;
using Accord;
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
            var filledRows = map.Keys.Select(k => k.Row).ToHashSet();
            var filledCols = map.Keys.Select(k => k.Col).ToHashSet();
            var galaxies = new List<Coord>();

            var sum = 0;
            var rowSpacing = new List<int>();
            for (int r = 0; r<=map.Bottom; r++)
            {
                if (!filledRows.Contains(r))
                    sum += factor - 1;
                rowSpacing.Add(sum);
            }

            sum = 0;
            var colSpacing = new List<int>();
            for (int c = 0; c <= map.Right; c++)
            {
                if (!filledCols.Contains(c))
                    sum += factor - 1;
                colSpacing.Add(sum);
            }

            foreach (var x in map.Keys)
            {
                galaxies.Add(x.Move(new Coord(rowSpacing[x.Row], colSpacing[x.Col])));
            }

            var dist = 0L;
            foreach (var x in galaxies.AsCombinations())
                dist += x.Item1.Dist(x.Item2);
            return dist;
        }
    }
}