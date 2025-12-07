using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AdventofCode.Utils;
using NUnit.Framework;
using NUnit.Framework.Internal.Commands;

namespace AdventofCode.AoC_2025
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    internal class Day07 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(21, 40, "<day>_test.txt")]
        [TestCase(1660, null, "<day>.txt")]
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
            var beams = new HashSet<Coord>() { start };
            while (beams.Any())
            {
                var nextBeams = new HashSet<Coord>();
                foreach (var beam in beams)
                {
                    var pos = beam.Move(Coord.S);
                    if (pos.Y > map.Bottom)
                        continue;
                    if (map[pos] == '^')
                    {
                        nextBeams.Add(pos.Move(Coord.E));
                        nextBeams.Add(pos.Move(Coord.W));
                        part1++;
                    }
                    else nextBeams.Add(pos);
                }

                beams = nextBeams;

            }
            // solve part 1 here

            LogAndReset("*1", sw);

            // solve part 2 here

            LogAndReset("*2", sw);

            return (part1, part2);
        }
    }
}