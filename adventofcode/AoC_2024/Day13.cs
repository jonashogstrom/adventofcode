using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using AdventofCode.Utils;
using System.Text.RegularExpressions;


namespace AdventofCode.AoC_2024
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day13 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(480, null, "Day13_test.txt")]
        [TestCase(29438, null, "Day13.txt")]
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

            /*
            Button A: X+94, Y+34
            Button B: X+22, Y+67
            Prize: X=8400, Y=5400
             */

            // regex for finding two positive integers somewhere in the string
            var rexp = new Regex("([\\d]+)");
            var machines = new List<Machine>();
            foreach (var g in source.AsGroups())
            {
                var a = rexp.Matches(g[0]);
                var b = rexp.Matches(g[1]);
                var prize = rexp.Matches(g[2]);
                var machine = new Machine(s=>Log(s, LogLevel),
                    int.Parse(a[0].Value),
                    int.Parse(a[1].Value),
                    int.Parse(b[0].Value),
                    int.Parse(b[1].Value),
                    int.Parse(prize[0].Value),
                    int.Parse(prize[1].Value));
                machines.Add(machine);
            }

            LogAndReset("Parse", sw);
            

            foreach (var machine in machines)
            {
                var res = machine.FindCostOfBestSolution();
                if (res.HasValue)
                    part1 += res.Value;
            }

            LogAndReset("*1", sw);

            // solve part 2 here

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        internal class Machine(Action<string> log, int aX, int aY, int bX, int bY, int prizeX, int prizeY)
        {
            private readonly Action<string> _log = log;
            public int[] A = [aX, aY];
            public int[] B = [bX, bY];
            public int[] Prize = [prizeX, prizeY];

            public IEnumerable<(int a, int b)> FindSolutions(int max = 100)
            {
                for (int a = 0; a < max; a++)
                {
                    for (int b = 0; b < max; b++)
                    {
                        if (
                            a * A[0] + b * B[0] == Prize[0] &&
                            a * A[1] + b * B[1] == Prize[1]
                        )
                        {
                            yield return (a, b);
                        }
                    }
                }
            }

            public int? FindCostOfBestSolution(int max = 100)
            {
                var best = int.MaxValue;
                var found = false;
                _log("New Machine");
                foreach (var sol in FindSolutions(max))
                {
                    var newCost = sol.a * 3 + sol.b;
                    _log($"{sol.a}, {sol.b} => {newCost}");
                    best = Math.Min(best, newCost);
                    found = true;
                }

                return found ? best : null;
            }
        };
    }
}