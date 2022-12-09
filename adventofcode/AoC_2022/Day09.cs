using System;
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
        [TestCase(13, 1, "Day09_test.txt")]
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
            var headPos = new Coord(0, 0);
            var tailPos = new Coord(0, 0);

            var visited = new SparseBuffer<bool>();
            visited[tailPos] = true;
            var stepCounter = 0;
            foreach (var instr in source)
            {
                var parts = instr.Split(' ');
                var dir = Coord.trans2Coord[parts[0][0]];
                var dist = int.Parse(parts[1]);
                for (var i = 0; i < dist; i++)
                {
                    headPos = headPos.Move(dir);
                    if (headPos == tailPos)
                    {
                        // covers
                    }
                    else if (headPos.Dist(tailPos) <= 1)
                    {
                        // adjacent
                    }
                    else if (headPos.GenAdjacent8().Contains(tailPos))
                    {
                        // no action -- corners
                    }
                    else if (tailPos.X == headPos.X || tailPos.Y == headPos.Y)
                    {
                        tailPos = tailPos.Move(dir);
                        Log("MoveTail " + Coord.trans2NESW[dir]);
                    }
                    else // need to move diagonally
                    {
                        if (tailPos.IsNorthOf(headPos)) // above
                        {
                            if (tailPos.IsWestOf(headPos)) // north east
                            {
                                tailPos = tailPos.Move(Coord.SE);
                                Log("MoveTail SE");
                            }

                            else
                            {
                                tailPos = tailPos.Move(Coord.SW);
                                Log("MoveTail SW");
                            }
                        }
                        else // below
                        {
                            if (tailPos.IsWestOf(headPos)) // South east
                                tailPos = tailPos.Move(Coord.NE);
                            else
                            {
                                tailPos = tailPos.Move(Coord.NW);
                            }
                        }
                    }

                    var map = new SparseBuffer<char>('.');
                    map[Coord.Origin] = 's';
                    map[tailPos] = 'T';
                    map[headPos] = 'H';
                    stepCounter++;

                    Log("======= step: " + stepCounter + " - " + instr);
                    Log(map.ToString(c=>c.ToString()));


                    visited[tailPos] = true;
                }
            }
            LogAndReset("Parse", sw);
            part1 = visited.Keys.Count();
            LogAndReset("*1", sw);
            LogAndReset("*2", sw);

            return (part1, part2);
        }
    }
}