using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2021
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day14 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(1588, 2188189693529, "Day14_test.txt")]
        [TestCase(2874, 5208377027195, "Day14.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            //LogLevel = resourceName.Contains("test") ? 20 : -1;
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1 = 0;
            Part2Type part2 = 0;
            var sw = Stopwatch.StartNew();

            var g = source.AsGroups();
            var polymer = g.First().First();
            var rules = g.Last();
            var ruleDic = new Dictionary<string, string>();
            foreach (var r in rules)
            {
                var parts = r.Split(new []{" -> "}, StringSplitOptions.None);
                ruleDic[parts[0]] = parts[1];
            }

            LogAndReset("parse", sw);

            var pairCount = InitPairCount(polymer);
            for (int i = 0; i < 10; i++)
            {
                pairCount = Evolve2(pairCount, ruleDic);
            }
            part1 = CountResult(pairCount, polymer);

            LogAndReset("*1", sw);

            pairCount = InitPairCount(polymer);
            for (int i = 0; i < 40; i++)
            {
                pairCount = Evolve2(pairCount, ruleDic);
            }
            part2 = CountResult(pairCount, polymer);

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private static long CountResult(DicWithDefault<string, long> pairCount, string polymer)
        {
            var doubleCount = new DicWithDefault<char, long>();
            foreach (var k in pairCount.Keys)
            {
                doubleCount[k[0]] += pairCount[k];
                doubleCount[k[1]] += pairCount[k];
            }

            doubleCount[polymer.First()]++;
            doubleCount[polymer.Last()]++;

            var orderedChars = doubleCount.Keys.OrderBy(c => doubleCount[c]);
            return doubleCount[orderedChars.Last()] / 2 - doubleCount[orderedChars.First()] / 2;
        }

        private static DicWithDefault<string, long> InitPairCount(string polymer)
        {
            var xDic = new DicWithDefault<string, long>(0);
            foreach (var p in polymer.AsPairs())
            {
                var sub = p.Item1 + p.Item2.ToString();
                xDic[sub]++;
            }

            return xDic;
        }

        private static DicWithDefault<string, long> Evolve2(DicWithDefault<string, long> pairCount, Dictionary<string, string> ruleDic)
        {
            var newPairCount = new DicWithDefault<string, long>();
            foreach (var pair in pairCount.Keys)
            {
                var oldCount = pairCount[pair];
                var newChar = ruleDic[pair];
                newPairCount[pair[0] + newChar] += oldCount;
                newPairCount[newChar + pair[1]] += oldCount;
            }

            return newPairCount;
        }

        private static string EvolveSlow(string polymer, Dictionary<string, string> ruleDic)
        {
            var sb = new StringBuilder();
            foreach (var p in polymer.AsPairs())
            {
                var sub = p.Item1 + p.Item2.ToString();
                sb.Append(p.Item1);
                sb.Append(ruleDic[sub]);
            }

            sb.Append(polymer.Last());
            var polymer2 = sb.ToString();
            return polymer2;
        }
    }
}