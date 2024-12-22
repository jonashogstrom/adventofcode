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

    // started 23.02 dec 20
    [TestFixture]
    class Day19 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(6, null, "Day19_test.txt")]
        [TestCase(1, null, "Day19_test2.txt")]
        [TestCase(283, null, "Day19.txt")]
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
            var towels = g.First()[0].Split(',').Select(s => s.Trim());
            // var towelDic = new Dictionary<char, List<ReadOnlyMemory<char>>>();
            var root = new Node();
            var usedTowels = 0;
            foreach (var towel in towels.OrderBy(t=>t.Length))
            {
                if (Solve2(towel, root, root, 0))
                {
                    Log($"Skipping towel {towel}");
                    continue;
                }
                usedTowels++;
                var s = "";
                var temp = root;
                foreach (var c in towel)
                {
                    s += c;
                    if (!temp.next.TryGetValue(c, out var next))
                    {
                        next = new Node();
                        next.nodeStr = s;
                        temp.next[c] = next;
                    }

                    temp = next;
                }

                temp.term = true;
                temp.towel = towel;
                
                // if (!towelDic.TryGetValue(towel[0], out var list))
                // {
                //     list = new List<ReadOnlyMemory<char>>();
                //     towelDic.Add(towel[0], list);
                // }
                //
                // list.Add(towel.AsMemory());
                
            }

            Log($"Used towels: {usedTowels}");


            LogAndReset("Parse", sw);
            var strings = g.Last().ToArray();
            for (int i = 0; i < strings.Length; i++)
            {
                var t = strings[i];
                if (t.StartsWith("//"))
                    continue;
                var b = Solve2(t, root, root, 0);
                if (b)
                    part1++;
            }

            LogAndReset("*1", sw);

            // solve part 2 here

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private bool Solve2(string str, Node root, Node n, int pos)
        {
            if (!n.next.TryGetValue(str[pos], out var next))
                return false;
            if (pos == str.Length-1)
                return next.term;
            if (Solve2(str, root, next, pos+1))
                return true;
            return next.term && Solve2(str, root, root, pos+1);
            // if (n.term)
            // {
            //     if (str.IsEmpty)
            //         return true;
            //     if (!n.next.TryGetValue(str[0], out var next))
            //         return Solve2(str.Slice(1), root, n);
            // }
            // else
            // {
            //     if (!n.next.TryGetValue(str[0], out var next))
            //         return Solve2(str.Slice(1), root, n);
            //     return false;
            // }
            //
            //
            // if (str.IsEmpty)
            //     return n.term;
            // if (!n.next.TryGetValue(str[0], out var next))
            //     return false;
            //
            // return Solve2(str.Slice(1), root, n.term?root:next);

        }

        private bool Solve(ReadOnlySpan<char> s, Dictionary<char, List<ReadOnlyMemory<char>>> towelDic)
        {
            if (s.Length == 0)
                return true;
            if (towelDic.TryGetValue(s[0], out var list))
            {
                foreach (var l in list)
                {
                    if (s.StartsWith(l.Span))
                    {
                        if (Solve(s.Slice(l.Span.Length), towelDic))
                            return true;
                    }
                }
            }

            return false;
        }

        public class Node
        {
            public bool term { get; set; }
            public string towel { get; set; }
            public string nodeStr { get; set; }

            public Dictionary<char, Node> next = new(); 
        }
    }
}