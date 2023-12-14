using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2023
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]

    class Day12 : TestBaseClass<Part1Type, Part2Type>
    {
        private List<string> _blocks = new();
        public bool Debug { get; set; }


        [Test]
        [TestCase(21, 525152, "Day12_test.txt")]
        [TestCase(6488, null, "Day12.txt")]
        [TestCase(10, 506250, "?###???????? 3,2,1")] // row 6
        [TestCase(1, 1, "???.### 1,1,3")]
        [TestCase(4, 16384, ".??..??...?##. 1,1,3")]
        [TestCase(1, 1, "?#?#?#?#?#?#?#? 1,3,1,6")]
        [TestCase(1, 16, "????.#...#... 4,1,1")]
        [TestCase(4, 2500, "????.######..#####. 1,6,5")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            LogLevel = resourceName.Contains("test") ? 20 : -1;
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            _blocks.Clear();
            for (int i = 0; i < 20; i++)
                _blocks.Add("".PadLeft(i, '#'));
            Part1Type part1 = 0;
            Part2Type part2 = 0;
            var sw = Stopwatch.StartNew();

            LogLevel = 0;

            LogAndReset("Parse", sw);
            
            foreach (var s in source)
                part1 += Solve1(s);

            LogAndReset("*1", sw);

            var ix = 0;
            foreach (var s in source)
            {
                var res = Solve2(s);
                ix++;
                Log($"{ix}: {s} ==> {res}");
                part2 += res;
            }

            LogAndReset("*2", sw);

            return (part1, part2);
        }
        
        private long Solve1(string s)
        {
            var x = new SplitNode(s, ' ', ',');
            Log(s);
            Log("========");
            var cache = new Cache22<long, string, string>(Recurse2New);

            return cache.Do(x.First.First.Trimmed, x.Second.Trimmed); 
        }

        private long Solve2(string s)
        {
            var x = new SplitNode(s, ' ', ',');
            var answers = x.Second.IntChildren.ToArray();
            var input = x.First.First.Trimmed;
            input = input + '?' + input + '?' + input + '?' + input + '?' + input;
            answers = answers.Concat(answers).Concat(answers).Concat(answers).Concat(answers).ToArray();

            var cache = new Cache22<long, string, string>(Recurse2New);
            var result = cache.Do(input, string.Join(',', answers)); 
            return result; 
        }

        private long Recurse2New(string inputStr, string answerStr, Cache22<long, string, string> cache) // Span<char> input, Span<int> answers)
        {
            var input = inputStr.AsSpan();
            var answers = answerStr.Split(',',StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToArray();
            if (answers.Length == 0) 
                return input.Contains('#') ? 0 : 1;

            if (input.Length == 0)
                return 0;

            while (input[0] == '.')
            {
                input = input[1..];
                if (input.Length == 0)
                    return 0; // out of string
            }

            var res = 0L;
            if (input[0] == '#' || input[0] == '?')
            {
                if (input.Length < answers[0])
                    res += 0;
                else if (input[..answers[0]].Contains('.'))
                    res += 0;
                else if (input.Length == answers[0] && answers.Length == 1)
                {
                    res += 1;
                }
                else if (input.Length <= answers[0] || input[answers[0]] == '#')
                    res += 0;
                else
                    res += cache.Do(input[(answers[0] + 1)..].ToString(), string.Join(',',answers[1..])); //, resultStr + "#".PadLeft(answers[0], '#')+".");
            }
            if (input[0] == '?') // pretend the ? is a .
            {
                res += cache.Do(input[1..].ToString(), answerStr);
            }

            return res;
        }
    }
}