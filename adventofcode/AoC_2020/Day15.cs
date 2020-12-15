using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AdventofCode.AoC_2018;
using NUnit.Framework;

namespace AdventofCode.AoC_2020
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day15 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(436, 175594, "0,3,6")]
        [TestCase(1085, 10652, "1,20,11,6,12,0")]
        [TestCase(1, 2578, "1,3,2")]
        [TestCase(27, 261214, "1,2,3")]
        [TestCase(10, 3544142, "2,1,3")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            var part1 = 0L;
            var part2 = 0L;

            var sw = Stopwatch.StartNew();
            var positionDictionary = new Dictionary<long, int>();

            var pos = 0;
            long lastNumber = -1;
            var data = source[0].Split(',').Select(long.Parse);
            foreach (var v in data)
            {
                pos++;
                positionDictionary[v] = pos;
                lastNumber = v;
            }
            LogAndReset("Parse", sw);

            while (true)
            {
                var newValue = 0L;
                if (positionDictionary.TryGetValue(lastNumber, out var lastIndex))
                {
                    newValue = pos - lastIndex;
                }

                if (pos == 2020)
                {
                    part1 = lastNumber;
                    LogAndReset("*1", sw);
                }

                if (pos == 30000000)
                {
                    part2 = lastNumber;
                    LogAndReset("*2", sw);
                    break;
                }
                positionDictionary[lastNumber] = pos;
                lastNumber = newValue;

                pos++;
            }

            return (part1, part2);
        }
    }
}

// var comp = new IntCodeComputer(source[0]);
// comp.Execute();
// var part1 = (int)comp.LastOutput;
// return (part1, 0);
