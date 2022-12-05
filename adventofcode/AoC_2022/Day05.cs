using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2022
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day05 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(-1, null, "Day05_test.txt")]
        [TestCase(-1, null, "Day05.txt")]
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
            LogAndReset("Parse", sw);
            var parts = source.AsGroups().ToList();
            var stacks = new List<Stack<char>>();
            var map = parts[0];
            foreach (var line in map.Take(map.Count - 1))
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

            for (int i = 0; i<stacks.Count; i++)
                stacks[i] = new Stack<char>(stacks[i]);
            foreach (var op in parts[1])
            {
                var opParts = op.Split(' ').ToList();
                var count = int.Parse(opParts[1]);
                var from = int.Parse(opParts[3])-1;
                var to = int.Parse(opParts[5])-1;
                MoveCrates9001(stacks, count, from, to);
            }

            var res = string.Join("", stacks.Select(s=>s.Peek()));
            LogAndReset("*1", sw);
            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private void MoveCrates(List<Stack<char>> stacks, int count, int from, int to)
        {
            for (int i = 0; i < count; i++)
                MoveCrate(stacks, from, to);
        }
        private void MoveCrates9001(List<Stack<char>> stacks, int count, int from, int to)
        {
            var stack = new Stack<char>();
            for (int i = 0; i < count; i++)
                stack.Push(stacks[from].Pop());
            while (stack.Count > 0)
                stacks[to].Push(stack.Pop());
        }

        private void MoveCrate(List<Stack<char>> stacks, int from, int to)
        {
            var crate = stacks[from].Pop();
            stacks[to].Push(crate);
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