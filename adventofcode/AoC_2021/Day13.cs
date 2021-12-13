using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
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
            var paper = new SparseBuffer<char>('.');
            foreach (var s in g.First())
            {
                var c = Coord.Parse(s);
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
            Log(paper.ToString(c=>c.ToString()));

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private SparseBuffer<char> Fold(string s, SparseBuffer<char> paper)
        {
            var parts = s.Split(new[] { ' ', '=' });
            var val = int.Parse(parts[3]);
            Func<Coord, Coord> foldFunc;
            if (parts[2] == "x")
                foldFunc = c => new Coord(c.Row, c.Col < val ? c.Col : 2*val - c.Col);
            else
            {
                foldFunc = c => new Coord(c.Row < val ? c.Row : 2*val - c.Row, c.Col);
            }

            var newPaper = new SparseBuffer<char>('.');
            foreach (var k in paper.Keys)
            {
                var newCoord = foldFunc(k);
                newPaper[newCoord] = paper[k];

            }

            return newPaper;
        }
    }
}