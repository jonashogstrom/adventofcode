using System;
using System.Diagnostics;
using System.Windows.Forms;
using NUnit.Framework;

namespace AdventofCode.AoC_2020
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day12 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(25, 286, "Day12_test.txt")]
        [TestCase(1457, 106860, "Day12.txt")]
        public void Test1(Part1Type exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            var sw = Stopwatch.StartNew();

            LogAndReset("Parse", sw);

            var part1 = CalculatePart(source, Coord.E, NSEWMove.boat, false);
            LogAndReset("*1", sw);

            var part2 = CalculatePart(source, Coord.Origin.Move(Coord.E, 10).Move(Coord.N, 1), NSEWMove.target, true);
            LogAndReset("*2", sw);
            return (part1, part2);
        }


        private int CalculatePart(string[] source, Coord target, NSEWMove nsewMove, bool igorsequence)
        {
            var pos = Coord.Origin;

            foreach (var s in source)
            {
                var value = int.Parse(s.Substring(1));
                var instr = s[0];

                switch (instr)
                {
                    case 'L':
                        target = target.RotateCCWDegrees(value);
                        break;
                    case 'R':
                        target = target.RotateCWDegrees(value);
                        break;
                    case 'F':
                        pos = pos.Move(target, value);
                        break;
                    case 'N':
                    case 'S':
                    case 'W':
                    case 'E':
                        var dir = Coord.trans2Coord[instr];
                        if (nsewMove == NSEWMove.boat)
                            pos = pos.Move(dir, value);
                        else
                            target = target.Move(dir, value);
                        break;
                }
                if (igorsequence)
                    Log($"[{pos.X}, {pos.Y-40000}, 100000, 20, 0],");
            }

            return pos.Dist(Coord.Origin);
        }
    }

    internal enum NSEWMove
    {
        boat, target
    }
}