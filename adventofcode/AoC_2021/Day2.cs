using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace AdventofCode.AoC_2021
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day2 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(150, 900, "Day2_test.txt")]
        [TestCase(2070300, 2078985210, "Day2.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1 = 0;
            Part2Type part2 = 0;
            var sw = Stopwatch.StartNew();

            Coord c = new Coord(0, 0);
            var depth = 0;
            var pos = 0;
            foreach (var s in source)
            {
                var p = s.Split(' ');
                var v = int.Parse(p[1]);
                switch (p[0][0])
                {
                    case 'f': pos += v;
                    break;
                    case 'd': 
                        depth += v;
                    break;
                    case 'u': 
                        depth -= v;
                    break;
                }
            }

            part1 = pos * depth;

            
            LogAndReset("Parse", sw);

            LogAndReset("*1", sw);

            depth = 0;
            pos = 0;
            var aim = 0;
            foreach (var s in source)
            {
                var p = s.Split(' ');
                var v = int.Parse(p[1]);
                switch (p[0][0])
                {
                    case 'f':
                        pos += v;
                        depth += aim*v;

                        break;
                    case 'd':
                        aim += v;
                        break;
                    case 'u':
                        aim -= v;
                        break;
                }
            }

            part2 = pos * depth;
            LogAndReset("*2", sw);
            return (part1, part2);
        }
    }
}