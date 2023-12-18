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
    class Day10 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(8, null, "Day10_test.txt")]
        [TestCase(4, null, "Day10_test2.txt")]
        [TestCase(4, null, "Day10_test3.txt")]
        [TestCase(6942, 297, "Day10.txt")]
        [TestCase(null, 10, "Day10_test4.txt")]
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
            var start = map.Keys.Single(c => map[c] == 'S');

            LogAndReset("Parse", sw);

            var steps = 0;

            var g = new Graph();

            var connectsToWest = new List<char> { '-', 'J', '7', 'S' };
            var connectsToEast = new List<char> { '-', 'L', 'F', 'S' };
            var connectsToNorth = new List<char> { '|', 'J', 'L', 'S' };
            var connectsToSouth = new List<char> { '|', '7', 'F', 'S' };

            foreach (var c in map.Keys)
            {

                var ch = map[c];

                var E = c.Move(Coord.E);
                if (connectsToEast.Contains(ch) && connectsToWest.Contains(map[E]))
                {
                    g.AddEdge(c, E);
                    g.AddEdge(E, c);
                }

                var N = c.Move(Coord.N);
                if (connectsToNorth.Contains(ch) && connectsToSouth.Contains(map[N]))
                {
                    g.AddEdge(c, N);
                    g.AddEdge(N, c);
                }
            }

            var path = new List<Coord>();
            var visited = new HashSet<Coord>();
            var pos = g.neighbors(start).First();
            var prev = start;
            path.Add(start);
            visited.Add(start);
            while (!pos.Equals(start))
            {
                var next = g.neighbors(pos).Single(x => !x.Equals(prev));
                path.Add(pos);
                visited.Add(pos);
                prev = pos;
                pos = next;
            }

            part1 = path.Count / 2;


            LogAndReset("*1", sw);

            var trans = new Dictionary<char, char>
            {
                ['-'] = '\u2550',
                ['|'] = '\u2551',
                ['7'] = '\u2557',
                ['L'] = '\u255a',
                ['J'] = '\u255d',
                ['F'] = '\u2554',
                ['S'] = 'S'
            };

            var newMap = new SparseBuffer<char>('.');
            foreach (var x in path)
            {
                newMap[x] = trans[map[x]];
            }

            // replaced code by computing the manhattan size of the loop and removing the perimeter

            // determine if a square is inside by counting how many times the loop is intersected when moving west from the cell

            // var inside = 0;
            //
            // var coords = map.AllKeysInMap.ToList();
            // foreach (var c in coords)
            // {
            //     if (newMap[c] == '.')
            //     {
            //         var edgeCounter = 0;
            //         var p = c;
            //
            //         while (p.X >= 0)
            //         {
            //             if (visited.Contains(p) && g.HasEdge(p, p.Move(Coord.S)))
            //                 edgeCounter++;
            //             p = p.Move(Coord.W);
            //         }
            //
            //         if (edgeCounter % 2 == 1)
            //         {
            //             newMap[c] = '*';
            //             inside++;
            //         }
            //         else
            //             newMap[c] = ' ';
            //     }
            // }
            //
            // part2 = inside;

            part2 = path.CalcManhattanSize(true);
            Log(newMap.ToString(c => c.ToString()), -1);

            LogAndReset("*2", sw);

            return (part1, part2);
        }
    }
}