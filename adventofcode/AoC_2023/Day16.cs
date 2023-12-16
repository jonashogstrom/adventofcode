using System;
using System.Diagnostics;
using NUnit.Framework;
using System.Collections.Generic;
using AdventofCode.Utils;
using System.Linq;
using System.Threading;
using ABI.Windows.ApplicationModel.Activation;
using Accord.Collections;


namespace AdventofCode.AoC_2023
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day16 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(46, 51, "Day16_test.txt")]
        [TestCase(8034, 8225, "Day16.txt")]
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

            LogAndReset("Parse", sw);

            part1 = RecStart(map, new Coord(0, 0), Coord.E);

            LogAndReset("*1", sw);

            foreach (var c in map.TopEdge)
            {
                part2 = Math.Max(part2, RecStart(map, c, Coord.S));
            }
            foreach (var c in map.BottomEdge)
            {
                part2 = Math.Max(part2, RecStart(map, c, Coord.N));
            }
            foreach (var c in map.LeftEdge)
            {
                part2 = Math.Max(part2, RecStart(map, c, Coord.E));
            }
            foreach (var c in map.RightEdge)
            {
                part2 = Math.Max(part2, RecStart(map, c, Coord.W));
            }


            // part2 = Math.Max(part2, RecStart(map, new Coord(r, map.Right), Coord.W));
            // }
            // foreach (var c in map.AllColIndices)
            // {
            //     part2 = Math.Max(part2, RecStart(map, new Coord(map.Top, c), Coord.S));
            //     part2 = Math.Max(part2, RecStart(map, new Coord(map.Bottom, c), Coord.N));
            // }

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private void PrintMap(SparseBuffer<char> map, Dictionary<Coord, HashSet<Coord>> memory)
        {
            Log(() => map.ToString((c, coord) =>
            {
                if (c == '.')
                {
                    if (memory.TryGetValue(coord, out var hashset))
                    {
                        if (hashset.Count == 1)
                            return Coord.trans2Arrow[hashset.First()].ToString();
                        return hashset.Count.ToString();
                    }
                }

                return c.ToString();
            }));
        }

        private long RecStart(SparseBuffer<char> map, Coord beam, Coord beamdir)
        {
            var memory = new Dictionary<Coord, HashSet<Coord>>();
            Rec(map, memory, beam, beamdir);
            PrintMap(map, memory);
            return memory.Keys.Count;
        }

        private void Rec(SparseBuffer<char> map, Dictionary<Coord, HashSet<Coord>> memory, Coord beam, Coord beamdir)
        {
            if (beam.Row < map.Top || beam.Row > map.Bottom || beam.Col < map.Left || beam.Col > map.Right)
            {
                return;
            }

            if (!memory.TryGetValue(beam, out var hashset))
            {
                hashset = new HashSet<Coord>();
                memory[beam] = hashset;
            }
            if (hashset.Contains(beamdir))
                return;
            hashset.Add(beamdir);

            var newDirs = new List<Coord>();
            switch (map[beam])
            {
                case '.':
                    newDirs.Add(beamdir);
                    break;
                case '|':
                    if (beamdir.Equals(Coord.E) || beamdir.Equals(Coord.W))
                    {
                        newDirs.Add(Coord.N);
                        newDirs.Add(Coord.S);
                    }
                    else
                    {
                        newDirs.Add(beamdir);
                    }

                    break;

                case '-':
                    if (beamdir.Equals(Coord.S) || beamdir.Equals(Coord.N))
                    {
                        newDirs.Add(Coord.E);
                        newDirs.Add(Coord.W);
                    }
                    else
                    {
                        newDirs.Add(beamdir);
                    }

                    break;
                case '/':
                    if (beamdir.Equals(Coord.S))
                        newDirs.Add(Coord.W);
                    else if (beamdir.Equals(Coord.W))
                        newDirs.Add(Coord.S);
                    else if (beamdir.Equals(Coord.E))
                        newDirs.Add(Coord.N);
                    else if (beamdir.Equals(Coord.N))
                        newDirs.Add(Coord.E);
                    break;
                case '\\':
                    if (beamdir.Equals(Coord.S))
                        newDirs.Add(Coord.E);
                    else if (beamdir.Equals(Coord.W))
                        newDirs.Add(Coord.N);
                    else if (beamdir.Equals(Coord.E))
                        newDirs.Add(Coord.S);
                    else if (beamdir.Equals(Coord.N))
                        newDirs.Add(Coord.W);
                    break;
            }
            foreach (var dir in newDirs)
                Rec(map, memory, beam.Move(dir), dir);
        }
    }
}