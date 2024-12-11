using System;
using System.Diagnostics;
using NUnit.Framework;
using System.Collections.Generic;
using AdventofCode.Utils;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace AdventofCode.AoC_2024
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day11 : TestBaseClass<Part1Type, Part2Type>
    {
        private int _steps;
        public bool Debug { get; set; }

        [Test]
        [TestCase(22, null, "Day11_test.txt", 6)]
        [TestCase(55312, null, "Day11_test.txt", 25)]
        [TestCase(19778, null, "Day11_test0.txt", 25)]
        [TestCase(203609, 240954878211138, "Day11.txt", 25)]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName, int steps)
        {
            _steps = steps;
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

            var gen0 = source.First().Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToList();
            
            LogAndReset("Parse", sw);
            var p2_gen0 = new DicWithDefault<long, long>();
            foreach (var s in gen0)
                p2_gen0[s]++;
            var p2_list = p2_gen0;
            var stones = -1L;
            for (var i = 0; i < 75; ++i)
            {
                p2_list = Evolve2(p2_list);
                stones = p2_list.Keys.Sum(k => p2_list[k]);
                if (i == (_steps - 1))
                {
                    part1 = stones;
                    LogAndReset("*1", sw);
                }
            }

            part2 = stones;

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private DicWithDefault<long, long> Evolve2(DicWithDefault<long, long> p2List)
        {
            var res = new DicWithDefault<long, long>();
            foreach (var s in p2List.Keys)
            {
                var c = p2List[s];
                if (s == 0)
                {
                    res[1] += c;
                }
                else if (s.ToString().Length % 2 == 0)
                {
                    var str = s.ToString();

                    res[long.Parse(str[..(str.Length / 2)])] += c;
                    res[long.Parse(str[(str.Length / 2)..])] += c;
                }
                else
                {
                    res[s * 2024] += c;
                }
            }

            return res;
        }

        // algo that works for *1, but not *2
        private List<long> Evolve(List<long> list)
        {
            var res = new List<long>();
            foreach (var s in list)
            {
                if (s == 0)
                {
                    res.Add(1);
                }
                else if (s.ToString().Length % 2 == 0)
                {
                    var str = s.ToString();
                    res.Add(long.Parse(str[..(str.Length / 2)]));
                    res.Add(long.Parse(str[(str.Length / 2)..]));
                }
                else
                {
                    res.Add(s * 2024);
                }
            }
            return res;
        }
    }
}