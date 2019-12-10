using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2019
{
    class Day9 : BaseBaseDay
    {
        [Test]
        [TestCase(
            new long[] { 109, 1, 204, -1, 1001, 100, 1, 100, 1008, 100, 16, 101, 1006, 101, 0, 99 },
            null,
            "Day9_test.txt", new long[0])]
        [TestCase(
            new long[] { 1219070632396864 },
            null,
            "Day9_test2.txt", new long[0])]
        [TestCase(
            new long[] { 1125899906842624 },
            null,
            "Day9_test3.txt", new long[0])]
        [TestCase(new long[] { 2494485073 }, null, "Day9.txt", new long[] { 1 })]
        [TestCase(new long[] { 44997 }, null, "Day9.txt", new long[] { 2 })]
        [TestCase(new long[] { 3013554615 }, null, "Day9_jesper.txt", new long[] { 1 })]
        public void Test1(IEnumerable<long> exp1, int? exp2, string inp, IEnumerable<long> intCodeInput)
        {
            var source = GetResource(inp);
            var res = Compute(source[0], intCodeInput);

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
    }
}