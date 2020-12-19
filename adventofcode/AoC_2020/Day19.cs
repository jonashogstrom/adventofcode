using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2020
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day19 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(2, null, "Day19_test.txt")]
        [TestCase(3, 12, "Day19_test2.txt")]
        [TestCase(129, 243, "Day19.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            var sw = Stopwatch.StartNew();

            var groups = source.AsGroups();
            var rules = groups.First();
            var data = groups.Last();

            var ruleDic = new Dictionary<int, Rule19>();
            foreach (var s in rules)
            {
                ParseRule(s, ruleDic);
            }

            LogAndReset("Parse", sw);

            long part1 = CountMatches(data, ruleDic);

            LogAndReset("*1", sw);

            ParseRule("8: 42 | 42 8", ruleDic);
            ParseRule("11: 42 31 | 42 11 31", ruleDic);
            long part2 = CountMatches(data, ruleDic);

            LogAndReset("*2", sw);
            return (part1, part2);
        }

        private int CountMatches(IList<string> data, Dictionary<int, Rule19> ruleDic)
        {
            var x = 0;
            foreach (var i in data)
            {
                var res = ruleDic[0].Match(i, 0, ruleDic);
                if (res.Any(r => r == i.Length))
                {
                    //Log($"Matching data {i}");
                    x++;
                }
            }

            return x;
        }

        private void ParseRule(string s, Dictionary<int, Rule19> ruleDic)
        {
            var p = s.Replace(": ", ":").Replace(" | ", "|").Split(':');
            var ruleIndex = int.Parse(p[0]);
            if (p[1].StartsWith("\""))
                ruleDic[ruleIndex] = new Rule19lit(p[1][1]);
            else
            {
                var alternatives = p[1].Split('|');
                var r = new Rule19r();
                foreach (var alt in alternatives)
                {
                    r.Alternatives.Add(GetInts(alt, false).ToList());
                }

                ruleDic[ruleIndex] = r;
            }
        }
    }

    internal class Rule19r : Rule19
    {
        public List<List<int>> Alternatives = new List<List<int>>();

        public override HashSet<int> Match(string s, int initialIndex, Dictionary<int, Rule19> ruleDic)
        {
            var res = new HashSet<int>();
            if (initialIndex == s.Length)
                return res;

            foreach (var alt in Alternatives)
            {

                var p = new HashSet<int> { initialIndex };

                foreach (var r in alt)
                {
                    var results = new HashSet<int>();
                    var rule = ruleDic[r];
                    foreach (var i in p)
                    {
                        var matchRes = rule.Match(s, i, ruleDic);
                        foreach (var m in matchRes)
                            results.Add(m);
                    }

                    if (results.Any())
                        p = results;
                    else
                    {
                        p = null;
                        break;
                    }
                }

                if (p != null)
                    foreach (var x in p)
                        res.Add(x);
            }

            return res;
        }
    }

    internal class Rule19lit : Rule19
    {
        public char C { get; }

        public Rule19lit(char c)
        {
            C = c;
        }

        public override HashSet<int> Match(string s, int startindex, Dictionary<int, Rule19> ruleDic)
        {
            var res = new HashSet<int>();
            if (startindex < s.Length && s[startindex] == C)
                res.Add(startindex + 1);
            return res;
        }
    }

    internal abstract class Rule19
    {
        public abstract HashSet<int> Match(string s, int startindices, Dictionary<int, Rule19> ruleDic);
    }
}