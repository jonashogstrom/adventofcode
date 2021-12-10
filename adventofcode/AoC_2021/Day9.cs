using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2021
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day9 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(15, 1134, "Day9_test.txt")]
        [TestCase(537, null, "Day9_sara.txt")]
        [TestCase(500, 970200, "Day9.txt")]
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

            var map = source.ToSparseBuffer(9, c=>int.Parse(c.ToString()));

            Log(map.ToString(i => i.ToString()));

            var basinSizes= new List<int>();
            foreach (var c in map.Keys)
            {
                var isLow = true;
                foreach (var neig in c.GenAdjacent4())
                    if (map[neig] <= map[c])
                    {
                        isLow = false;
                        break;
                    }

                if (isLow)
                {
                    Log("Found low point: " + c + ": "+map[c]);
                    part1 += map[c] + 1;
                    var basin = FindBasin(map, c);
                    basinSizes.Add(basin.Count);
                }
            }

            LogAndReset("Parse", sw);
            LogAndReset("*1", sw);
            part2 = basinSizes.OrderByDescending(x => x).Take(3).Multiply();
            LogAndReset("*2", sw);
            // not 1830
            return (part1, part2);
        }

        private HashSet<Coord> FindBasin(SparseBuffer<int> map, Coord coord)
        {
            var basin = new HashSet<Coord>();
            var queue = new Queue<Coord>();
            queue.Enqueue(coord);
            while (queue.Any())
            {
                var c = queue.Dequeue();
                if (!basin.Contains(c))
                {
                    Log($"Looking at coord {c} with value {map[c]}");
                    basin.Add(c);
                    foreach (var x in c.GenAdjacent4())
                        if (!basin.Contains(x) && map[x] != 9)
                            queue.Enqueue(x);
                }
            }
            return basin;
        }
    }


}