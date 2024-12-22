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

    // 1418 is too low
    [TestFixture]
    class Day20 : TestBaseClass<Part1Type, Part2Type>
    {
        private int _threshold;
        private int _thresholdp2;
        public bool Debug { get; set; }

        [Test]
        [TestCase(30, 285, "Day20_test.txt", 3, 50)]
        [TestCase(1452, 999556, "Day20.txt", 100, 100)]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName, int threshold, int thresholdp2)
        {
            _threshold = threshold;
            _thresholdp2 = thresholdp2;
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

            var map = source.ToSparseBuffer(' ');
            var start = map.Keys.Single(k => map[k] == 'S');
            var end = map.Keys.Single(k => map[k] == 'E');
            var path = new Dictionary<Coord, int>();

            path[start] = 0;
            var p = start;
            //var length = 0;
            while (!p.Equals(end))
            {
                foreach (var n in p.GenAdjacent4())
                {
                    if (map.InsideBounds(n) && map[n] != '#' && !path.ContainsKey(n))
                    {
                        path[n] = path[p] + 1;
                        p = n;
                        break;
                    }
                }
            }

            LogAndReset("Parse", sw);

            foreach (var k in map.Keys.Where(k => map[k] == '#'))
            {
                var emptyNeighbours = k.GenAdjacent4().Where(n => path.ContainsKey(n)).ToArray();
                if (emptyNeighbours.Length is 2 or 3)
                {
                    var pathPositions = emptyNeighbours.OrderBy(n=>path[n]).ToArray();

                    var first = pathPositions.First();
                    var last = pathPositions.Last();
                    var firstPos = path[first];
                    var lastPos = path[last];
                    Log($"Skip from {first} to {last}, save {lastPos-firstPos-2} steps ({emptyNeighbours.Length} empty neighbours)");
                    if ((lastPos - firstPos) - 2 >= _threshold)
                        part1++;
                }
            }

            LogAndReset("*1", sw);

            foreach (var p1 in path.Keys)
            {
                foreach (var p2 in path.Keys)
                {
                    var dist = p1.Dist(p2);
                    if (dist <= 20)
                    {
                        var diff = path[p2] - path[p1];
                        var gain = diff - dist;
                        if (gain >= _thresholdp2)
                            part2++;
                    }
                }
            }
            // solve part 2 here

            LogAndReset("*2", sw);

            return (part1, part2);
        }
    }
}