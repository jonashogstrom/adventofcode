using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NUnit.Framework;

namespace AdventofCode.AoC_2019
{
    using Part1Type = Int64;
    using Part2Type = Int32;

    [TestFixture]
    class Day16 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        private int[] Pattern = { 0, 1, 0, -1 };

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


        [TestCase(59522422, 18650834, "Day16.txt", 100)]

        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName, int phases)
        {
            Debug = false;
            var source = GetResource(resourceName);
            var digits = source[0].Select(c => int.Parse(c.ToString())).ToArray();
            var res = Compute(digits, phases, source[0], exp2.HasValue);
            DoAsserts(res, exp1, exp2);
        }

        private (long part1, int? part2) Compute(int[] digits, int phases, string orgInput, bool doPart2)
        {
            var value = ComputeFullMessage(digits, phases, 0, digits.Length);
            var part1 = int.Parse(value.Substring(0, 8));

            int part2 = -1;

            if (doPart2)
            {
                part2 = SmartCalcPart2WithOffset(digits, orgInput, phases);
            }

            return (part1, part2);
        }

        private int SmartCalcPart2WithOffset(IReadOnlyList<int> orgDigits, string orgInput, int phases)
        {
            var offset = int.Parse(orgInput.Substring(0, 7));
            var allDigits = new int[orgDigits.Count * 10000];
            for (var x = 0; x < 10000; x++)
                for (var y = 0; y < orgDigits.Count; y++)
                    allDigits[x * orgDigits.Count + y] = orgDigits[y];

            Assert.That(offset, Is.GreaterThan(allDigits.Length / 2));
            
            for (int phase = 0; phase < phases; phase++)
            {
                var sum = 0;
                var tempDigits = new int[allDigits.Length];
                for (int i = allDigits.Length - 1; i >= offset; i--)
                {
                    sum += allDigits[i];
                    tempDigits[i] = Math.Abs(sum) % 10;
                }

                allDigits = tempDigits;
            }

            var values = allDigits.Skip(offset).Take(8).ToArray();
            var result = values.Aggregate(0, (acc, v) => acc * 10 + v);

            Log($"{result}");

            return result;
        }

        private string ComputeFullMessage(int[] digits, int phases, int start, int stop)
        {
            for (int phase = 0; phase < phases; phase++)
            {
                var nextDigits = new int[digits.Length];
                for (int z = 0; z < digits.Length; z++)
                    nextDigits[z] = -1;
                for (int row = start; row < stop; row++)
                {
                    var sb = new StringBuilder();
                    int sum = 0;
                    for (int col = start; col < stop; col++)
                    {

                        var factor = GetFactor(Pattern, col + 1, row);
                        if (factor != 0)
                        {
                            var d = digits[col];
                            if (d == -1)
                            {
                                Log("-1");
                            }
                            var product = factor * d;
                            sum = sum + product;
                            var sign = factor < 0 ? '-' : '+';
                            if (Debug)
                                sb.Append($"{sign}{d}");
                        }
                        else
                        {
                            if (Debug)
                                sb.Append("  ");
                        }
                    }

                    sum = Math.Abs(sum) % 10;
                    if (Debug)
                    {
                        sb.Remove(0, 1);
                        sb.Append($" = {sum}");

                        Log(sb.ToString);
                    }
                    nextDigits[row] = sum;
                }

                if (Debug)
                {
                    var temp = new StringBuilder();
                    for (int i = start; i < stop; i++)
                    {
                        temp.Append(nextDigits[i].ToString());
                    }
                    Log($"Phase {phase:D4}: {temp}");
                    Log("=========================================");
                }

                digits = nextDigits;
            }

            var res = new StringBuilder();
            for (int i = 0; i < digits.Length; i++)
            {
                if (digits[i] == -1)
                    res.Append("X");
                else
                    res.Append(digits[i].ToString());
            }
            return res.ToString();
        }


        private static int GetFactor(int[] pattern, int col, int row)
        {
            var repeatLength = row + 1;
            var fullCycleLength = repeatLength * pattern.Length;

            //            var fullcycles = col / fullCycleLength;
            var cyclePos = col % fullCycleLength;
            var x = cyclePos / repeatLength;
            return pattern[x];
        }
    }
}