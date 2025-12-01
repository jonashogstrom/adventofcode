using System;
using System.Diagnostics;
using NUnit.Framework;
using System.Collections.Generic;
using AdventofCode.Utils;
using System.Linq;


namespace AdventofCode.AoC_2024
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day245 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(3, null, "Day25_test.txt")]
        [TestCase(9999999, null, "Day25.txt")]
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

            var keys = new List<int[]>();
            var locks = new List<int[]>();
            // parse input here
            foreach (var schematic in source.AsGroups())
            {
                if (schematic[0] == "#####")
                {
                    var l = new List<int>();
                    // parse Lock;
                    for (int col = 0; col < 5; col++)
                    {
                        var r = 0;
                        while (schematic[r][col] == '#') r++;
                        l.Add(r - 1);
                    }

                    locks.Add(l.ToArray());
                }
                else if (schematic[6] == "#####")
                {
                    var k = new List<int>();
                    // parse Key;
                    for (int col = 0; col < 5; col++)
                    {
                        var r = 6;
                        while (schematic[r][col] == '#') r--;
                        k.Add(5 - r);
                    }

                    keys.Add(k.ToArray());
                }
                else
                    throw new Exception();
            }

            LogAndReset("Parse", sw);

            // solve part 1 here

            foreach (var key in keys)
            foreach (var l in locks)
            {
                var ok = true;
                for (int col = 0; col < 5; col++)
                    if (key[col] + l[col] > 5)
                        ok = false;
                if (ok)
                    part1++;
            }

            LogAndReset("*1", sw);

            // solve part 2 here

            LogAndReset("*2", sw);

            return (part1, part2);
        }
    }
}