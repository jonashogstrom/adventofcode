using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2023
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day09 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(114, 2, "Day09_test.txt")]
        [TestCase(1708206096, null, "Day09.txt")]
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

            foreach (var s in source)
            {
                part1 += Solve(s.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(x=>int.Parse(x)).ToList());
            }
            // parse input here

            LogAndReset("Parse", sw);

            // solve part 1 here

            LogAndReset("*1", sw);

               foreach (var s in source)
            {
             part2 += Solve2(s.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(x=>int.Parse(x)).ToList());
             }
             

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private long Solve(List<int> data)
        {
            var rows = new List<List<int>> { data };
            while (rows.Last().Any(x => x != 0))
            {
                var newLine = new List<int>();
                foreach(var p in rows.Last().AsPairs())
                    newLine.Add(p.Item2-p.Item1);
                rows.Add(newLine);
            }
            rows.Last().Add(0);

            for (int i = rows.Count - 2; i >= 0; i--)
            {
                rows[i].Add(rows[i].Last() + rows[i+1].Last());
            }

            return rows.First().Last();

        }

        private long Solve2(List<int> data)
        {
            var rows = new List<List<int>> { data };
            while (rows.Last().Any(x => x != 0))
            {
                var newLine = new List<int>();
                foreach (var p in rows.Last().AsPairs())
                    newLine.Add(p.Item2 - p.Item1);
                rows.Add(newLine);
            }
            rows.Last().Add(0);

            for (int i = rows.Count - 2; i >= 0; i--)
            {
                rows[i].Insert(0,rows[i].First() - rows[i + 1].First());
            }

            return rows.First().First();

        }
    }
}