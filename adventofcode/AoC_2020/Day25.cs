using System;
using System.Diagnostics;
using System.Windows.Forms.VisualStyles;
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

            LogAndReset("Parse", sw);
            var ints = GetIntInput(source);
            var cardpub = ints[0];
            var doorpub = ints[1];
            var cardLoop = FindKeyLoop(cardpub);
            var doorLoop = FindKeyLoop(doorpub);

            part1 = Transform(cardLoop, doorpub);
            Assert.That(Transform(doorLoop, cardpub), Is.EqualTo(part1));

            LogAndReset("*1", sw);

            LogAndReset("*2", sw);
            return (part1, part2);
        }

        private long Transform(long loopSize, int subjectNumber)
        {
            var temp = 1L;
            for (int i = 0; i < loopSize; i++)
            {
                temp = (temp * subjectNumber) % 20201227;
            }

            return temp;

        }

        private long FindKeyLoop(int publicKey)
        {
            var res = 0L;
            var temp = 1L;
            while (true)
            {
                temp = (temp * 7) % 20201227;
                res++;
                if (temp == publicKey)
                    return res;

            }
        }
    }
}

// var comp = new IntCodeComputer(source[0]);
// comp.Execute();
// var part1 = (int)comp.LastOutput;
// return (part1, 0);
