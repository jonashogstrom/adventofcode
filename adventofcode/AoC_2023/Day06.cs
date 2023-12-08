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
    class Day06 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(288, 71503, "Day06_test.txt")]
        [TestCase(2756160, 34788142, "Day06.txt")]
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

            LogAndReset("Parse", sw);

            part1 = Solve(source[0], source[1]);
            LogAndReset("*1", sw);

            part2 = Solve(
                source[0].Replace(" ", ""),
                source[1].Replace(" ", ""));

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private long Solve(string timeLine, string distLine)
        {
            var times = SplitNode.Create(timeLine, new[] { ' ', ':' }).Children.Skip(1).ToList();
            var dists = SplitNode.Create(distLine, new[] { ' ', ':' }).Children.Skip(1).ToList();

            var res = 1L;
            for (int i = 0; i < times.Count; i++)
            {
                var solutions = GetSolutions(times[i].i, dists[i].l);
                Log($"Time: {times[i].i}, dist: {dists[i].l} => {solutions} solutions");
                res *= solutions;

            }
            return res;
        }

        private long GetSolutions(long time, long dist)
        {
            // solve a second degree equation where f(x) = -i^2 + time*i - dist
            var a = -1;
            var b = time;
            var c = -dist;
            var x1 = (-b + Math.Sqrt(b * b - 4 * a * c)) / (2 * a);
            var x2 = (-b - Math.Sqrt(b * b - 4 * a * c)) / (2 * a);

            x1 = Math.Ceiling(x1);
            x2 = Math.Floor(x2);
            var solutions = x2 -x1 + 1;
            // check perfect roots
            if (-x1 * x1 + time * x1 - dist == 0)
                solutions--;
            if (-x2 * x2 + time * x2 - dist == 0)
                solutions--;
            Log($"Time: {time}, dist: {dist} => {solutions} solutions");


            return (long)solutions;
        }
        private long GetSolutions_oldandslow(long time, long dist)
        {
            var solutions = 0L;
            for (int i = 1; i < time; i++)
            {
            
                // solve a second degree equation where f(x) = -i^2 + time*i - dist
                if ((time-i)*i > dist)
                    solutions++;
            }

            return solutions;
        }
    }
}