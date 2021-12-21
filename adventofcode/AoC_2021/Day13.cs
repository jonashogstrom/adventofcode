using System;
using System.Diagnostics;
using System.Linq;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2021
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day13 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(17, null, "Day13_test.txt")]
        [TestCase(18968, null, "Day13_large.txt")]
        [TestCase(669, null, "Day13.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            //LogLevel = resourceName.Contains("test") ? 20 : -1;
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
            var paper = new SparseBufferL<char>('.');
            foreach (var s in g.First())
            {
                var c = CoordL.Parse(s);
                paper[c] = '#';
            }
            LogAndReset("Parse", sw);
            var instructions = g.Last();
            paper = Fold(instructions.First(), paper);

            part1 = paper.Keys.Count();
            // not 90
            LogAndReset("*1", sw);
            foreach (var f in instructions.Skip(1))
            {
                paper = Fold(f, paper);
            }
//            Log(paper.ToString(c => c.ToString()));

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private SparseBufferL<char> Fold(string s, SparseBufferL<char> paper)
        {
            var parts = s.Split(new[] { ' ', '=' });
            var val = long.Parse(parts[3]);

            Func<CoordL, CoordL> foldFunc;
            if (parts[2] == "x")
                foldFunc = c => new CoordL(c.Row, c.Col < val ? c.Col : 2 * val - c.Col);
            else
                foldFunc = c => new CoordL(c.Row < val ? c.Row : 2 * val - c.Row, c.Col);

            var newPaper = new SparseBufferL<char>('.');
            foreach (var k in paper.Keys.ToArray())
            {
                var newCoord = foldFunc(k);
                newPaper[newCoord] = paper[k];
            }

            return newPaper;
        }
    }
}