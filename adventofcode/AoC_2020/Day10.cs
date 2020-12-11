using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2020
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day10 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(7 * 5, 8, "Day10_test.txt")]
        [TestCase(22 * 10, 19208, "Day10_test2.txt")]
        [TestCase(2112, 3022415986688, "Day10.txt")]
        [Repeat(5)]
        public void Test1(Part1Type exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {

            var sw = Stopwatch.StartNew();

            var adapters = GetIntInput(source).OrderBy(x1 => x1).ToList();


            LogAndReset("Parse", sw);
            adapters.Insert(0, 0);
            adapters.Add(adapters.Last() + 3);
            var diffs = adapters.AsPairs().Select(pair => pair.Item2 - pair.Item1).ToArray();
            var counts = diffs.CountValues();
            var part1 = counts[1] * counts[3];
            LogAndReset("*1", sw);

            var permutations = new Dictionary<long, int> {[1] = 1, [2] = 2, [3] = 4, [4] = 7};


            var part2 = diffs.FindSequenceLengthsForKey(1).Select(i=>permutations[i]).Multiply();

            
            LogAndReset("*2", sw);
            return (part1, part2);
        }
    }
}

