using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2022
{
    using Part1Type = String;
    using Part2Type = String;

    [TestFixture]
    class Day05 : TestBaseClass2<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase("CMZ", "MCD", "Day05_test.txt")]
        [TestCase("ZSQVCCJLL", "QZFJRWHGS", "Day05.txt")]
        public void Test1(Part1Type exp1, Part2Type exp2, string resourceName)
        {
            LogLevel = resourceName.Contains("test") ? 20 : -1;
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type part1, Part2Type part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1 = "";
            Part2Type part2 = "";
            var sw = Stopwatch.StartNew();
            var parts = source.AsGroups().ToList();
            var ops = new List<List<int>>();
            foreach (var op in parts[1])
            {
                var opParts = op.Split(' ').ToList();
                var count = int.Parse(opParts[1]);
                var from = int.Parse(opParts[3])-1;
                var to = int.Parse(opParts[5])-1;
                ops.Add(new List<int> { count, from, to });
            }

            LogAndReset("Parse", sw);

            var stacks = ParseMap(parts);
            foreach (var op in ops)
            {
                MoveCrates(stacks, op[0], op[1], op[2]);
            }
            part1 = string.Join("", stacks.Select(s=>s.Peek()));
            LogAndReset("*1", sw);

            stacks = ParseMap(parts);
            foreach (var op in ops)
            {
                MoveCrates9001(stacks, op[0], op[1], op[2]);
            }
            part2 = string.Join("", stacks.Select(s => s.Peek()));
            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private static List<Stack<char>> ParseMap(List<IList<string>> parts)
        {
            var stacks = new List<Stack<char>>();
            var map = parts[0];
            foreach (var line in map.Take(map.Count - 1).Reverse())
            {
                var pos = 1;
                var stackIndex = 0;
                while (pos < line.Length)
                {
                    if (stacks.Count == stackIndex)
                        stacks.Add(new Stack<char>());
                    if (line[pos] != ' ')
                        stacks[stackIndex].Push(line[pos]);
                    stackIndex++;
                    pos += 4;
                }
            }

            return stacks;
        }

        private void MoveCrates(List<Stack<char>> stacks, int count, int from, int to)
        {
            for (int i = 0; i < count; i++)
            {
                var crate = stacks[from].Pop();
                stacks[to].Push(crate);
            }
        }
        private void MoveCrates9001(List<Stack<char>> stacks, int count, int from, int to)
        {
            var tempStack = new Stack<char>();
            for (int i = 0; i < count; i++)
                tempStack.Push(stacks[from].Pop());
            while (tempStack.Count > 0)
                stacks[to].Push(tempStack.Pop());
        }
    }
}


/*
 * 
 *
    [D]    
[N] [C]    
[Z] [M] [P]
 1   2   3 

move 1 from 2 to 1
move 3 from 1 to 3
move 2 from 2 to 1
move 1 from 1 to 2
*/