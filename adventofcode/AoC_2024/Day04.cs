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
    class Day04 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(18, 9, "Day04_test.txt")]
        [TestCase(2639, null, "Day04.txt")]
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

            var buff = source.ToSparseBuffer();

            LogAndReset("Parse", sw);

            foreach (var k in buff.Keys.Where(x => buff[x] == 'X'))
            {
                foreach (var dir in Coord.Directions8)
                {
                    if (Find(buff, k, dir, "XMAS"))
                        part1++;
                }
            }

            LogAndReset("*1", sw);

            foreach (var k in buff.Keys.Where(x => buff[x] == 'A'))
            {
                if ((Find(buff, k.Move(Coord.SE), Coord.NW, "MAS") || 
                    Find(buff, k.Move(Coord.NW), Coord.SE, "MAS"))
                    && 
                    (Find(buff, k.Move(Coord.SW), Coord.NE, "MAS") || 
                     Find(buff, k.Move(Coord.NE), Coord.SW, "MAS")))
                    part2++;
            }

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private bool Find(SparseBuffer<char> buff, Coord start, Coord dir,  string str)
        {
            if (str == "")
                return true;
            if (buff[start] != str[0])
                return false;
            return Find(buff, start.Move(dir), dir, str[1..]);
        }
    }
}