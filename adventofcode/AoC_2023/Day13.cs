using System;
using System.Diagnostics;
using System.Linq;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2023
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    // not 31849
    // not 35549
    // not 37072


    [TestFixture]
    class Day13 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(405, 400, "Day13_test.txt")]
        //[TestCase(405, null, "Day13_test2.txt")]
        [TestCase(34889, null, "Day13.txt")]
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

            var maps = source.AsGroups().Select(g => g.ToSparseBuffer('.'));
            // parse input here

            LogAndReset("Parse", sw);

            foreach (var map in maps)
            {
                Log("=============================================================", -1);
                part1 += FindLine(map, 0);
            }

            LogAndReset("*1", sw);

            foreach (var map in maps)
            {
                Log("=============================================================", -1);
                part2 += FindLine(map, 1);
            }


            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private long FindLine(SparseBuffer<char> map, int errors)
        {
            var sum = 0;
            for (int x = 1; x <= map.Right; x++)
                if (MapHasVerticalFold(x, map) == errors)
                {
                    var printMap = map.Clone();
                    printMap[Coord.FromXY(x - 1, -1)] = '>';
                    printMap[Coord.FromXY(x, -1)] = '<';
                    printMap[Coord.FromXY(x - 1, printMap.Bottom + 1)] = '>';
                    printMap[Coord.FromXY(x, printMap.Bottom)] = '<';

                    Log(printMap.ToString(c => c.ToString()), -1);
                    Log($"Result: {x}", -1);
                    sum += x;
                }
            for (int y = 1; y <= map.Bottom; y++)
                if (MapHasHorizontalFold(y, map) == errors)
                {
                    var printMap = map.Clone();
                    printMap[Coord.FromXY(printMap.Left - 1, y - 1)] = 'v';
                    printMap[Coord.FromXY(printMap.Left, y)] = '^';
                    printMap[Coord.FromXY(printMap.Right + 1, y - 1)] = 'v';
                    printMap[Coord.FromXY(printMap.Right, y)] = '^';

                    Log(printMap.ToString(c => c.ToString()), -1);
                    Log($"Result: {y}*100", -1);

                    sum += y * 100;
                }

            return sum;
            // Log(map.ToString(c=>c.ToString()), -1);
            // throw new NotImplementedException();
        }

        private int MapHasVerticalFold(int col, SparseBuffer<char> map)
        {
            var leftCol = col - 1;
            var rightCol = col;
            var errors = 0;
            while (leftCol >= 0 && rightCol <= map.Right)
            {
                for (int row = 0; row <= map.Bottom; row++)
                    if (map[new Coord(row, leftCol)] != map[new Coord(row, rightCol)])
                        errors++;
                leftCol--;
                rightCol++;
            }
            return errors;
        }

        private int MapHasHorizontalFold(int row, SparseBuffer<char> map)
        {
            var topRow = row - 1;
            var bottomRow = row;
            var errors = 0;
            while (topRow >= 0 && bottomRow <= map.Bottom)
            {
                for (int col = 0; col <= map.Right; col++)
                    if (map[new Coord(topRow, col)] != map[new Coord(bottomRow, col)])
                        errors++;
                topRow--;
                bottomRow++;
            }
            return errors;
        }
    }
}