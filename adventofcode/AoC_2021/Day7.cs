﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AdventofCode.AoC_2020;
using NUnit.Framework;

namespace AdventofCode.AoC_2021
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day7 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(37, 168, "Day7_test.txt")]
        [TestCase(325528, 85015836, "Day7.txt")]
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
            var positions = GetIntArr(source[0]);
            LogAndReset("Parse", sw);
            var maxpos = positions.Max();
            var minpos = positions.Min();
            var cheapest = int.MaxValue;

            for (int i = minpos + 1; i < maxpos; i++)
            {
                cheapest = Math.Min(cheapest, GetCost(positions, i));
            }
            part1 = cheapest;


            LogAndReset("*1", sw);
            var cheapest2 = int.MaxValue;

            for (int i = minpos + 1; i < maxpos; i++)
            {
                cheapest2 = Math.Min(cheapest2, GetCost2(positions, i));
            }

            part2 = cheapest2;
            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private int GetCost(int[] positions, int i)
        {
            var sum = 0;
            for (int p = 0; p < positions.Length; p++)
                sum += Math.Abs(positions[p] - i);
            return sum;
        }

        private int GetCost2(int[] positions, int i)
        {

            var sum = 0;
            for (int p = 0; p < positions.Length; p++)
            {
                var dist = Math.Abs(positions[p] - i);
                sum += ((dist + 1) * (dist)) / 2; ;
            }
            return sum;
        }

    }


}