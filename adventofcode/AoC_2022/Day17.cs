using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2022
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day17 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(3068, 1514285714288, "Day17_test.txt")]
        [TestCase(3149, 1553982300884, "Day17.txt")]
        [TestCase(null, null, "Day17_jesper.txt")]
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

            var jets = source[0].AsEnumerable().Select(c => Coord.trans2Coord[c]).ToList();

            part1 = DropAlotOfShapes(2022, jets);

            LogAndReset("*1", sw);

            part2  = DropAlotOfShapes(1000000000000, jets);
            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private long DropAlotOfShapes(long iterations, List<Coord> jets)
        {
            var map = new SparseBuffer<char>(' ');
            var jetCounter = 0;
            var checkpoints = new Dictionary<int, (long shapeIndex, int height)>();
            var checker = 0;
            var checkerLimit = 5;
            var skippedHeight = 0L;
            for (long shapeCounter = 0; shapeCounter < iterations; shapeCounter++)
            {
                var shapeIndex = shapeCounter % 5;
                jetCounter = DropOnePiece((int)shapeIndex, map, jets, jetCounter);

                // check for |..####.|
                if (shapeIndex == 0 && map[new Coord(map.Top, 2)] == ' ' && map[new Coord(map.Top, 7)] == ' ')
                {
                    var jetIndex = jetCounter % jets.Count;
                    var height = -map.Top;
                    if (checker < checkerLimit && checkpoints.TryGetValue(jetIndex, out var last))
                    {
                        checker++;
                        var cycleSize = shapeCounter - last.shapeIndex;
                        var increase = height - last.height;
                        var left = iterations - shapeCounter;
                        var cycles = left / cycleSize;
                        Log(()=>$"ShapeCounter: {shapeCounter} JetIndex: {jetIndex} cycleSize {cycleSize}, increase: {increase}", -1);
                        if (checker == checkerLimit)
                        {
                            shapeCounter += cycles * cycleSize;
                            skippedHeight = cycles * increase;
                            Log(()=>$"Skipped {cycles * cycleSize} (height: {cycles * increase})", -1);
                        }
                    }

                    checkpoints[jetIndex] = (shapeCounter, height);
                }
            }

            return -map.Top + skippedHeight;
        }

        private int DropOnePiece(int shapeIndex, SparseBuffer<char> map, List<Coord> jets, int jetCounter)
        {
            var shape = GetShape(shapeIndex, map.Top - 4);
            while (true)
            {
                var jet = jets[jetCounter % jets.Count];
                PushShape(shape, jet, map);
                jetCounter++;
                if (!DropShape(shape, map))
                    break;
            }

            foreach (var c in shape)
                map[c] = '#';
            //            Log(() => map.ToString(c => c.ToString()));
            return jetCounter;
        }

        private bool DropShape(List<Coord> shape, SparseBuffer<char> map)
        {
            for (int i = 0; i < shape.Count; i++)
            {
                var newPos = shape[i].Move(Coord.S);
                if (map[newPos] != ' ' || newPos.Y == 0)
                    return false;
            }

            MoveShape(shape, Coord.S);
            return true;
        }

        private void PushShape(List<Coord> shape, Coord jet, SparseBuffer<char> sparseBuffer)
        {
            foreach (var c in shape)
            {
                var newPos = c.Move(jet);
                if (sparseBuffer[newPos] != ' ')
                    return;
                if (newPos.X < 1 || newPos.X > 7)
                    return;
            }
            MoveShape(shape, jet);
        }

        private static void MoveShape(List<Coord> shape, Coord dir)
        {
            for (int i = 0; i < shape.Count; i++)
                shape[i] = shape[i].Move(dir);
        }

        private List<Coord> GetShape(int shapeIndex, int vPos)
        {
            var res = new List<Coord>();
            switch ((shapeIndex) % 5)
            {
                case 0: // Shape -
                    res.Add(new Coord(vPos, 3));
                    res.Add(new Coord(vPos, 4));
                    res.Add(new Coord(vPos, 5));
                    res.Add(new Coord(vPos, 6));
                    break;
                case 1: // Shape +
                    res.Add(new Coord(vPos, 4));
                    res.Add(new Coord(vPos - 1, 3));
                    res.Add(new Coord(vPos - 1, 4));
                    res.Add(new Coord(vPos - 1, 5));
                    res.Add(new Coord(vPos - 2, 4));
                    break;
                case 2: // Shape backward L
                    res.Add(new Coord(vPos, 3));
                    res.Add(new Coord(vPos, 4));
                    res.Add(new Coord(vPos, 5));
                    res.Add(new Coord(vPos - 1, 5));
                    res.Add(new Coord(vPos - 2, 5));
                    break;
                case 3: // Shape backward |
                    res.Add(new Coord(vPos, 3));
                    res.Add(new Coord(vPos - 1, 3));
                    res.Add(new Coord(vPos - 2, 3));
                    res.Add(new Coord(vPos - 3, 3));
                    break;
                case 4: // Shape plutt
                    res.Add(new Coord(vPos, 3));
                    res.Add(new Coord(vPos, 4));
                    res.Add(new Coord(vPos - 1, 3));
                    res.Add(new Coord(vPos - 1, 4));
                    break;
            }

            return res;
        }
    }
}