using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using AdventofCode.AoC_2020;
using NUnit.Framework;

namespace AdventofCode.AoC_2021
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day6 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(5934, 26984457539, "Day6_test.txt")]
        [TestCase(383160, 1721148811504, "Day6.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            if (resourceName.Contains("test"))
                LogLevel = 20;

            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1 = 0;
            Part2Type part2 = 0;
            var sw = Stopwatch.StartNew();
            IEnumerable<int> state = GetIntArr(source.First());

            var res = EvolveFish(256);

            foreach (var s in state)
            {
                part1 += res[80 - s];
                part2 += res[256 - s];
            }

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private List<long> EvolveFish(int days)
        {
            var state = new long[9];
            state[1] = 1;

            var res = new List<long>();
            for (int d = 0; d < days; d++)
            {
                var nextstate = new long[9];
                for (var f = 1; f < state.Length; f++)
                    nextstate[f - 1] = state[f];
                nextstate[8] = state[0];
                nextstate[6] += state[0];
                state = nextstate;
                res.Add(state.Sum());
            }
            return res;

        }
    }


}