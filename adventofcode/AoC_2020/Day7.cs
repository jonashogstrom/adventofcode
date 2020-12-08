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
    class Day7 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(4, 32, "Day7_test.txt")]
        [TestCase(null, 126, "Day7_test2.txt")]
        [TestCase(211, 12414, "Day7.txt")]
        [Repeat(10)]
        public void Test1(Part1Type exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            var rules = new Dictionary<string, BagRule>();
            var sw = Stopwatch.StartNew();
            foreach (var s in source)
            {
                var rule = new BagRule(s, rules);
                rules[rule.source] = rule;
            }

            LogAndReset("Parse", sw);

            var shinyGoldBag = rules["shiny gold"];

            var part1 = rules.Values.Count(r => r.CanContain(shinyGoldBag));
            LogAndReset("*1", sw);

            var part2 = shinyGoldBag.ContentCount;
            LogAndReset("*2", sw);
            return (part1, part2);
        }
    }

    internal class BagRule
    {
        private readonly Dictionary<string, BagRule> _rules;
        public string source;
        public List<(string color, int count)> ContentsList = new List<(string, int)>();
        private int _contentCount = -1;
        private Dictionary<BagRule, bool> _canContain2 = new Dictionary<BagRule, bool>();

        public BagRule(string bagDescription, Dictionary<string, BagRule> rules)
        {
            _rules = rules;

            var pp1 = bagDescription.Split(' ');
            source = pp1[0] + " " + pp1[1];
            var pos = 4;
            while (pos < pp1.Length && pp1[pos] != "no")
            {
                ContentsList.Add(($"{pp1[pos+1]} {pp1[pos+2]}", int.Parse(pp1[pos])));
                pos += 4;
            }
        }

        public int ContentCount
        {
            get
            {
                if (_contentCount == -1)
                {
                    var sum = 0;
                    foreach (var c in ContentsList)
                    {
                        sum += (_rules[c.color].ContentCount + 1) * c.count;
                    }

                    _contentCount = sum;
                }

                return _contentCount;
            }
        }

        public bool CanContain(BagRule r)
        {
            if (!_canContain2.ContainsKey(r))
            {
                var res = false;
                foreach (var x in ContentsList)
                {
                    var r2 = _rules[x.color];
                    res = r2 == r || r2.CanContain(r);

                    if (res)
                        break;
                }

                _canContain2[r] = res;
            }

            return _canContain2[r];
        }
    }
}
