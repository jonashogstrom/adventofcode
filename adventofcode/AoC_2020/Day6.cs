using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2020
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day6 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(11, 6, "Day6_test.txt")]
        [TestCase(6726, 3316, "Day6.txt")]
        public void Test1(Part1Type exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            var part1 = 0L;
            foreach (var group in source.AsGroups())
            {
                var uniqueAnswers = new HashSet<char>();
                foreach (var person in group)
                {
                    foreach (var answer in person)
                    {
                        uniqueAnswers.Add(answer);
                    }
                }

                part1 += uniqueAnswers.Count;
                //Log($"Part1: Added {uniqueAnswers.Count} => {part1}");
            }

            var part2 = 0L;
            foreach (var group in source.AsGroups())
            {
                var answerCounter = new DicWithDefault<char, int>();
                foreach (var person in group)
                {
                    foreach (var answer in person)
                    {
                        answerCounter[answer] = answerCounter[answer] + 1;
                    }
                }
                
                var temp = answerCounter.Keys.Count(a => answerCounter[a] == group.Count);

                part2 += temp;
                //Log($"Part2: added {temp} => {part2}");
            }

            return (part1, part2);
        }
    }
}