using System.Collections.Generic;
using NUnit.Framework;

namespace AdventofCode.AoC_2019
{
    class DayTemplate2 : BaseDay
    {
        [Test]
        [TestCase(-1, null, "_test", new long[0])]
        [TestCase(-1, null, "", new long[] { 1 })]
        public void Test1(int exp1, int? exp2, string suffix, IEnumerable<long> input)
        {
            var source = GetResource(suffix);
            var res = Compute(source);

            Assert.That(res.Part1, Is.EqualTo(exp1));
            // Clipboard.SetText(res.Part1.ToString());
            if (exp2.HasValue)
            {
                Assert.That(res.Part2, Is.EqualTo(exp2.Value));
                // Clipboard.SetText(res.Part1.ToString());
            }
        }

        // not 203

        private (int Part1, int Part2) Compute(string[] input)
        {
            return (0, 0);
        }

        protected override void Setup()
        {
            //            Source = InputSource.test;
            //            //Source = InputSource.prod;
            //
            //            LogLevel = UseTestData ? 5 : 0;
            //
            //            Part1TestSolution = null;
            //            Part2TestSolution = null;
            //            Part1Solution = null;
            //            Part2Solution = null;
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