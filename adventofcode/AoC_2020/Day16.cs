using System;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Collections.Generic;
using NUnit.Framework;

namespace AdventofCode.AoC_2020
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    // part2  not 434014277
    [TestFixture]
    class Day16 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(71, null, "Day16_test.txt")]
        [TestCase(null, 1, "Day16_test2.txt")]
        [TestCase(25916, null, "Day16.txt")]
        [TestCase(null, null, "Day16_jesper.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1 = 0;
            Part2Type part2 = 0;

            var sw = Stopwatch.StartNew();

            var x = source.AsGroups().ToList();
            var rules = x[0];
            var myTicket = GetInts(x[1].Last()).ToList();
            var tickets = x[2].Skip(1).ToList();
            var ranges = new List<List<int>>();
            var ruleDic = new Dictionary<string, List<int>>();

            foreach (var r in rules)
            {
                var parts = r.Split(new string[] { ": ", "-", " or " }, StringSplitOptions.RemoveEmptyEntries);
                var range = new List<int>();
                range.Add(int.Parse(parts[1]));
                range.Add(int.Parse(parts[2]));
                range.Add(int.Parse(parts[3]));
                range.Add(int.Parse(parts[4]));
                ranges.Add(range);
                ruleDic[parts[0]] = range;
            }
            LogAndReset("Parse", sw);
            var invalidSum = 0L;
            var possiblyValidTickets = new List<List<int>>();
            foreach (var t in tickets)
            {
                var values = GetInts(t);
                var validForAll = true;
                foreach (var v in values)
                {
                    var validForAny = false;
                    foreach (var r in ranges)
                    {
                        var valid = isValid(v, r);
                        validForAny = validForAny || valid;
                        if (validForAny) break;
                    }

                    if (!validForAny)
                    {
                        invalidSum += v;
                        validForAll = false;
                    }
                }
                if (validForAll)
                {
                    possiblyValidTickets.Add(values.ToList());
                }

            }

            part1 = invalidSum;
            LogAndReset("*1", sw);
            var unmatchedRuleIndices = new HashSet<int>(Enumerable.Range(0, ruleDic.Count));
            var unmatchedRuleNames = new HashSet<string>(ruleDic.Keys);
            var ruleOrders = new Dictionary<string, int>();
            var rounds = 0;
            while (unmatchedRuleNames.Any(name=>name.StartsWith("departure")))
            {
                foreach (var ruleIndex in unmatchedRuleIndices.ToList())
                {
                    var matchingRules = new List<string>();
                    foreach (var rule in unmatchedRuleNames)
                    {
                        var allTicketsValidForRule = true;

                        foreach (var t in possiblyValidTickets)
                        {
                            var valid = isValid(t[ruleIndex], ruleDic[rule]);

                            if (!valid)
                            {
                                allTicketsValidForRule = false;
                                break;
                            }
                        }
                        if (allTicketsValidForRule)
                            matchingRules.Add(rule);
                    }

                    if (matchingRules.Count == 1)
                    {
                        unmatchedRuleNames.Remove(matchingRules.First());
                        unmatchedRuleIndices.Remove(ruleIndex);
                        ruleOrders[matchingRules.First()] = ruleIndex;
                        Log($"Found unique matching rule: {matchingRules.First()} => index {ruleIndex}");
                    }

                }

                rounds++;
            }
            var sum = 1L;

            foreach (var r in ruleOrders.Keys)
            {
                if (r.StartsWith("departure"))
                {
                    var ruleOrder = ruleOrders[r];
                    var ticketValue = myTicket[ruleOrder];
                    sum *= ticketValue;
                    Log($"rule {r}:{ruleOrder} value: {ticketValue} => res {sum}");

                }
            }

            part2 = sum;
            LogAndReset("*2", sw);
            return (part1, part2);
        }

        private static bool isValid(int value, List<int> ruleRanges)
        {
            return (value >= ruleRanges[0] && value <= ruleRanges[1]) || (value >= ruleRanges[2] && value <= ruleRanges[3]);
        }
    }
}

