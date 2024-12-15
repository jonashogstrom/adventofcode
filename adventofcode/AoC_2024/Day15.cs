using System;
using System.Diagnostics;
using NUnit.Framework;
using System.Collections.Generic;
using AdventofCode.Utils;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Accord;
using NUnit.Framework.Constraints;


namespace AdventofCode.AoC_2024
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day15 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(2028, null, "Day15_test1.txt")]
        [TestCase(10092, 9021, "Day15_test2.txt")]
        [TestCase(null, 105 + 207 + 306, "Day15_test3.txt")]
        [TestCase(1509863, 1548815, "Day15.txt")]
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

            var g = source.AsGroups().ToArray();
            var map = g[0].ToSparseBufferStr('.');
            Log(map.ToString());
            var sb = new StringBuilder();
            foreach (var s in g[1])
                sb.Append(s);
            var instructions = sb.ToString();

            var blocks = map.Keys.Where(k => map[k] == 'O').ToHashSet();
            var walls = map.Keys.Where(k => map[k] == '#').ToHashSet();
            var robot = map.Keys.Single(k => map[k] == '@');

            LogAndReset("Parse", sw);

            foreach (var i in instructions)
            {
                var dir = CoordStr.trans2Coord[i];
                var p = robot.Move(dir);
                var pushed = new List<CoordStr>();
                var done = false;
                while (!done)
                {
                    if (walls.Contains(p))
                        done = true;
                    else if (blocks.Contains(p))
                    {
                        pushed.Add(p);
                        p = p.Move(dir);
                    }
                    else
                    {
                        if (pushed.Any())
                        {
                            blocks.Remove(pushed.First());
                            blocks.Add(p);
                        }

                        robot = robot.Move(dir);
                        done = true;
                    }
                }
            }

            foreach (var b in blocks)
                part1 += b.Row * 100 + b.Col;

            LogAndReset("*1", sw);

            var map2 = new SparseBufferStr<char>('.');
            var blockPositions = new Dictionary<CoordStr, Block>();
            var blocks2 = new List<Block>();

            foreach(var b in map.Keys.Where(k => map[k] == 'O'))
            {
                var block = new Block(map2, b.MoveX(b.Col), blockPositions);
                blocks2.Add(block);
            }
            foreach (var w in map.Keys.Where(k => map[k] == '#'))
            {
                map2[w.MoveX(w.Col)] = '#';
                map2[w.MoveX(w.Col + 1)] = '#';
            }

            robot = map.Keys.Single(k => map[k] == '@');
            robot = robot.Move(new CoordStr(0, robot.Col));

            foreach (var i in instructions)
            {
                var dir = CoordStr.trans2Coord[i];
                var p = robot.Move(dir);
                if (map2[p] != '#')
                {
                    if (blockPositions.TryGetValue(p, out var block))
                    {
                        if (block.CanMove(dir))
                        {
                            block.ForceMove(dir);
                            robot = p;
                        }
                    }
                    else
                    {
                        robot = p;
                    }
                        
                }
                
            }

            foreach (var b in blocks2)
            {
                part2 += b.Coord.Row*100+b.Coord.Col;
            }
            
            LogAndReset("*2", sw);

            return (part1, part2);
        }

        internal class Block
        {
            private readonly SparseBufferStr<char> _map;
            private CoordStr _coord;
            private readonly Dictionary<CoordStr, Block> _blockPositions;

            public Block(SparseBufferStr<char> map, CoordStr coord, Dictionary<CoordStr, Block> blockPositions)
            {
                _map = map;
                _coord = coord;
                _blockPositions = blockPositions;
                
                MyCoords().ForEach(c=>_blockPositions.Add(c, this));
            }

            public CoordStr Coord => _coord;

            private IEnumerable<CoordStr> MyCoords()
            {
                yield return _coord;
                yield return _coord.Move(CoordStr.E);
            }

            public bool CanMove(CoordStr dir)
            {
                var newCoords = GetNextCoords(dir);

                var movedBlocks = new HashSet<Block>();
                foreach (var c in newCoords)
                {
                    if (_map[c] == '#')
                        return false;
                    
                    if (_blockPositions.TryGetValue(c, out var otherBlock))
                        movedBlocks.Add(otherBlock);
                }

                return movedBlocks.All(b => b.CanMove(dir));
            }
            public void ForceMove(CoordStr dir)
            {
                var newCoords = GetNextCoords(dir);
                
                foreach (var c in newCoords)
                {
                    if (_blockPositions.TryGetValue(c, out var otherBlock))
                        otherBlock.ForceMove(dir);
                }

                MyCoords().ForEach(c=>_blockPositions.Remove(c));
                _coord = _coord.Move(dir);
                MyCoords().ForEach(c=>_blockPositions.Add(c, this));
            }

            private List<CoordStr> GetNextCoords(CoordStr dir)
            {
                var newCoords = new List<CoordStr>();
                if (dir.Equals(CoordStr.N) || dir.Equals(CoordStr.S))
                {
                    MyCoords().ForEach(c=>newCoords.Add(c.Move(dir)));
                } 
                else if (dir.Equals(CoordStr.E))
                {
                    newCoords.Add(_coord.Move(dir, 2));
                }
                else if (dir.Equals(CoordStr.W))
                {
                    newCoords.Add(_coord.Move(dir));
                }

                return newCoords;
            }
        }

        private void DrawMap2(HashSet<CoordStr> blocks, HashSet<CoordStr> walls, CoordStr robot)
        {
            var map = new SparseBufferStr<char>('.');
            foreach (var b in blocks)
            {
                map[b] = '[';
                map[b.MoveX(1)] = ']';
            }

            foreach (var w in walls)
            {
                map[w] = '#';
                map[w.MoveX(1)] = '#';
            }

            map[robot] = '@';
            Log(map.ToString());
        }
    }
}