using System;
using System.Diagnostics;
using NUnit.Framework;
using System.Collections.Generic;
using AdventofCode.Utils;
using System.Linq;


namespace AdventofCode.AoC_2024
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day22 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(37327623, null, "Day22_test.txt")]
        [TestCase(null, 23, "Day22_test2.txt")]
        [TestCase(14869099597, 1717, "Day22.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            LogLevel = resourceName.Contains("test") ? 20 : -1;
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
            
            var sequenceValues = new Dictionary<int, long>();
            foreach (var s in source.AsInt())
            {
                Log($"----{s}-----------");
                var temp = ComputeSecret(s, 2000, sequenceValues);
                Log($"{s} => {temp}");
                part1 += temp;
            }


            LogAndReset("*1", sw);

            if (sequenceValues.Any())
                part2 = sequenceValues.Values.Max();

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private long ComputeSecret(long seed, long count, Dictionary<int, long> sequenceValues)
        {
            //var sequencesFound = new HashSet<string>();
            var sequencesFound2 = new HashSet<int>();
            var res = seed;
            var intBuff = 0;
            long? prevPrice = null;
            for (int i = 0; i < count; i++)
            {
                res = ComputeNext(res);
                var newPrice = res % 10;
                if (prevPrice.HasValue)
                {
                    var diff = (int)(newPrice - prevPrice);
                    // keep the last 4 diffs as bits in the intbuff-value,
                    // shift the buffer 5 bits each iteration, and mask out the low 20 bits.
                    // add 10 to the diff to make it a positive number.
                    intBuff = ((intBuff << 5) + (diff + 10)) % (1 << 20);
                    if (i >= 4)
                    {
                        if (!sequencesFound2.Contains(intBuff))
                        {
                            if (!sequenceValues.TryAdd(intBuff, newPrice))
                                sequenceValues[intBuff] += newPrice;
                            sequencesFound2.Add(intBuff);
                        }
                    }
                }
                prevPrice = newPrice;
            }

            return res;
        }

        private static long ComputeNext(long secret)
        {
            var s2 = (secret ^ (secret << 6)) & 0b111111111111111111111111; 
            var s3 = (s2 ^ (s2 >> 5)) & 0b111111111111111111111111; 
            var s4 = (s3 ^ (s3 << 11)) & 0b111111111111111111111111; 
            return s4;
        }
    }
}