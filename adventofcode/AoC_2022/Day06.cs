using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2022
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day06 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(7, 19, "mjqjpqmgbljsphdztnvjfqwrcgsmlb")]
        [TestCase(5, 23, "bvwbjplbgvbhsrlpgdmjqwftvncz")]
        [TestCase(6, 23, "nppdvjthqldpwncqszvftbrmjlhg")]
        [TestCase(10, 29, "nznrnfrfntjfmvfwmzdfjlvtqnbhcprsg")]
        [TestCase(11, 26, "zcfzfwzzqfrljwzlrfnpqdbhtmscgvjw")]
        [TestCase(1300, 3986, "Day06.txt")]
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
           
            var msg = source.First();
            part1 = FindMarker(msg, 4);
            LogAndReset("*1", sw);
            part2 = FindMarker(msg, 14);
            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private static long FindMarker(string msg, int markerLength)
        {
            var pos = 0;
            while (true)
            {
                var marker = msg.Substring(pos, markerLength);

                if (marker.ToHashSet().Count == markerLength)
                {
                    return pos + markerLength;
                }

                pos++;
            }
        }
    }
}