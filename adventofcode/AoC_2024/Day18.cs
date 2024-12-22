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
    class Day18 : TestBaseClass<Part1Type, Part2Type>
    {
        private int _size;
        private int _p1Count;
        public bool Debug { get; set; }

        [Test]
        [TestCase(22, 60001, "Day18_test.txt", 7, 12)]
        [TestCase(432, 560027, "Day18.txt", 71, 1024)]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName, int size, int p1Count)
        {
            _size = size;
            _p1Count = p1Count;
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

            var map = new SparseBuffer<char>('.');
            
            for (int i = 0; i<_p1Count; i++)
            {
                var c = Coord.Parse(source[i]);
                map[c] = '#';
            }

            var start = new Coord(0, 0);
            var end = new Coord(_size - 1, _size - 1);
     
            //Log(map.ToString(), LogLevel);

            LogAndReset("Parse", sw);
            var solution = SolveMaze(start, end, map);
            part1 = solution.length;
            LogAndReset("*1", sw);

            var solvedMazes = 1;
            var lineNum = _p1Count;
            while (lineNum < source.Length)
            {
                var coord = Coord.Parse(source[lineNum]);
                map[coord] = '#';
                if (solution.path.Contains(coord) || true)
                {
                    solvedMazes++;
                    solution = SolveMaze(start, end, map);
                    if (solution.length == int.MaxValue)
                    {
                        part2 = coord.Col * 10000 + coord.Row;
                        Log($"Part2 solution: {coord.Col},{coord.Row}", LogLevel);
                        break;
                    }
                }
                lineNum++;
            }
            Log($"Solved {solvedMazes} mazes", LogLevel);
            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private (Part1Type length, HashSet<Coord> path) SolveMaze(Coord start, Coord end, SparseBuffer<char> map)
        {
            var toExplore = new PriorityQueue<Coord, int>();
            toExplore.Enqueue(start, _size+_size);
            var distances = new Dictionary<Coord, int>();
            distances[start] = 0;
            var paths = new Dictionary<Coord, Coord>();
            var pathLength = int.MaxValue;
            while (toExplore.Count > 0)
            {
                var p = toExplore.Dequeue();
                var dist = distances[p];
                if (p.Equals(end) && dist < pathLength)
                {
                    pathLength = dist;
                    continue;
                }
                if (dist >= pathLength)
                {
                    continue;
                }
                foreach (var n in p.GenAdjacent4().Where(n=>map.InsideBounds(n) && map[n] == '.'))
                {
                    var nDist = dist + 1;
                    if (!distances.TryGetValue(n, out var oldNDist) || oldNDist > nDist)
                    {
                        distances[n] = nDist;
                        paths[n] = p;
                        toExplore.Enqueue(n, nDist + (_size-n.Col + _size-n.Row)/2);
                    }
                }
            }

            var path = new HashSet<Coord> { end };
            var step = end;
            while (paths.TryGetValue(step, out var prev))
            {
                path.Add(prev);
                step = prev;
            }

            return (pathLength, path);
        }
    }
}