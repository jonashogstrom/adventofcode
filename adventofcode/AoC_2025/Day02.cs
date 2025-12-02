using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace AdventofCode.AoC_2025
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day02 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(1227775554, 4174379265L, "Day02_test.txt")]
        [TestCase(44487518055, 53481866137, "Day02.txt")]
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

            var ranges = source[0].Split(',').Select(s => s.Split('-').Select(long.Parse).ToArray()).ToArray();

            LogAndReset("Parse", sw);
            foreach (var range in ranges)
            {
                for (var p = range[0]; p <= range[1]; p++)
                {
                    var s = p.ToString();
                    var mid = s.Length / 2;
                    var p1 = s[..mid];
                    var p2 = s[mid..];
                    if (s.Length % 2 == 0 && p1 == p2)
                    {
                        part1 += p;
                    }
                }
            }
            
            LogAndReset("*1", sw);

            foreach (var range in ranges)
            {
                for (var p = range[0]; p <= range[1]; p++)
                {
                    var s = p.ToString();
                    var invalid = false;
                    for (var l = 1; l<=s.Length/2; l++ )
                        if (s.Length % l == 0)
                        {
                            var times = s.Length / l;
                            var containsOnlyRepeats = true;
                            var p0 = s[..l];
                            for (int x = 1; x < times; x++)
                            {
                                var start = l * x;
                                var stop = start + l;

                                if (p0 != s[start..stop])
                                {
                                    containsOnlyRepeats = false;
                                    break;
                                }
                            }
                            if (containsOnlyRepeats)
                            {
                                invalid = true;
                                break;
                            }
                        }

                    if (invalid)
                    {
                        part2 += p;
                        //Console.WriteLine(s+" is invalid");
                    }
                }
            }

            LogAndReset("*2", sw);

            return (part1, part2);
        }
    }
}