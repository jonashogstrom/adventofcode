using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AdventofCode.AoC_2018;
using AdventofCode.Utils;
using NUnit.Framework;

// not 139
namespace AdventofCode.AoC_2020
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day11 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(37, 26, "Day11_test.txt")]
        [TestCase(2316, 2128, "Day11.txt")]
        public void Test1(Part1Type exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            var sw = Stopwatch.StartNew();

            var floor = source.ToSparseBuffer('.');

            LogAndReset("Parse", sw);
            var part1 = CalculatePart1(floor);

            LogAndReset("*1", sw);
            var part2 = CalculatePart2(floor);

            LogAndReset("*2", sw);
            return (part1, part2);
        }

        private long CalculatePart2(SparseBuffer<char> floor)
        {
            long part1;
            var changed = true;
            var gen = 0;
            while (changed)
            {
                gen++;
                changed = false;
                var floor2 = new SparseBuffer<char>('.');
                foreach (var k in floor.Keys)
                {
                    var neighbourcount = 0;
                    var remainingToTest = 8;
                    var c1 = floor[k];
                    foreach (var dir in Coord.Directions8)
                    {
                        var p = k.Move(dir);
                        while (floor.Keys.Contains(p))
                        {
                            var c = floor[p];
                            if (c == '#')
                            {
                                neighbourcount++;
                                break;
                            }

                            if (c == 'L')
                            {
                                break;
                            }

                            p = p.Move(dir);
                        }

                        remainingToTest--;

                        if (neighbourcount >= 5)
                            break;
                        if (neighbourcount > 0 && c1 == 'L')
                            break;
                        if (neighbourcount + remainingToTest < 5 && c1 == '#')
                            break;
                    }

                    if (c1 == 'L' && neighbourcount == 0)
                    {
                        floor2[k] = '#';
                        changed = true;
                    }
                    else if (c1 == '#' && neighbourcount >= 5)
                    {
                        floor2[k] = 'L';
                        changed = true;
                    }
                    else
                    {
                        floor2[k] = c1;
                    }

                }
                var occupiedSeats = floor2.Keys.Count(c => floor2[c] == '#');
                
                 Log($"--------------------------------------");
                 Log($"Gen: {gen}, occupiedSeats: {occupiedSeats}");
                 Log(floor2.ToString(c=>c.ToString()));
                floor = floor2;
            }

            Log($"Part 2, total generations: {gen} Coords created: {Coord.CoordCounter}");

            part1 = floor.Keys.Count(c => floor[c] == '#');
            return part1;
        }

        private static long CalculatePart1(SparseBuffer<char> floor)
        {
            long part1;
            var changed = true;
            while (changed)
            {
                changed = false;
                var floor2 = new SparseBuffer<char>('.');
                foreach (var k in floor.Keys)
                {
                    var neighbourcount = k.GenAdjacent8().Count(n => floor[n] == '#');
                    if (floor[k] == 'L' && neighbourcount == 0)
                    {
                        floor2[k] = '#';
                        changed = true;
                    }
                    else if (floor[k] == '#' && neighbourcount >= 4)
                    {
                        floor2[k] = 'L';
                        changed = true;
                    }
                    else
                    {
                        floor2[k] = floor[k];
                    }
                }

                floor = floor2;
            }

            part1 = floor.Keys.Count(c => floor[c] == '#');
            return part1;
        }
    }
}
