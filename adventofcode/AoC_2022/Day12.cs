using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Accord;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2022
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day12 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(31, 29, "Day12_test.txt")]
        [TestCase(456, 454, "Day12.txt")]
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

            var map = source.ToSparseBuffer(' ');
            var startPos = map.Keys.First(k => map[k] == 'S');
            var goal = map.Keys.First(k => map[k] == 'E');
            map[startPos] = 'a';
            map[goal] = 'z';

            LogAndReset("Parse", sw);

            var distances = new SparseBuffer<int>(-1)
            {
                [goal] = 0
            };
            var queue = new Queue<Coord>();
            queue.Enqueue(goal);
            FloodFind(queue, distances, map);
            part1 = distances[startPos];
            LogAndReset("*1", sw);
            part2 = distances.Keys.Where(k => map[k] == 'a').Select(k => distances[k]).OrderBy(x => x).First();
            LogAndReset("*2", sw);
            LogDistMap(distances, 0);

            return (part1, part2);
        }

        private void FloodFind(Queue<Coord> queue, SparseBuffer<int> distances, SparseBuffer<char> map)
        {
            while (queue.Any())
            {
                var p = queue.Dequeue();
                var d = distances[p];
                var elevation = (byte)map[p];
                foreach (var x in p.GenAdjacent4().Where(map.HasKey))
                {
                    if ((byte)map[x] >= elevation - 1)
                        if (distances[x] > d + 1 || distances[x] == -1)
                        {
                            distances[x] = d + 1;
                            queue.Enqueue(x);
                        }
                }

                LogDistMap(distances);
            }
        }

        private void LogDistMap(SparseBuffer<int> distances, int logLevel=0)
        {
            var size = distances.Keys.Count() > 99 ? 4 : 3;

            Log(() => distances.ToString((dist, coord) =>
            {
                if (dist == -1)
                    return ".".PadRight(size);
                return dist.ToString().PadRight(size);
            }, size), logLevel);
        }
    }
}