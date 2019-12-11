using System;
using NUnit.Framework;

namespace AdventofCode.AoC_2019
{
    using Part1Type = Int32;
    using Part2Type = Int32;

    [TestFixture]
    class DayTemplate : TestBaseClass<Part1Type, Part2Type>
    {
        [Test]
        [TestCase(-1, null, "DayX_test.txt")]
        [TestCase(-1, null, "DayX.txt")]
        public void Test1(Part1Type exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = Compute(source);
            DoAsserts(res, exp1, exp2);
        }

        private (Part1Type Part1, Part2Type Part2) Compute(string[] source)
        {
            return (0, 0);
        }
    }
}