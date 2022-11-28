using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Accord;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2021
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day25 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(58, null, "Day25_test.txt")]
        [TestCase(560, null, "Day25.txt")]
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
            var floor = source.ToSparseBuffer('.');
            Log("Initial State:");
            Log(floor.ToString(c=>c.ToString()));
            LogAndReset("Parse", sw);
            part1 = MoveCucumbers(floor);
            LogAndReset("*1", sw);
            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private long MoveCucumbers(SparseBuffer<char> floor)
        {
            var width = floor.Width;
            var height = floor.Height;
            var iterations = 0;
            while (true)
            {
                var s1 = TryMove(floor, '>', width, height);
                var s2 = TryMove(s1.Item1, 'v', width, height);
                iterations++;
                if (!s1.Item2 && !s2.Item2)
                    return iterations;
                floor = s2.Item1;
                Log($"After {iterations} steps");
                Log(floor.ToString(c => c.ToString()));
            }
        }


        private (SparseBuffer<char>, bool) TryMove(SparseBuffer<char> floor, char mover, int width, int height)
        {
            var direction = Coord.trans2Coord[mover];
            var moved = false;
            var newFloor = new SparseBuffer<char>('.');
            foreach (var pos in floor.Keys)
            {
                var cucumber = floor[pos];
                if (cucumber == mover)
                {
                    var newPos = Coord.FromXY(
                        (pos.X + direction.X) % width,
                        (pos.Y + direction.Y) % height);
                    if (floor[newPos] == '.')
                    {
                        newFloor[newPos] = mover;
                        moved = true;
                    }
                    else
                    {
                        newFloor[pos] = mover;
                    }
                }
                else if (cucumber != '.')
                {
                    newFloor[pos] = cucumber;
                }
            }

            return (newFloor, moved);
        }
    }
}