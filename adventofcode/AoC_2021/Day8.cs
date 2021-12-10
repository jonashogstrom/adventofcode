using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2021
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day8 : TestBaseClass<Part1Type, Part2Type>
    {
        private List<char> _allChars = new List<char> { 'a', 'b', 'c', 'd', 'e', 'f', 'g' };
        private List<char> _allSegments = new List<char> { 'A', 'B', 'C', 'D', 'E', 'F', 'G' };
        public bool Debug { get; set; }

        [Test]
        [TestCase(26, 61229, "Day8_test.txt")]
        [TestCase(543, 994266, "Day8.txt")]
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
            foreach (var x in source)
            {
                var px = x.Split('|');
                var parts = px[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var value in parts)
                    if (value.Length == 2 || value.Length == 3 || value.Length == 4 || value.Length == 7)
                        part1++;
            }

            LogAndReset("Parse", sw);
            LogAndReset("*1", sw);
            foreach (var s in source)
                part2 += ParseLine(s);
            LogAndReset("*2", sw);

            return (part1, part2);
        }

        [Test]
        [TestCase("acedgfb cdfbe gcdfa fbcad dab cefabd cdfgeb eafb cagedb ab |cdfeb fcadb cdfeb cdbaf", 5353)]
        [TestCase("be cfbegad cbdgef fgaecd cgeb fdcge agebfd fecdb fabcd edb | fdgacbe cefdb cefbgd gcbe", 8394)]
        public void TestLine(string line, int output)
        {
            Assert.That(ParseLine(line), Is.EqualTo(output));
        }

        private int ParseLine(string line)
        {
            Dictionary<char, HashSet<char>> candidates = new Dictionary<char, HashSet<char>>();

            var segmentsUsed = new Dictionary<int, List<char>>();
            segmentsUsed[0] = new List<char>() { 'A', 'B', 'C', 'E', 'F', 'G' };
            segmentsUsed[1] = new List<char>() { 'F', 'C' };
            segmentsUsed[2] = new List<char>() { 'A', 'C', 'D', 'E', 'G' };
            segmentsUsed[3] = new List<char>() { 'A', 'C', 'D', 'F', 'G' };
            segmentsUsed[4] = new List<char>() { 'B', 'C', 'D', 'F' };
            segmentsUsed[5] = new List<char>() { 'A', 'B', 'D', 'F', 'G' };
            segmentsUsed[6] = new List<char>() { 'A', 'B', 'D', 'E', 'F', 'G' };
            segmentsUsed[7] = new List<char>() { 'A', 'C', 'F' };
            segmentsUsed[8] = new List<char>() { 'A', 'B', 'C', 'D', 'E', 'F', 'G' };
            segmentsUsed[9] = new List<char>() { 'A', 'B', 'C', 'D', 'F', 'G' };

            foreach (var c in _allChars)
            {
                candidates[c] = new HashSet<char>(_allSegments);
            }

            var parts = line.Split('|');
            var unique = parts[0].Trim().Split(' ');
            var output = parts[1].Trim().Split(' ');
            foreach (var value in unique.OrderBy(x => x.Length))
            {
                if (value.Length == 2) // must be 1
                {
                    EliminateCandidates(value, candidates, segmentsUsed[1]);
                    Elim2(value, candidates, segmentsUsed[1]);
                }

                if (value.Length == 3) // must be 7
                {
                    EliminateCandidates(value, candidates, segmentsUsed[7]);
                    Elim2(value, candidates, segmentsUsed[7]);
                }

                if (value.Length == 4) // must be 4
                {
                    EliminateCandidates(value, candidates, segmentsUsed[4]);

                    Elim2(value, candidates, segmentsUsed[4]);
                }

                if (value.Length == 5) // must be 2, 3 or 5
                {
                    var uniqueForFiveSegments = new List<char> { 'G', 'A', 'D' };
                    Elim2(value, candidates, uniqueForFiveSegments);
                }

                if (value.Length == 6) // must be 0, 6, 9
                {
                    var uniqueForSixSegments = new List<char> { 'A', 'B', 'G', 'F' };
                    Elim2(value, candidates, uniqueForSixSegments);
                }
            }

            foreach (var c in _allChars)
                if (candidates[c].Count == 1)
                {
                    Elim2(c.ToString(), candidates, candidates[c].ToList());
                }

            foreach (var c in candidates.Keys)
                Log($"{c} => " + string.Join(",", candidates[c]));

            var res = "";
            foreach (var a in output)
            {
                var possibleNumbers = new HashSet<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                var allCandidates = new HashSet<char>();

                foreach (var c in a)
                {
                    foreach (var x in candidates[c])
                        allCandidates.Add(x);
                }

                for (int i = 0; i < 10; i++)
                {
                    if (segmentsUsed[i].Count != a.Length)
                        possibleNumbers.Remove(i);
                    else
                    {
                        foreach (var segment in segmentsUsed[i])
                        {
                            if (!allCandidates.Contains(segment))
                            {
                                possibleNumbers.Remove(i);
                            }
                        }
                    }
                }

                if (possibleNumbers.Count == 1)
                {
                    res += possibleNumbers.First();
                    Log($"IS unique {a}");
                }
                else
                {
                    Log($"Not unique {a}, could be either " + string.Join(",", possibleNumbers));
                }
            }

            return int.Parse(res);
        }

        private void Elim2(string value, Dictionary<char, HashSet<char>> candidates, IEnumerable<char> charactersUsed)
        {
            foreach (var c in _allChars)
                if (!value.Contains(c))
                {
                    foreach (var x in charactersUsed)
                        candidates[c].Remove(x);
                }
        }

        private void EliminateCandidates(string value, Dictionary<char, HashSet<char>> candidates, List<char> charactersUsed)
        {
            foreach (var c in value)
            {
                var cand = candidates[c];
                foreach (var c1 in _allSegments)
                    if (!charactersUsed.Contains(c1))
                        cand.Remove(c1);
            }
        }
    }
}