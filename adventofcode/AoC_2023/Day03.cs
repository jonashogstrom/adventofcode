using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Security.Cryptography.Certificates;
using Accord;
using AdventofCode.Utils;
using NUnit.Framework;

// not 552835
// not 88373022
namespace AdventofCode.AoC_2023
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day03 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(4361, 467835, "Day03_test.txt")]
        [TestCase(556367, 89471771, "Day03.txt")]
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
            // parse input here

            LogAndReset("Parse", sw);

            var partSum = 0;
            var numbers = new Dictionary<Coord, int>();
            var partnumids = new Dictionary<Coord, int>();
            var partnumid = 0;
            for (int y = map.Top; y <= map.Bottom+1; y++)
            {
                var num = "";
                var numCoords = new List<Coord>();
                for (int x = map.Left; x <= map.Right+1; x++)
                {
                    var c = Coord.FromXY(x, y);
                    var ch = map[c];
                    if (Char.IsDigit(ch))
                    {
                        num += ch;
                        numCoords.Add(c);
                    }
                    else if (numCoords.Count > 0)
                    {
                        var partNum = int.Parse(num);
                        var adjacent = false;
                        foreach (var temp in numCoords)
                        {
                            foreach (var neigh in temp.GenAdjacent8())
                            {
                                var neighchar = map[neigh];
                                adjacent = adjacent || (neighchar != '.' && !char.IsDigit(neighchar));
                            }
                        }

                        if (adjacent)
                        {
                            partSum += partNum;
                        }

                        foreach (var temp in numCoords)
                        {
                            numbers[temp] = partNum;
                            partnumids[temp] = partnumid;
                        }

                        partnumid++;
                        num = "";
                        numCoords.Clear();
                    }

                }
            }


            part1 = partSum;
            LogAndReset("*1", sw);

            var ratioSum = 0;
            foreach (var c in map.Keys)
            {
                if (map[c] == '*')
                {
                    var allNeighNums = new List<int>();
                    var allPartNumIds = new HashSet<int>();
                    if (c.Row == 98)
                    {
                        Log("Row: 98");
                    }
                    foreach (var neigh in c.GenAdjacent8())
                    {
                        if (partnumids.TryGetValue(neigh, out var foundpartnumid) &&
                            !allPartNumIds.Contains(foundpartnumid))
                        {
                            allNeighNums.Add(numbers[neigh]);
                            allPartNumIds.Add(foundpartnumid);
                        }
                    }

                    if (allNeighNums.Count == 2)
                    {
                        var ratio = allNeighNums[0] * allNeighNums[1];
                        ratioSum += ratio;
                        Log($"Gear row {c.Y} col: {c.X} partsNums: {allNeighNums[0]}*{allNeighNums[1]}");
                        map[c] = 'A';
                    }
                    else
                    {
                        map[c] = 'B';

                    }
                }
            }
            Log(map.ToString(c=>c.ToString()));
            part2 = ratioSum;

            LogAndReset("*2", sw);

            return (part1, part2);
        }
    }
}