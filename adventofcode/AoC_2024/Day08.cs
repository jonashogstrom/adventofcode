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
    class Day08 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(14, 34, "Day08_test.txt")]
        [TestCase(291, 1015, "Day08.txt")]
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

            var map = source.ToSparseBuffer(' ');

            LogAndReset("Parse", sw);

            var groups = map.Keys.GroupBy(k => map[k]).ToList();
            var antinodes = new HashSet<Coord>();
            foreach (var g in groups)
            {
                if (g.Key == '.')
                    continue;
                foreach (var pair in g.ToList().AsCombinations())
                {
                    var diff = pair.Item1.Subtract(pair.Item2);
                    var a1 = pair.Item1.Move(diff);
                    var a2 = pair.Item2.Move(diff.RotateCWDegrees(180));
                    if (map.InsideBounds(a1))
                    {
                        if (map[a1] == '.') 
                            map[a1] = '#';
                        else
                            map[a1] = '@';

                        antinodes.Add(a1);
                    }

                    if (map.InsideBounds(a2))
                    {
                        if (map[a2] == '.') 
                            map[a2] = '#';
                        else
                            map[a2] = '@';
                        antinodes.Add(a2);
                    }
                }
                Log(map.ToString);
            }

            
            part1 = antinodes.Count();

            LogAndReset("*1", sw);

            var antinodes2 = new HashSet<Coord>();
            foreach (var g in groups)
            {
                if (g.Key == '.')
                    continue;
                foreach (var pair in g.ToList().AsCombinations())
                {
                    var diff = pair.Item1.Subtract(pair.Item2);
                    // find common factors
                    var gcd = GCD(Math.Abs(diff.Col), Math.Abs(diff.Row));
                    var diff2 = new Coord(diff.Row / gcd, diff.Col / gcd);
                    var diff2Inv = diff2.RotateCWDegrees(180);
                    var p = pair.Item1;
                    while (map.InsideBounds(p))
                    {
                        antinodes2.Add(p);
                        p = p.Move(diff2);
                    }

                    p = pair.Item1;
                    while (map.InsideBounds(p))
                    {
                        antinodes2.Add(p);
                        p = p.Move(diff2Inv);
                    }
                }
            }

            part2 = antinodes2.Count();

            LogAndReset("*2", sw);

            return (part1, part2);
        }
        
        static int GCD(int num1, int num2)
        {
            int Remainder;
 
            while (num2 != 0)
            {
                Remainder = num1 % num2;
                num1 = num2;
                num2 = Remainder;
            }
 
            return num1;
        }
    }
}

