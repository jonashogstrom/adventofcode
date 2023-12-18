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
        [TestCase(50746, null, "Day18.txt")]
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

            var corners = new List<Coord> { Coord.Origin };
            var corners2 = new List<Coord> { Coord.Origin };
            var perimeter = 0;
            var permieter2 = 0L;
            foreach (var x in source)
            {
                var parts = x.Split(new[] { ' ', '(', ')', '#' }, StringSplitOptions.RemoveEmptyEntries);
                var len = int.Parse(parts[1]);
                var dir = Coord.trans2Coord[parts[0][0]];
                corners.Add(corners.Last().Move(dir, len));
                perimeter += len;

                var len2 = Convert.ToInt32("0x" + parts[2].Substring(0, 5), 16);
                var dir2 = Coord.E.RotateCWDegrees(90 * int.Parse(parts[2].Substring(5)));
                permieter2 += len2;
                corners2.Add(corners2.Last().Move(dir2, len2));
            }
            LogAndReset("Parse", sw);

            // for Manhattan maps, each perimeter square will give a half square extra
            // inner corners will give a negative quarter square and outer corners will give a positive quarter square
            // there are four more outer corners, so the sum of all corners will be 4*1/4 square = 1 square
            part1 = CalcPolySize(corners) + perimeter / 2 + 1;

            LogAndReset("*1", sw);


            part2 = CalcPolySize(corners2) + permieter2 / 2 + 1;
            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private static long CalcPolySize(List<Coord> corners)
        {
            long part2;
            var sum = 0L;
            // A = 0.5 * |(x1*y2 - x2*y1) + (x2*y3 - x3*y2) + ... + (xn*y1 - x1*yn)| 
            for (var i = 0; i < corners.Count; i++)
            {
                var p1 = corners[i];
                var p2 = corners[(i + 1) % corners.Count];
                long p1x = p1.X;
                long p1y = p1.Y;
                long p2x = p2.X;
                long p2y = p2.Y;
                sum += p1x * p2y - p2x * p1y;
            }

            part2 = Math.Abs(sum) / 2;
            return part2;
        }
    }
}