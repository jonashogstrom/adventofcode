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
    class Day14 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(136, 64, "Day14_test.txt")]
        [TestCase(108641, 84328, "Day14.txt")]
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


            var map = source.ToSparseBuffer('.');

            LogAndReset("Parse", sw);

            var bottomRow = map.Bottom;

            map.AddBorders('#');
            var boulders = map.Keys.Where(k => map[k] == 'O').ToHashSet();
            var rocks = map.Keys.Where(k => map[k] == '#').ToHashSet();

            var map2 = InitMap2(map, boulders, rocks);

            boulders = TiltMap2(map2, boulders.OrderBy(b => b.Row), rocks, Coord.N);
            part1 += CalculateLoad(boulders, bottomRow);



            LogAndReset("*1", sw);

            map = source.ToSparseBuffer('.');

            map.AddBorders('#');
            boulders = map.Keys.Where(k => map[k] == 'O').ToHashSet();
            rocks = map.Keys.Where(k => map[k] == '#').ToHashSet();

            map2 = InitMap2(map, boulders, rocks);

            Log(() => map.ToString());

            var loads = new List<long>();


            var iteration = 0;
            int cycle;
            while (true)
            {
                boulders = TiltMap2(map2, boulders.OrderBy(b => b.Row), rocks, Coord.N);
                boulders = TiltMap2(map2, boulders.OrderBy(b => b.Col), rocks, Coord.W);
                boulders = TiltMap2(map2, boulders.OrderBy(b => -b.Row), rocks, Coord.S);
                boulders = TiltMap2(map2, boulders.OrderBy(b => -b.Col), rocks, Coord.E);
                var load = CalculateLoad(boulders, bottomRow);
                loads.Add(load);
                Log(() => $"Round: {iteration}, load: {load}");
                if (loads.FindCycle(out cycle))
                    break;
                iteration++;
            }
            var max = 1000000000;
            var skipCycles = max / cycle;

            var part2Pos = max - skipCycles * cycle - 1;
            while (part2Pos + cycle < loads.Count)
                part2Pos += cycle;

            part2 = loads[part2Pos];

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private static DenseArray<char> InitMap2(SparseBuffer<char> map, HashSet<Coord> boulders, HashSet<Coord> rocks)
        {
            var map2 = new DenseArray<char>(map.Width, map.Height, '.');

            foreach (var b in boulders)
                map2.SetValue(b.Row + 1, b.Col + 1, 'O');
            foreach (var b in rocks)
                map2.SetValue(b.Row + 1, b.Col + 1, '#');
            return map2;
        }

        private HashSet<Coord> TiltMap2(DenseArray<char> denseArray, IOrderedEnumerable<Coord> boulders,
            HashSet<Coord> rocks, Coord dir)
        {
            var bouldersList = boulders.ToList();
            var boulderMap = bouldersList.ToHashSet();
            foreach (var b in bouldersList)
            {
                var r = b.Row;
                var c = b.Col;
                var nextr = r + dir.Row;
                var nextc = c + dir.Col;
                while (denseArray.GetValue(nextr+1, nextc+1)== '.')
                {
                    r = nextr;
                    c = nextc;
                    nextr = r + dir.Row;
                    nextc = c + dir.Col;
                }

                if (r != b.Row || c != b.Col)
                {
                    boulderMap.Remove(b);
                    boulderMap.Add(new Coord(r, c));
                    denseArray.SetValue(b.Row+1, b.Col+1, '.');
                    denseArray.SetValue(r+1, c+1, 'O');
                }
            }
            Log(denseArray.ToString);
            return boulderMap;
        }

        private static long CalculateLoad(HashSet<Coord> boulders, int bottomRow)
        {
            var load = 0L;
            foreach (var k in boulders)
                load += bottomRow - k.Row + 1;
            return load;
        }
    }
}