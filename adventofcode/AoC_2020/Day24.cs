using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace AdventofCode.AoC_2020
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day24 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(10, 2208, "Day24_test.txt")]
        [TestCase(549, null, "Day24.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1 = 0;
            Part2Type part2 = 0;

            var sw = Stopwatch.StartNew();
            var directions = new List<List<hexdirection>>();
            foreach (var s in source)
            {
                var x = new List<hexdirection>();
                var p = 0;
                while (p < s.Length)
                {
                    var temp = s[p].ToString();
                    if (temp == "n" || temp == "s")
                    {
                        p++;
                        temp += s[p];
                    }

                    p++;
                    x.Add((hexdirection)Enum.Parse(typeof(hexdirection), temp));
                }

                directions.Add(x);
            }

            LogAndReset("Parse", sw);
            var floor = new SparseBuffer<bool>();
            foreach (var list in directions)
            {
                var c = Coord.Origin;
                foreach (var d in list)
                {
                    c = HexMove(c, d);
                }

                floor[c] = !floor[c];
            }

            part1 = floor.Count(true);
            LogAndReset("*1", sw);

            for (int i = 0; i < 100; i++)
            {
                var neighbours = new SparseBuffer<int>();
                foreach (var t in floor.Keys)
                {
                    neighbours[t] = 0;
                }

                foreach (var t in floor.Keys)
                {
                    if (floor[t])
                    {
                        foreach (var n in Coord.HexNeighbours)
                            neighbours[HexMove(t, n)]++;
                    }
                }

                var floor2 = new SparseBuffer<bool>();
                foreach (var t in neighbours.Keys)
                {
                    floor2[t] = floor[t];
                    if (floor[t])
                    {
                        if (neighbours[t] == 0 || neighbours[t] > 2)
                            floor2[t] = false;
                    }
                    else
                    {
                        if (neighbours[t] == 2)
                            floor2[t] = true;
                    }
                }

                floor = floor2;
                Log($"Day {i+1}: {floor.Count(true)}");
            }

            part2 = floor.Count(true);
            LogAndReset("*2", sw);
            return (part1, part2);
        }

        private Coord HexMove(Coord coord, hexdirection hexdirection)
        {
            if (hexdirection == hexdirection.e)
                return coord.Move(Coord.E);
            if (hexdirection == hexdirection.w)
                return coord.Move(Coord.W);

            if (coord.Y % 2 == 0)
            {
                switch (hexdirection)
                {
                    case hexdirection.nw:
                        return coord.Move(Coord.N);
                    case hexdirection.ne:
                        return coord.Move(Coord.NE);
                    case hexdirection.sw:
                        return coord.Move(Coord.S);
                    case hexdirection.se:
                        return coord.Move(Coord.SE);
                    default:
                        throw new Exception();
                }

            }
            else
            {
                switch (hexdirection)
                {
                    case hexdirection.nw:
                        return coord.Move(Coord.NW);
                    case hexdirection.ne:
                        return coord.Move(Coord.N);
                    case hexdirection.sw:
                        return coord.Move(Coord.SW);
                    case hexdirection.se:
                        return coord.Move(Coord.S);
                    default:
                        throw new Exception();
                }
            }
        }
    }


}

