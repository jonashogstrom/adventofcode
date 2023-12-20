using System;
using System.Diagnostics;
using NUnit.Framework;
using System.Collections.Generic;
using System.Data;
using AdventofCode.Utils;
using System.Linq;


namespace AdventofCode.AoC_2023
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day19 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(19114, 167409079868000, "Day19_test.txt")]
        [TestCase(420739, 130251901420382, "Day19.txt")]
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
            var workflows = new Dictionary<string, Workflow>();
            var parts = new List<Part>();
            foreach (var s in g.First())
            {
                var r = Workflow.Parse(s);
                workflows[r.Name] = r;
            }

            foreach (var s in g.Last())
            {
                var p = Part.Parse(s);
                parts.Add(p);
            }

            LogAndReset("Parse", sw);

            var accepted = new List<Part>();

            foreach (var p in parts)
            {
                var w = workflows["in"];
                while (w != null)
                {
                    var res = w.Handle(p);
                    if (res == "A")
                    {
                        accepted.Add(p);
                        break;
                    }
                    if (res == "R")
                        break;
                    w = workflows[res];
                }
            }

            foreach (var p in accepted)
                part1 += p.TotalValue();

            LogAndReset("*1", sw);

            var possibilities = new Dictionary<char, Range>();
            possibilities['x'] = new Range(1, 4000);
            possibilities['m'] = new Range(1, 4000);
            possibilities['a'] = new Range(1, 4000);
            possibilities['s'] = new Range(1, 4000);

            part2 = workflows["in"].Solve2(possibilities, workflows);

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        internal class Part
        {
            // {x=787,m=2655,a=1222,s=2876}
            public static Part Parse(string s)
            {
                var x = new SplitNode(s.Trim('{', '}'), ',', '=');
                var part = new Part();
                foreach (var c in x.Children)
                    part.Values[c.First.Trimmed[0]] = c.Second.i;
                return part;
            }

            public Dictionary<char, int> Values { get; } = new();

            public long TotalValue()
            {
                return Values.Values.Sum();
            }
        }

        internal class Workflow
        {
            private readonly List<RuleX> _rules;
            public string Name { get; }

            public static Workflow Parse(string s)
            {
                // px{a<2006:qkq,m>2090:A,rfg}

                var x = new SplitNode(s.TrimEnd('}'), '{', ',');
                var rule = new Workflow(x.First.First.Trimmed, x.Second.Children.Select(c => c.Trimmed).ToList());
                return rule;
            }

            private Workflow(string name, List<string> rules)
            {
                _rules = rules.Select(x => RuleX.Parse(x)).ToList();
                Name = name;
            }

            public string Handle(Part part)
            {
                foreach (var c in _rules)
                {
                    if (c.Handle(part))
                        return c.Next;
                }

                throw new Exception("");
            }

            public long Solve2(Dictionary<char, Range> possibilities, Dictionary<string, Workflow> workflows)
            {
                var sum = 0L;
                var empty = possibilities.Values.Any(r => r.Start.Value > r.End.Value);
                if (empty)
                    return 0;
                foreach (var r in _rules)
                {
                    if (r.HasCond)
                    {
                        var res= r.Split(possibilities);
                        if (r.Next == "A")
                            sum += Multiply(res.Item1);
                        else if (r.Next != "R")
                            sum += workflows[r.Next].Solve2(res.Item1, workflows);
                        possibilities = res.Item2;
                    }
                    else if (r.Next == "A")
                    {
                        sum += Multiply(possibilities);
                    }
                    else if (r.Next != "R")
                        sum += workflows[r.Next].Solve2(possibilities, workflows);
                }

                return sum;
            }

            private static long Multiply(Dictionary<char, Range> possibilities)
            {
                var temp = 1L;
                foreach (var v in possibilities.Values)
                    temp = temp * (v.End.Value - v.Start.Value + 1);
                return temp;
            }
        }
        internal class RuleX
        {
            private readonly string _rule;
            private readonly int _comp;
            private readonly char _variable;
            private readonly int _value;
            public string Next { get; }
            public bool HasCond => _rule != null;

            private RuleX(string next)
            {
                Next = next;
            }
            private RuleX(string rule, string next)
            {
                _rule = rule;
                _comp = rule[1] == '<' ? -1 : +1;
                _variable = rule[0];
                _value = int.Parse(rule[2..]);
                Next = next;

            }

            public static RuleX Parse(string s)
            {
                // x < 1416:A
                // crn
                var parts = s.Split(':');
                if (parts.Length == 1)
                    return new RuleX(s);
                return new RuleX(parts[0], parts[1]);
            }

            public bool Handle(Part part)
            {
                if (_rule == null) return true;
                var comp = part.Values[_variable].CompareTo(_value);
                return comp == _comp;
            }

            public (Dictionary<char, Range>, Dictionary<char, Range>) Split(Dictionary<char, Range> possibilities)
            {
                var passRule = new Dictionary<char, Range>();
                var leftOver = new Dictionary<char, Range>();
                foreach (var k in possibilities)
                {
                    if (k.Key == _variable)
                    {
                        if (_comp == -1)
                        {
                            // must be smaller than comp-value
                            passRule[k.Key] = new Range(k.Value.Start, _value-1);
                            leftOver[k.Key] = new Range(_value, k.Value.End.Value);
                        }
                        else
                        {
                            passRule[k.Key] = new Range(_value+1, k.Value.End);
                            leftOver[k.Key] = new Range(k.Value.Start, _value);
                        }
                    }
                    else
                    {
                        passRule[k.Key] = k.Value;
                        leftOver[k.Key] = k.Value;
                    }
                }
                return (passRule, leftOver);
            }
        }
    }
}