using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NUnit.Framework;

namespace AdventofCode.AoC_2019
{
    using Part1Type = Int64;
    using Part2Type = Int32;

    [TestFixture]
    class Day16 : TestBaseClass<Part1Type, Part2Type>
    {
        [Test]
        [TestCase(48226158, null, "12345678", 1)]
        [TestCase(34040438, null, "12345678", 2)]
        [TestCase(01029498, null, "12345678", 4)]
        [TestCase(24176176, null, "80871224585914546619083218645595", 100)]
        [TestCase(73745418, null, "19617804207202209144916044189917", 100)]
        [TestCase(52432133, null, "69317163492948606335995924319873", 100)]

        [TestCase(null, 84462026, "03036732577212944063491565474664", 100)]
        [TestCase(null, 78725270, "02935109699940807407585447034323", 100)]
        [TestCase(null, 53553731, "03081770884921959731165446850517", 100)]


        [TestCase(59522422, null, "Day16.txt", 100)]

        public void Test1(Part1Type exp1, Part2Type? exp2, string resourceName, int phases)
        {
            var source = GetResource(resourceName);
            var pattern = new int[] { 0, 1, 0, -1 };
            var digits = source[0].Select(c => int.Parse(c.ToString())).ToList();
            var res = Compute(digits, pattern, phases, source[0]);
            DoAsserts(res, exp1, exp2);
        }

        private (long part1, int? part2) Compute(List<int> digits, int[] pattern, int phases, string orgInput)
        {
            var value = ComputFullMessage(digits, pattern, phases);
            var part1 = int.Parse(value.Substring(0, 8));


//            var digits2 = new List<int>();
//            for (var x = 0; x<10000; x++)
//                digits2.AddRange(digits);
//            var value2 = ComputFullMessage(digits2, pattern, phases);
//            var offset = int.Parse(orgInput.Substring(0, 7));
//            var part2 = int.Parse(value.Substring(offset, 8));

            return (part1, 0);
        }

        private string ComputFullMessage(List<int> digits, int[] pattern, int phases)
        {
            string value = "";
            for (int phase = 0; phase < phases; phase++)
            {
                value = "";
                var nextDigits = new List<int>();
                for (int pos = 0; pos < digits.Count; pos++)
                {
                    var expPatternx = ExpandPattern(pattern, pos + 1, digits.Count).Skip(1).ToList();
                    var expPattern = expPatternx.GetEnumerator();
                    expPattern.MoveNext();
                    int sum = 0;
                    for (int i = 0; i < digits.Count; i++)
                    {
                        var product = expPattern.Current * digits[i];
                        sum = sum + product;
                        expPattern.MoveNext();
                    }

                    sum = Math.Abs(sum) % 10;
                    nextDigits.Add(sum);
                    value += sum.ToString();
                }

                Log($"Phase {phase:D4}: {value}");
                digits = nextDigits;
            }

            return value;
        }

        private IEnumerable<int> ExpandPattern(int[] pattern, int repeats, int digitsLength)
        {
            var yielded = 0;
            while (yielded < digitsLength+1)
                foreach (var value in pattern)
                    for (int i = 0; i < repeats; i++)
                    {
                        yielded++;
                        yield return value;
                    }

        }
    }
}