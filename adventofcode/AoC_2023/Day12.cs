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
                Log($"{ix}: {s} ==> {res}", -1);
                part2 += res;
            }

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private long Solve2(string s)
        {
            var x = new SplitNode(s, ' ', ',');
            var res = x.Second.IntChildren.ToArray();
            var input = x.First.First.Trimmed;
            input = input + '?' + input + '?' + input + '?' + input + '?' + input;
            res = res.Concat(res).Concat(res).Concat(res).Concat(res).ToArray();
            var groups = input.Split('.', StringSplitOptions.RemoveEmptyEntries);

            var cache = new Cache22<long, string, string>(Recurse2New);
            var result = cache.Do(input, string.Join(',', res)); //x.Second  => Recurse2New(p, cache1));
            return result; 

            //return Recurse2(groups, res);
        }

        public ref struct RecurseParam
        {
            public ReadOnlySpan<char> input;
            public ReadOnlySpan<int> ansers;
        }

        private long Recurse2New(string inputStr, string answerStr, Cache22<long, string, string> cache) // Span<char> input, Span<int> answers)
        {
            var input = inputStr.AsSpan();
            //Log(answerStr, -1);
            var answers = answerStr.Split(',',StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToArray();
            if (answers.Length == 0)
                if (input.Contains('#'))
                {
                    return 0;
                }
                else
                {
  //                  Log(resultStr + inputstr.Replace('?', '.'));
                    return 1;
                }

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
    //                Log(resultStr + inputstr.Replace('?', '#'));
                    res += 1;
                }
                else if (input.Length <= answers[0] || input[answers[0]] == '#')
                    res += 0;
                else
                    res += cache.Do(input[(answers[0] + 1)..].ToString(), string.Join(',',answers[1..])); //, resultStr + "#".PadLeft(answers[0], '#')+".");
            }
            if (input[0] == '?') // pretend the ? is a .
            {
                res += cache.Do(input[1..].ToString(), answerStr) ; //, resultStr + ".");
            }

            return res;
        }

        private long Recurse2(Span<string> groups, Span<int> answer)
        {
            if (groups.Length == 1 && answer.Length == 1)
            {
                return Recurse1(groups[0], answer.ToArray());
            }

            if (groups.Length == 0 || answer.Length == 0)
            {
                return 0;
            }
            var maxAnswer = Max(answer);

            if (MaxLen(groups) < maxAnswer)
                return 0;

            if (SumLen(groups) < Sum(answer))
                return 0;

            var maxAnswers = FindMax(answer, maxAnswer).ToArray();
            var maxAvailableStrings = FindMaxOrLonger(groups, maxAnswer).ToArray();
            if (maxAnswers.Length > maxAvailableStrings.Length)
                return 0;
            if (maxAnswers.Length == maxAvailableStrings.Length)
            {
                var res = 1L;
                var groupPos = 0;
                var answerPos = 0;
                for (int i = 0; i < maxAnswers.Length; i++)
                {
                    if (answerPos < maxAnswers[i] && groupPos < maxAvailableStrings[i])
                        res *= Recurse2(groups[groupPos..maxAvailableStrings[i]], answer[answerPos..maxAnswers[i]]);
                    res *= Recurse2(groups[maxAvailableStrings[i]..(maxAvailableStrings[i] + 1)],
                        answer[maxAnswers[i]..(maxAnswers[i] + 1)]);
                    groupPos = maxAvailableStrings[i] + 1;
                    answerPos = maxAnswers[i] + 1;
                }
                if (answerPos < answer.Length - 1 && groupPos < groups.Length - 1)
                    res *= Recurse2(groups[groupPos..groups.Length], answer[answerPos..answer.Length]);
                return res;
            }

            var lastGroup = groups[^1];
            var firstGroup = groups[0];

            // check if first (or last) matches perfectly
            if (firstGroup.Length == answer[0] && !firstGroup.Contains('?'))
            {
                if (groups.Length == 1)
                    return 1;
                return Recurse2(groups[1..], answer[1..]);
            }

            if (lastGroup.Length == answer[^1] && !lastGroup.Contains('?'))
            {
                if (groups.Length == 1)
                    return 1;
                return Recurse2(groups[..^1], answer[..^1]);
            }

            // check if first (or last) can't match at all
            if (firstGroup.Length < answer[0])
            {
                if (firstGroup.Contains('#'))
                    return 0;

                return Recurse2(groups[1..], answer);
            }

            if (lastGroup.Length < answer[^1])
            {
                if (lastGroup.Contains('#'))
                    return 0;
                return Recurse2(groups[..^1], answer);
            }

            if (firstGroup.StartsWith(_blocks[answer[0] + 1]))
                return 0;

            // firstGroup can only contain first answer?
            if (answer.Length > 1 && firstGroup.Contains('#') && firstGroup.Length < answer[0] + answer[1] + 1)
            {
                return Recurse2(groups[..1], answer[..1]) *
                       Recurse2(groups[1..], answer[1..]);
            }
            return SplitAndRecurse(groups, answer);
        }

        private int Max(Span<int> values)
        {
            var max = -1;
            foreach (var v in values)
            {
                max = Math.Max(max, v);
            }

            return max;
        }

        private int MaxLen(Span<string> strings)
        {
            var max = -1;
            foreach (var s in strings)
            {
                max = Math.Max(max, s.Length);
            }

            return max;
        }
        private int Sum(Span<int> values)
        {
            var sum = 0;
            foreach (var v in values)
            {
                sum += v;
            }

            return sum;
        }
        private IEnumerable<int> FindMax(Span<int> values, int c)
        {
            var res = new List<int>();
            for (var index = 0; index < values.Length; index++)
            {
                if (values[index] == c)
                    res.Add(index);
            }

            return res;
        }
        private IEnumerable<int> FindMaxOrLonger(Span<string> values, int c)
        {
            var res = new List<int>();
            for (var index = 0; index < values.Length; index++)
            {
                if (values[index].Length >= c)
                    res.Add(index);
            }

            return res;
        }

        private int SumLen(Span<string> strings)
        {
            var sum = 0;
            foreach (var s in strings)
            {
                sum += s.Length;
            }

            return sum;
        }

        private long SplitAndRecurse(Span<string> input, Span<int> answer)
        {
            var unknownGroups = input.ToArray().SelectMany(s => s.Split('#', StringSplitOptions.RemoveEmptyEntries)).ToArray();
            var maxLength = unknownGroups.Select(x => x.Length).Max();
            // for (int i = 0; i < unknownGroups.Length; i++)
            // {
            //     if (unknownGroups[i].Length == maxLength)
            //     {
            //         var s1 = 
            //     }
            // }
            var splitpos = 1;

            foreach (var g in unknownGroups)
                if (g.Length < maxLength)
                {
                    splitpos += g.Length;
                }
                else
                {
                    splitpos += g.Length / 2;
                    break;
                }

            for (int x = 0; x < input.Length; x++)
            {
                var inp = input[x];
                for (int i = 0; i < inp.Length; i++)
                {
                    if (inp[i] == '?')
                        splitpos--;
                    if (splitpos == 0)
                    {


                        var s = input[x];
                        var s1 = new[] { ReplaceString(s, i, '#') };
                        var s2 = new[] { s.Substring(0, i), s.Substring(i + 1) }.Where(str => str != "");

                        var tempArray = input.ToArray();
                        var input1 = tempArray.Take(x).Concat(s1).Concat(tempArray.Skip(x + 1)).ToArray();
                        var input2 = tempArray.Take(x).Concat(s2).Concat(tempArray.Skip(x + 1)).ToArray();


                        return Recurse2(input2, answer) + Recurse2(input1, answer);
                    }
                }
            }

            throw new Exception();
        }


        private long Solve1(string s)
        {
            var x = new SplitNode(s, ' ', ',');
            var res = x.Second.IntChildren.ToArray();

            var input = x.First.First.Trimmed.ToCharArray();
            Log(s);
            Log("========");
            var cache = new Cache22<long, string, string>(Recurse2New);

            return cache.Do(x.First.First.Trimmed, x.Second.Trimmed); //Recurse2New(input, res); //, "");
        }

        private long Recurse1(string input, int[] res)
        {
            var pos = input.IndexOf('?');
            if (pos == -1)
            {
                var temp = input.Split('.', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Length).ToArray();
                return Enumerable.SequenceEqual(res, temp) ? 1 : 0;
            }
            var s1 = ReplaceString(input, pos, '#');
            var s2 = ReplaceString(input, pos, '.');
            Assert.That(s1.Length, Is.EqualTo(input.Length));
            Assert.That(s2.Length, Is.EqualTo(input.Length));
            return Recurse1(s1, res) + Recurse1(s2, res);
        }

        private static string ReplaceString(string input, int pos, char c)
        {
            if (pos == 0)
                return c + input.Substring(1);
            if (pos == input.Length - 1)
                return input.Substring(0, pos) + c;
            return input.Substring(0, pos) + c + input.Substring(pos + 1);
        }
    }

}