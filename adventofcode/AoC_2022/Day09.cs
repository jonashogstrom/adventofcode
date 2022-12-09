using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2022
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day09 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(null, 1, "Day09_test.txt")]
        [TestCase(null, 36, "Day09_test2.txt")]
        [TestCase(6376, null, "Day09.txt")]
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
            // var prev = new Coord(0, 0);
            // var k.Pos = new Coord(0, 0);

            var visited = new SparseBuffer<bool>();
            var stepCounter = 0;
            var ropeLength = 10;
            var knots = new List<Knot>();
            knots.Add(new Knot(Coord.Origin, 'H'));
            for (int x = 1; x < ropeLength; x++)
            {
                knots.Add(new Knot(Coord.Origin, x.ToString()[0]));
            }
            visited[knots.Last().Pos] = true;

            foreach (var instr in source)
            {
                var parts = instr.Split(' ');
                var dir = Coord.trans2Coord[parts[0][0]];
                var dist = int.Parse(parts[1]);
                for (var i = 0; i < dist; i++)
                {
                    knots[0].Pos = knots[0].Pos.Move(dir);
                    var prev = knots[0].Pos;
                    foreach (var k in knots.Skip(1))
                    {
                        if (prev == k.Pos)
                        {
                            // covers
                        }
                        else if (prev.Dist(k.Pos) <= 1)
                        {
                            // adjacent
                        }
                        else if (prev.GenAdjacent8().Contains(k.Pos))
                        {
                            // no action -- corners
                        }
                        else if (k.Pos.Y == prev.Y )
                        {
                            if (k.Pos.IsWestOf(prev))
                                k.Pos = k.Pos.Move(Coord.E);
                            else 
                                k.Pos = k.Pos.Move(Coord.W);
                        } 
                        else if (k.Pos.X == prev.X)
                        {
                            if (k.Pos.IsNorthOf(prev))
                                k.Pos = k.Pos.Move(Coord.S);
                            else
                                k.Pos = k.Pos.Move(Coord.N);
                        }
                        else // need to move diagonally
                        {
                            if (k.Pos.IsNorthOf(prev)) // above
                            {
                                if (k.Pos.IsWestOf(prev)) // north east
                                {
                                    k.Pos = k.Pos.Move(Coord.SE);
                                }

                                else
                                {
                                    k.Pos = k.Pos.Move(Coord.SW);
                                }
                            }
                            else // below
                            {
                                if (k.Pos.IsWestOf(prev)) // South east
                                    k.Pos = k.Pos.Move(Coord.NE);
                                else
                                {
                                    k.Pos = k.Pos.Move(Coord.NW);
                                }
                            }
                        }
                        prev = k.Pos;
                    }

                    visited[knots.Last().Pos] = true;
                    stepCounter++;
                    if (instr == "R 17")
                        LogMap(visited, knots, stepCounter, instr);
                }

                LogMap(visited, knots, stepCounter, instr);
                if (instr == "R 17")
                {
                    Log("here");
                }
            }
            LogAndReset("Parse", sw);
            part2 = visited.Keys.Count();
            LogAndReset("*1", sw);
            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private void LogMap(SparseBuffer<bool> visited, List<Knot> knots, int stepCounter, string instr)
        {
            var map = new SparseBuffer<char>('.');
            foreach (var x in visited.Keys)
                map[x] = '#';

            map[Coord.Origin] = 's';
            foreach (var k in knots.Skip(0).Reverse())
            {
                map[k.Pos] = k.Name;
            }

            Log("======= step: " + stepCounter + " - " + instr);
            Log(map.ToString(c => c.ToString()));
        }
    }

    internal class Knot
    {
        public Coord Pos { get; set; }
        public char Name { get; }

        public Knot(Coord pos, char name)
        {
            Pos = pos;
            Name = name;
        }
    }
}