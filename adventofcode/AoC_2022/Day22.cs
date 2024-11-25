using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2022
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day22 : TestBaseClass<Part1Type, Part2Type>
    {
        private int _cubeSize;
        private bool _testdata;
        public bool Debug { get; set; }

        [Test]
        [TestCase(6032, 5031, "Day22_test.txt")]
        [TestCase(1484, 142228, "Day22.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            _testdata = resourceName.Contains("test");
            LogLevel = _testdata ? 20 : -1;
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1 = 0;
            Part2Type part2 = 0;
            var sw = Stopwatch.StartNew();

            // parse input here

            LogAndReset("Parse", sw);
            var data = source.AsGroups();
            var map = data.First().ToSparseBuffer(' ');
            var path = data.Last().First();

            var startPos = map.Keys.Where(x => x.Y == 0 && map[x] == '.').OrderBy(x => x.X).First();

            // solve part 1 here

            var instructions = Tokenize(path).ToList();
            var res = ExecuteInstructions(startPos, instructions, map);

            part1 = CalcScore(res);
            LogAndReset("*1", sw);
            // solve part 2 here
            _cubeSize = Math.Max(map.Width, map.Height) / 4;

            map = data.First().ToSparseBuffer(' ');
            var warpMap = new Dictionary<(Coord, Coord), (Coord, Coord)>();
            var topLeft = new Coord(0, 0);
            var topRight = new Coord(0, _cubeSize - 1);
            var bottomRight = new Coord(_cubeSize - 1, _cubeSize - 1);
            var bottomLeft = new Coord(_cubeSize - 1, 0);
            if (_testdata)
            {
                var sides = new List<Coord>()
                {
                    new Coord(0, 2),
                    new Coord(1, 0),
                    new Coord(1, 1),
                    new Coord(1, 2),
                    new Coord(2, 2),
                    new Coord(2, 3),
                };
                AddWarpEdge(warpMap, sides,
                    1, topLeft, 'v',
                    3, topLeft, '>', '<', '^');

                AddWarpEdge(warpMap, sides,
                    1, topLeft, '>',
                    2, topRight, '<', '^', '^');

                AddWarpEdge(warpMap, sides,
                    3, bottomLeft, '>',
                    5, bottomLeft, '^', 'v', '<');

                AddWarpEdge(warpMap, sides,
                    4, topRight, 'v',
                    6, topRight, '<', '>', '^');

                AddWarpEdge(warpMap, sides,
                    1, topRight, 'v',
                    6, bottomRight, '^', '>', '<');

                AddWarpEdge(warpMap, sides,
                    2, bottomLeft, '>',
                    5, bottomRight, '<', 'v', 'v');

                AddWarpEdge(warpMap, sides,
                    2, topLeft, 'v',
                    6, bottomRight, '<', '<', 'v');
            }
else
            {
                var sides = new List<Coord>()
                {
                    new Coord(0, 2),
                    new Coord(0, 1),
                    new Coord(1, 1),
                    new Coord(2, 1),
                    new Coord(2, 0),
                    new Coord(3, 0),
                };
                AddWarpEdge(warpMap, sides,
                    1, bottomLeft, '>',
                    3, topRight, 'v', 'v', '>');

                AddWarpEdge(warpMap, sides,
                    3, bottomLeft, '^',
                    5, topRight, '<', '<', '^');

                AddWarpEdge(warpMap, sides,
                    4, bottomLeft, '>',
                    6, topRight, 'v', 'v', '>');

                AddWarpEdge(warpMap, sides,
                    1, bottomRight, '^',
                    4, topRight, 'v', '>', '>');

                AddWarpEdge(warpMap, sides,
                    1, topLeft, '>',
                    6, bottomLeft, '>', '^', 'v');

                AddWarpEdge(warpMap, sides,
                    2, topLeft, 'v',
                    5, bottomLeft, '^', '<', '<');

                AddWarpEdge(warpMap, sides,
                    2, topLeft, '>',
                    6, topLeft, 'v', '^', '<');
            }


            res = ExecuteInstructions2(startPos, instructions, map, warpMap);
            part2 = CalcScore(res);

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private void AddWarpEdge(
            Dictionary<(Coord, Coord), (Coord, Coord)> warpMap,
            List<Coord> sideCoordinates,
            int side1Index, Coord side1Start, char edgeDir1,
            int side2Index, Coord side2Start, char edgeDir2, char moveDir1, char moveDir2)
        {
            var p1 = sideCoordinates[side1Index-1].Multiply(_cubeSize).Move(side1Start);
            var p2 = sideCoordinates[side2Index-1].Multiply(_cubeSize).Move(side2Start);
            for (int i = 0; i < _cubeSize; i++)
            {
                warpMap[(p1, Coord.CharToDir(moveDir1))] = (p2, Coord.CharToDir(moveDir2).RotateCWDegrees(180));
                warpMap[(p2, Coord.CharToDir(moveDir2))] = (p1, Coord.CharToDir(moveDir1).RotateCWDegrees(180));
                p1 = p1.Move(Coord.CharToDir(edgeDir1));
                p2 = p2.Move(Coord.CharToDir(edgeDir2));
            }
        }

        private static int CalcScore((Coord pos, Coord dir) res)
        {
            var dirScore = GetDirScore(res);
            return (res.pos.Y + 1) * 1000 + (res.pos.X + 1) * 4 + dirScore;
        }

        private static int GetDirScore((Coord pos, Coord dir) res)
        {
            var dirScore = -1;
            switch (Coord.trans2Arrow[res.dir])
            {
                case '>':
                    dirScore = 0;
                    break;
                case 'v':
                    dirScore = 1;
                    break;
                case '<':
                    dirScore = 2;
                    break;
                case '^':
                    dirScore = 3;
                    break;
            }

            return dirScore;
        }

        private (Coord pos, Coord dir) ExecuteInstructions(Coord startPos, List<string> instructions,
            SparseBuffer<char> map)
        {
            var pos = startPos;
            var dir = Coord.E;
            foreach (var instr in instructions)
            {
                if (int.TryParse(instr, out var dist))
                {
                    for (int i = 0; i < dist; i++)
                    {
                        var next = MapWrap(map, pos, dir);

                        if (map[next.pos] != '#')
                        {
                            map[pos] = Coord.trans2Arrow[dir];
                            pos = next.pos;
                            dir = next.dir;
                        }
                        else
                        {
                            break;
                        }
                    }

                  // Log(() => map.ToString(c => c.ToString()));
                }
                else if (instr == "R")
                {
                    dir = dir.RotateCW90();
                }
                else
                    dir = dir.RotateCCW90();
            }


            return (pos, dir);
        }

        private (Coord pos, Coord dir) ExecuteInstructions2(Coord startPos, List<string> instructions,
            SparseBuffer<char> map, Dictionary<(Coord pos, Coord dir), (Coord pos, Coord dir)> warpMap)
        {
            var pos = startPos;
            int warpCounter = 0;
            int stepCounter = 0;
            var dir = Coord.E;
            foreach (var instr in instructions)
            {
                if (int.TryParse(instr, out var dist))
                {
                    for (int i = 0; i < dist; i++)
                    {
                        if (!warpMap.TryGetValue((pos, dir), out var next))
                        {
                            next = (pos.Move(dir), dir);
                        }
                        else
                        {
                            warpCounter++;
                        }

                        stepCounter++;
                        if (map[next.pos] != '#')
                        {
                            map[pos] = Coord.trans2Arrow[dir];
                            pos = next.pos;
                            dir = next.dir;
                        }
                        else
                        {
                            break;
                        }
                    }

                    Log(() => map.ToString(c => c.ToString()));
                }
                else if (instr == "R")
                {
                    dir = dir.RotateCW90();
                }
                else
                    dir = dir.RotateCCW90();
            }

            Log($"Steps: {stepCounter}", -1);
            Log($"warps: {warpCounter}", -1);

            Log(() => map.ToString(c => c.ToString()),-1);
            return (pos, dir);
        }


        private IEnumerable<string> Tokenize(string path)
        {
            var temp = "";
            var ParsingDigits = true;
            foreach (var x in path)
            {
                if (ParsingDigits && (x == 'R' || x == 'L'))
                {
                    yield return temp;
                    temp = "";
                    ParsingDigits = false;
                }
                else if (!ParsingDigits && char.IsDigit(x))
                {
                    yield return temp;
                    temp = "";
                    ParsingDigits = true;
                }

                temp += x;
            }

            yield return temp;
        }

        private (Coord pos, Coord dir) MapWrap(SparseBuffer<char> map, Coord pos, Coord dir)
        {
            pos = pos.Move(dir);
            var c = map[pos];
            while (c == ' ')
            {
                if (pos.X > map.Right)
                    pos = new Coord(pos.Y, 0);
                else if (pos.X < map.Left)
                    pos = new Coord(pos.Y, map.Right);
                else if (pos.Y > map.Bottom)
                    pos = new Coord(0, pos.X);
                else if (pos.Y < map.Top)
                    pos = new Coord(map.Bottom, pos.X);
                else
                {
                    pos = pos.Move(dir);
                }

                c = map[pos];
            }

            return (pos, dir);
        }
    }
}