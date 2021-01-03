using System;
using System.Diagnostics;
using System.Numerics;
using NUnit.Framework;

namespace AdventofCode.AoC_2020
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day25 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(14897079, null, "Day25_test.txt")]
        [TestCase(16933668, null, "Day25.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1 = 0;
            Part2Type part2 = 0;

            var sw = Stopwatch.StartNew();

            var intValues = GetIntInput(source);
            var doorPublicKey = intValues[1];
            var cardPublicKey = intValues[0];

            LogAndReset("Parse", sw);
            var cardLoop = FindLoopSize(cardPublicKey, 7);
            part1 = Transform(cardLoop, doorPublicKey);
            LogAndReset("*1", sw);

            var doorLoop = FindLoopSize(doorPublicKey, 7);
            Assert.That(Transform(doorLoop, cardPublicKey), Is.EqualTo(part1));
            return (part1, part2);
        }

        private long FindLoopSize(int publicKey, int subjectNumber)
        {
            var loopCounter = 0L;
            var temp = 1L;
            while (true)
            {
                temp = (temp * subjectNumber) % 20201227;
                loopCounter++;
                if (temp == publicKey)
                    return loopCounter;
            }
        }

        private long Transform(long loopSize, int subjectNumber)
        {
            return (long)BigInteger.ModPow(subjectNumber, loopSize, 20201227);
        }
    }
}