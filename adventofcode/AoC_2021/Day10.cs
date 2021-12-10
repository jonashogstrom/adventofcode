using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2021
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day10 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(26397, 288957, "Day10_test.txt")]
        [TestCase(318081, null, "Day10.txt")]
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
            var scores = new List<long>();
            foreach (var s in source)
            {
                var res = SyntaxErrorScore(s);
                part1 += res.error;
                if (res.complete != 0)
                    scores.Add(res.complete);

            }

            scores.Sort();
            part2 = scores[scores.Count / 2 ];
            LogAndReset("*1", sw);
            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private (long error ,long complete) SyntaxErrorScore(string s)
        {
            var scoreMap= new Dictionary<char, int>();
            scoreMap[')'] = 3;
            scoreMap[']'] = 57;
            scoreMap['}'] = 1197;
            scoreMap['>'] = 25137;
            var completeMap= new Dictionary<char, int>();
            completeMap[')'] = 1;
            completeMap[']'] = 2;
            completeMap['}'] = 3;
            completeMap['>'] = 4;
            var expMap = new Dictionary<char, char>();
            expMap['('] = ')';
            expMap ['['] = ']';
            expMap ['{'] = '}';
            expMap ['<'] = '>';
            
            var stack = new Stack<char>();
            for (int i = 0; i < s.Length; i++)
            {
                var curr = s[i];
                if (expMap.ContainsKey(curr ))
                    stack.Push(expMap[curr]);
                else
                {
                    var top = stack.Pop();
                    if (curr != top) // all is 
                    {
                        return (scoreMap[curr], 0);
                    }
                }
            }

            var res = 0L;
            foreach (var c in stack)
                res = res * 5 + completeMap[c];
            return (0, res);
        }
    }
}