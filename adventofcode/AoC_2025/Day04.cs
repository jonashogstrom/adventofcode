using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Accord;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2025
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day04 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(13, 43, "<day>_test.txt")]
        [TestCase(1578, 10132, "<day>.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            LogLevel = resourceName.Contains("test") ? 20 : -1;
            var source = GetResource2(ref resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1 = 0;
            Part2Type part2 = 0;
            var sw = Stopwatch.StartNew();

            var map = source.ToSparseBuffer('.');
            var map2 = source.ToSparseBuffer();
            // parse input here

            LogAndReset("Parse", sw);

            var rolls = new HashSet<Coord>(map.Keys);
            
            var gen = 0;
            var ttq = false;
            while (!ttq)
            {
                 ttq = true;
                foreach (var k in map.Keys)
                {
                    if (map[k] == '@')
                    {
                        var neighbours = k.GenAdjacent8();
                        if (neighbours.Where(n => map[n] == '@').Count() < 4)
                        {
                            if (gen == 0)
                                part1++;
                            part2++;
                            map2[k] = '.';
                            ttq = false;
                        }
                    }
                }

                gen++;
                map = map2;
            }

            Log(()=>map2.ToString());
            // solve part 1 here

            LogAndReset("*1", sw);

            // solve part 2 here

            LogAndReset("*2", sw);

            return (part1, part2);
        }
    }
}