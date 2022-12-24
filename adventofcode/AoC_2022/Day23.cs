using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2022
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day23 : TestBaseClass<Part1Type, Part2Type>
    {
        private readonly List<List<Coord>> _moves = new()
        {
            new() { Coord.N, Coord.NE, Coord.NW },
            new() { Coord.S, Coord.SE, Coord.SW },
            new() { Coord.W, Coord.NW, Coord.SW },
            new() { Coord.E, Coord.NE, Coord.SE }
        };

        public bool Debug { get; set; }

        [Test]
        [TestCase(110, 20, "Day23_test.txt")]
        [TestCase(null, 4, "Day23_testsmall.txt")]
        [TestCase(4181, 973, "Day23.txt")]
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

            // parse input here
            var initialMap = source.ToSparseBuffer('.');
            initialMap.RemoveDefaults();
            LogAndReset("Parse", sw);
            var elves = initialMap.Keys.ToList();

            var elfMoved = true;
            var round = 0;
            while (elfMoved)
            {
                var res = MoveElves(elves, round);
                elves = res.Item1;
                elfMoved = res.Item2;

                if (LogLevel > 0)
                {
                    var map = ElvesToMap(elves);
                    Log(() => $"== End of round {round + 1} ==");
                    Log(() => map.ToString(c => c.ToString()));
                }

                if (round + 1 == 10)
                {
                    var map = ElvesToMap(elves);
                    part1 = map.Width * map.Height - map.Keys.Count();
                }

                round++;
            }
            var finalMap = ElvesToMap(elves);

            part2 = round;
            LogAndReset("*1", sw);

            // solve part 2 here

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private static SparseBuffer<char> ElvesToMap(List<Coord> elves)
        {
            var map = new SparseBuffer<char>('.', elves.First());
            foreach (var e in elves)
                map[e] = '#';
            return map;
        }


        private (List<Coord>, bool) MoveElves(List<Coord> elves, int round)
        {
            var elvesSet = new HashSet<Coord>(elves);

            var proposed = new Dictionary<Coord, Coord>();
            var taken = new DicWithDefault<Coord, int>();
            foreach (var elf in elves)
            {
                // check if the elf has any neighbors
                if (elf.GenAdjacent8().Any(c => elvesSet.Contains(c)))
                {
                    for (int i = 0; i < 4; i++)
                    {
                        var adjacent = _moves[(round + i) % 4];
                        if (adjacent.All(c => !elvesSet.Contains(elf.Move(c))))
                        {
                            var newPos = elf.Move(adjacent.First());
                            proposed[elf] = newPos;
                            taken[newPos]++;
                            break;
                        }
                    }
                }
            }

            var newElves = new List<Coord>();

            var elfMoved = false;
            foreach (var e in elves)
            {
                if (proposed.TryGetValue(e, out var newPos) && taken[newPos] == 1)
                {
                    newElves.Add(newPos);
                    elfMoved = true;
                }
                else
                {
                    newElves.Add(e);
                }
            }
            return (newElves, elfMoved);
        }
    }
}