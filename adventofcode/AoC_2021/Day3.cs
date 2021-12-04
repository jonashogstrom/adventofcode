using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace AdventofCode.AoC_2021
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day3 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(198, 230, "Day3_test.txt")]
        [TestCase(3958484, null, "Day3.txt")]
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
            var gamma = "";
            var epsilon = "";
            for (int bit = 0; bit < source[0].Length; bit++)
            {
                var msb = MostCommonBit(source, bit,0);
                gamma += msb.ToString();
                epsilon += (1 - msb).ToString();
            }
            Log("Gamma: " + gamma);
            Log("epsilon: " + epsilon);
            part1 = Convert.ToInt32(gamma, 2) * Convert.ToInt32(epsilon, 2); ;
            LogAndReset("*1", sw);

            Log("Calculating oxygen");
            var oxygen = Eliminate(source.ToList(),  true, 1);
            Log("Calculating CO2");
            var co2 = Eliminate(source.ToList(), false, 1);
        

            part2 = Convert.ToInt32(oxygen, 2) * Convert.ToInt32(co2, 2); ;

            LogAndReset("*2", sw);
            return (part1, part2);
        }

        private string Eliminate(List<string> source, bool b, int def)
        {
            var bit = 0;
            while (source.Count > 1)
            {
                var msb = MostCommonBit(source, bit, def);
                var filterBitValue = b ? msb : 1 - msb;
                var filterBit = filterBitValue == 0 ? '0' : '1';
                Log("Checking bit " + bit+" Filterbit is "+filterBit);

                var newList = new List<string>();
                foreach (var s in source)
                {
                    if (s[bit] == filterBit)
                    {
                        Log("Keeping "+ s);
                        newList.Add(s);

                    }
                }

                source = newList;

                bit++;
            }

            return source.First();
        }

        public int MostCommonBit(IEnumerable<string> data, int bitPos, int def)
        {
            var zeros = 0;
            foreach (var s in data)
            {
                if (s[bitPos] == '0') zeros++;
            }

            var ones = data.Count() - zeros;
            if (zeros == ones)
                return def;
            if (zeros > ones)
                return 0;
            return 1;
        }
    }
}