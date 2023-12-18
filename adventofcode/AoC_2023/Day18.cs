using System;
using System.Diagnostics;
using NUnit.Framework;
using System.Collections.Generic;
using AdventofCode.Utils;
using System.Linq;
using System.Runtime.InteropServices;
using Windows.System.Update;
using Windows.UI;


namespace AdventofCode.AoC_2023
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day18 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(62, 952408144115, "Day18_test.txt")]
        [TestCase(50746, 70086216556038, "Day18.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            LogLevel = resourceName.Contains("test") ? 20 : -1;
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Part2Type part2;
            var sw = Stopwatch.StartNew();

            var corners = new List<Coord> { Coord.Origin };
            var corners2 = new List<Coord> { Coord.Origin };

            foreach (var x in source)
            {
                var parts = x.Split(new[] { ' ', '(', ')', '#' }, StringSplitOptions.RemoveEmptyEntries);

                var len = int.Parse(parts[1]);
                var dir = Coord.trans2Coord[parts[0][0]];
                corners.Add(corners.Last().Move(dir, len));

                var len2 = Convert.ToInt32("0x" + parts[2].Substring(0, 5), 16);
                var dir2 = Coord.E.RotateCWDegrees(90 * int.Parse(parts[2].Substring(5)));
                corners2.Add(corners2.Last().Move(dir2, len2));
            }
            LogAndReset("Parse", sw);
            var part1 = corners.CalcManhattanSize();
            LogAndReset("*1", sw);
            part2 = corners2.CalcManhattanSize();
            LogAndReset("*2", sw);

            return (part1, part2);
        }
    }
}