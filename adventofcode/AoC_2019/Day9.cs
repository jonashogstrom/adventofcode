using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2019
{
    class Day9 : BaseDay
    {
        [Test]
        [TestCase(
            new long[] { 109, 1, 204, -1, 1001, 100, 1, 100, 1008, 100, 16, 101, 1006, 101, 0, 99 },
            null,
            "_test", new long[0])]
        [TestCase(
            new long[] {1219070632396864},
            null,
            "_test2", new long[0])]
        [TestCase(
            new long[] { 1125899906842624 },
            null,
            "_test3", new long[0])]
        [TestCase(new long[] { 2494485073 }, null, "", new long[]{ 1 })]
        [TestCase(new long[] { 0 }, null, "", new long[]{ 2 })]
        public void Test1(IEnumerable<long> exp1, int? exp2, string suffix, IEnumerable<long> input)
        {
            var source = GetResource(suffix);
            var res = Compute(source[0], input);

            Assert.That(res.Part1, Is.EqualTo(exp1));
            if (exp2.HasValue)
                Assert.That(res.Part2, Is.EqualTo(exp2.Value));
        }

        // not 203

        private (IEnumerable<long> Part1, int Part2) Compute(string prog, IEnumerable<long> input)
        {
            var computer = new IntCodeComputer(input.ToList(), prog);
            computer.Execute();
            return (computer.Output, 0);
        }

        protected override void Setup()
        {
            Source = InputSource.test;
            //Source = InputSource.prod;

            LogLevel = UseTestData ? 5 : 0;

            Part1TestSolution = null;
            Part2TestSolution = null;
            Part1Solution = null;
            Part2Solution = null;
        }

        protected override void DoRun(string[] input)
        {
            //            var res = Compute(input);
            //            Part1 = res.Part1;
            //
            //            Part2 = res.Part2;
        }
    }
}