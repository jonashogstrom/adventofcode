using System;
using System.Collections.Generic;
using System.Diagnostics;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2021
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day15 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(40, 315, "Day15_test.txt")]
        [TestCase(581, 2916, "Day15.txt")]
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
            var map = source.ToSparseBuffer(-1, c => int.Parse(c.ToString()));
            LogAndReset("Parse", sw);
            part1 = FindWay(map);
            LogAndReset("*1", sw);

            var largeMap = ExpandMap(map, 5);
            part2 = FindWay(largeMap);
            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private static SparseBuffer<int> ExpandMap(SparseBuffer<int> map, int size)
        {
            var largeMap = new SparseBuffer<int>(-1);
            foreach (var c in map.Keys)
            {
                for (int x = 0; x < size; x++)
                {
                    for (int y = 0; y < size; y++)
                    {
                        var newCoord = Coord.FromXY(c.X + map.Width * x, c.Y + map.Height * y);
                        var newValue = ((map[c] - 1 + x + y) % 9) + 1;
                        largeMap[newCoord] = newValue;
                    }
                }
            }

            return largeMap;
        }

        private long FindWay(SparseBuffer<int> map)
        {
            var start = new Coord(0, 0);
            var end = new Coord(map.Height - 1, map.Width - 1);

            var best = new SparseBuffer<long>(int.MaxValue);
            best[start] = 1;

            for (var y = 0; y < map.Width; y++)
                for (var x = 0; x < map.Width; x++)
                {
                    var c = Coord.FromXY(x, y);
                    if (!c.Equals(start))
                    {
                        var bestN = best[c.Move(Coord.N)];
                        var bestW = best[c.Move(Coord.W)];
                        var newBest = Math.Min(bestN - 1, bestW - 1) + 1 + map[c];
                        best[c] = newBest;
                    }
                }
            best[start] = 0;
            var temp = best[end];

//            Log(best.ToString((i, c) => i.ToString().PadLeft(4), 4));

            var worst = best[end];
            var path = new List<Coord> { start };
            return FindWay2(map, best, start, end, worst, path);
        }

        private long FindWay2(SparseBuffer<int> map, SparseBuffer<long> best, Coord start, Coord end, long worst,
            List<Coord> path)
        {
            if (start.Equals(end))
            {
                Log("Reached end at cost " + best[end]);
                Log(map.ToString((i, c) => path.Contains(c) ? "." : " "));
                return best[end];
            }

            var res = long.MaxValue;
            if (best[start] > worst)
                return long.MaxValue;
            foreach (var n in start.GenAdjacent4())
            {
                if (map.HasKey(n))
                {
                    var newCost = best[start] + map[n];
                    if (best[n] > newCost)
                    {
                        best[n] = newCost;

                        var newPath = new List<Coord>(path) { n };
                        res = Math.Min(res, FindWay2(map, best, n, end, worst, newPath));
                        worst = Math.Min(res, worst);
                    }
                }
            }

            return res;
        }
    }
}