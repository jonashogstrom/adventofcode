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
        [TestCase(549, 4147, "Day24.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1;
            Part2Type part2;

            var sw = Stopwatch.StartNew();
            var directions = new List<List<HexDirection>>();
            foreach (var s in source)
            {
                var x = new List<HexDirection>();
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
                    x.Add((HexDirection)Enum.Parse(typeof(HexDirection), temp));
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
                    c = c.HexMove(d);
                }

                floor[c] = !floor[c];
            }

            part1 = floor.Count(true);
            LogAndReset("*1", sw);

            for (int i = 0; i < 100; i++)
            {
                var neighbors = new SparseBuffer<int>();

                foreach (var t in floor.Keys)
                {
                    if (floor[t])
                    {
                        foreach (var neighbor in Coord.HexNeighbors)
                            neighbors[t.HexMove(neighbor)]++;
                    }
                }

                var nextGen = new SparseBuffer<bool>();
                foreach (var t in neighbors.Keys)
                {
                    nextGen[t] = floor[t];
                    if (floor[t])
                    {
                        if (neighbors[t] == 0 || neighbors[t] > 2)
                            nextGen[t] = false;
                    }
                    else
                    {
                        if (neighbors[t] == 2)
                            nextGen[t] = true;
                    }
                }

                floor = nextGen;
                Log($"Day {i+1}: {floor.Count(true)}");
            }

            part2 = floor.Count(true);
            LogAndReset("*2", sw);
            return (part1, part2);
        }
    }


}

