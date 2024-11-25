using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2023
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day08 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(2, null, "Day08_test.txt")]
        [TestCase(6, null, "Day08_test2.txt")]
        [TestCase(null, 6, "Day08_test3.txt")]
        [TestCase(19099, 17099847107071, "Day08.txt")]
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

            var map = new Dictionary<string, (string r, string l)>();
            var groups = source.AsGroups();
            var instructions = groups.First().First();
            foreach (var s in groups.Last())
            {// HVA = (NMF, CTG)
                var x = s.Replace(" ", "").Replace("(", "").Replace(")", "");
                var n = new SplitNode(x, '=', ',');
                map[n.First.Trimmed] = (n.Last.Last.Trimmed, n.Last.First.Trimmed);
            }
            // parse input here

            LogAndReset("Parse", sw);
            var pos = "AAA";

            if (map.ContainsKey(pos))
            {
                var moves = 0;
                while (pos != "ZZZ")
                {
                    pos = MakeMove(map, pos, instructions, moves);
                    moves++;
                }

                part1 = moves;
            }

            LogAndReset("*1", sw);

            var startPositions = map.Keys.Where(x => x.EndsWith('A')).ToList();
            var moves2 = 0;
            var positions = new List<int>(startPositions.Select(x=>-1));
            var cyclesFound = 0;
            while (cyclesFound < startPositions.Count)
            {
                var newPositions = new List<string>();
                moves2++;

                for (var i = 0 ; i<startPositions.Count; i++)
                {
                    var newPos = MakeMove(map, startPositions[i], instructions, moves2-1);
                    newPositions.Add(newPos);
                    if (newPos.EndsWith('Z'))
                    {
                        if (positions[i] == -1)
                        {
                            positions[i] = moves2;
                            cyclesFound++;
                        }
                    }
                }
                startPositions = newPositions;
            }

            var sharedPrimeFactors = GetPrimeFactors(positions.First()).ToHashSet();
            foreach (var x in positions.Skip(1))
            {
                sharedPrimeFactors.IntersectWith(GetPrimeFactors(x));
            }

            var sharedFactor = 1;
            foreach(var factor in sharedPrimeFactors)
                sharedFactor *= factor;

            var p2 = 1L;
            foreach (var s in positions)
                p2 *= s / sharedFactor;

            p2 *= sharedFactor;
            part2 = p2;

            // not 24085386913221873651759779 multiply all periods
            // not 63568204859 - multiply all periods divided by the shared factor (269)

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private List<int> GetPrimeFactors(int value)
        {
            var x = 2;
            var res = new List<int>();

            while (value >= x * x)
            {
                if (value % x == 0)
                {
                    res.Add(x);
                    value /= x;
                }
                else
                {
                    if ((x&1) == 1)
                        x+= 2;
                    else
                    {
                        x += 1;
                    }
                }
            }

            res.Add(value);

            return res;
        }

        private static string MakeMove(Dictionary<string, (string r, string l)> map, string pos, string instructions, int moves)
        {
            var options = map[pos];
            if (instructions[moves % instructions.Length] == 'R')
                pos = options.r;
            else
            {
                pos = options.l;
            }

            return pos;
        }
    }
}