using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using NUnit.Framework;

namespace AdventofCode.AoC_2020
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day7 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(4, 32, "Day7_test.txt")]
        [TestCase(null, 126, "Day7_test2.txt")]
        [TestCase(211, null, "Day7.txt")]
        public void Test1(Part1Type exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            var part1 = 0;
            var part2 = 0;
            var rules = new Dictionary<string, BagRule>();
            foreach (var s in source)
            {
                var parts = s.Split(new[] { "bags", "contain", "bag", ",", "." }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(y => !string.IsNullOrEmpty(y))
                    .ToArray();
                var rule = new BagRule(parts);
                rules[rule.source] = rule;
            }

            part1 = FindBags(rules, "shiny gold");
            part2 = FindBagsContents(rules, "shiny gold");
            return (part1, part2);
        }

        private int FindBags(Dictionary<string, BagRule> rules, string target)
        {
            var t = FindBags2(rules, new HashSet<string> { target });
            return t.Count - 1;
        }

        private int FindBagsContents(Dictionary<string, BagRule> rules, string target)
        {
            while (rules[target].contents == -1)
            {
                foreach (var r in rules.Keys)
                {
                    var rule = rules[r];
                    if (rule.contents == -1)
                    {
                        var sum = 1;
                        var ok = true;
                        foreach (var d in rule.dest.Keys)
                        {
                            if (rules[d].contents != -1)
                                sum += rule.dest[d] * rules[d].contents;
                            else
                            {
                                ok = false;
                            }

                        }

                        if (ok)
                            rule.contents = sum;
                    }
                }
            }

            return rules[target].contents-1;
        }
        private HashSet<string> FindBags2(Dictionary<string, BagRule> rules, HashSet<string> targets)
        {
            var res = new HashSet<string>(targets);
            foreach (var x in rules.Keys)
            {
                foreach (var t in targets)
                    if (rules[x].dest.ContainsKey(t))
                        res.Add(x);
            }

            if (res.Count == targets.Count)
                return res;
            return FindBags2(rules, res);

        }
    }

    internal class BagRule
    {
        public string source;
        public Dictionary<string, int> dest = new Dictionary<string, int>();

        public BagRule(string[] parts)
        {
            source = parts[0];
            foreach (var p in parts.Skip(1))
            {
                if (p != "no other")
                {
                    var pp = p.Split(new[] { ' ' }, 2);

                    dest[pp[1]] = int.Parse(pp[0]);
                    contents = -1;
                }
                else
                    contents = 1;
            }
        }

        public int contents;
    }
}
