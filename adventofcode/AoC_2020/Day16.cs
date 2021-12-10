using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using AdventofCode.Utils;
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
        [TestCase(25916, 2564529489989, "Day16.txt")]
        [TestCase(20060, 2843534243843, "Day16_jesper.txt")]
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
            var ruleDic2 = new List<(string name, List<int> values)>();


            foreach (var r in rules)
            {
                var parts = r.Split(new string[] { ": ", "-", " or " }, StringSplitOptions.RemoveEmptyEntries);
                var range = new List<int>();
                range.Add(int.Parse(parts[1]));
                range.Add(int.Parse(parts[2]));
                range.Add(int.Parse(parts[3]));
                range.Add(int.Parse(parts[4]));
                ranges.Add(range);
                ruleDic2.Add((parts[0], range));
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

            var matchingRules = new List<(int index, HashSet<string> names)>();
            for (var ruleIndex = 0; ruleIndex < ruleDic2.Count; ruleIndex++)
            {
                var matchingRules2 = new HashSet<string>();

                foreach (var rule in ruleDic2)
                {
                    var allTicketsValidForRule = true;
                    var r = rule.values;
                    foreach (var ticket in possiblyValidTickets)
                    {
                        var valid = isValid(ticket[ruleIndex], r);

                        if (!valid)
                        {
                            allTicketsValidForRule = false;
                            break;
                        }
                    }

                    if (allTicketsValidForRule)
                    {
                        matchingRules2.Add(rule.name);
                    }
                }
                matchingRules.Add((ruleIndex, matchingRules2));
            }
            LogMidTime("*2-1", sw);

            var unmatchedRuleNames = new HashSet<string>(ruleDic2.Select(r => r.name));
            var ruleOrders = new Dictionary<string, int>();
            var rounds = 0;
            while (unmatchedRuleNames.Any())//name => name.StartsWith("departure")))
            {
                foreach (var rule in matchingRules.Where(r => r.names.Count == 1).ToList())
                {
                    var ruleName = rule.names.First();
                    unmatchedRuleNames.Remove(ruleName);
                    ruleOrders[ruleName] = rule.index;
                    foreach (var o in matchingRules)
                    {
                        o.Item2.Remove(ruleName);
                    }

                    //                    Log($"Found unique matching rule: {ruleName} => index {rule.index}");
                }
                rounds++;
            }

            Log("Rounds: " + rounds);
            part2 = 1;

            foreach (var r in ruleOrders.Keys)
            {
                if (r.StartsWith("departure"))
                {
                    var ruleOrder = ruleOrders[r];
                    var ticketValue = myTicket[ruleOrder];
                    part2 *= ticketValue;
                    //                    Log($"rule {r}:{ruleOrder} value: {ticketValue} => res {part2 }");

                }
            }

            LogAndReset("*2", sw);
            return (part1, part2);
        }

        private static bool isValid(int value, List<int> ruleRanges)
        {
            return (value >= ruleRanges[0] && value <= ruleRanges[1]) || (value >= ruleRanges[2] && value <= ruleRanges[3]);
        }
    }
}

