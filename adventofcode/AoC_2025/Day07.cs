using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AdventofCode.Utils;
using NUnit.Framework;
using NUnit.Framework.Internal.Commands;

namespace AdventofCode.AoC_2025
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    internal class Day07 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(21, 40, "<day>_test.txt")]
        [TestCase(1660, 305999729392659, "<day>.txt")]
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
            
            LogAndReset("Parse", sw);

            var start = map.Keys.Single(k => map[k] == 'S');
            var beams = new DicWithDefault<Coord, long>(0)
            {
                [start] = 1
            };
            var timelines = 1L;
            var splits = 0;
            while (beams.Any())
            {
                var nextBeams = new DicWithDefault<Coord, long>(0);
                foreach (var beam in beams.Keys)
                {
                    var pos = beam.Move(Coord.S);
                    var count =  beams[beam];
                    if (pos.Y > map.Bottom)
                        continue;
                    if (map[pos] == '^')
                    {
                        nextBeams[pos.Move(Coord.E)] += count;
                        nextBeams[pos.Move(Coord.W)] += count;
                        splits++;
                        timelines += count;
                    }
                    else nextBeams[pos] += count;
                }

                beams = nextBeams;

            }

            part1 = splits;
            part2 = timelines;

            LogAndReset("*12", sw);

            return (part1, part2);
        }
    }
}