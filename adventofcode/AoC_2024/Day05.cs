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
    class Day05 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(143, 123, "Day05_test.txt")]
        [TestCase(6041, 4884, "Day05.txt")]
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

            var g = source.AsGroups();
            var rules = new DicWithDefault<int, HashSet<int>>(() => []);
            foreach (var g1 in g.First())
            {
                var parts = g1.Split('|').Select(int.Parse).ToArray();
                rules[parts[0]].Add(parts[1]);
            }


            LogAndReset("Parse", sw);
            var incorrect = new List<int[]>();

            foreach (var update in g.Last())
            {
                var pages = update.Split(',').Select(int.Parse).ToArray();
                var wrong = false;
                for (var i = 0; i < pages.Length; i++)
                for (var j = i + 1; j < pages.Length; j++)
                {
                    if (rules[pages[j]].Contains(pages[i]))
                        wrong = true;
                }

                if (!wrong)
                    part1 += pages[pages.Length / 2];
                else
                {
                    incorrect.Add(pages);
                }
            }

            LogAndReset("*1", sw);

            foreach (var pages in incorrect)
            {
                var newOrder = OrderPages(pages, rules).ToArray();
                part2 += newOrder[pages.Length/2];
            }

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private IEnumerable<int> OrderPages(int[] pages, DicWithDefault<int, HashSet<int>> rules)
        {
            var hashset = new HashSet<int>(pages);
            while (hashset.Any())
            {
                foreach(var page in hashset)
                {
                    if (hashset.Where(p => p!=page).All(p=>rules[p].Contains(page)))
                    {
                        yield return page;
                        hashset.Remove(page);
                        break;
                    }
                }
            }
        }
    }
}