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
    class Day05 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(5, 12, "Day05_test.txt")]
        [TestCase(5197, 18605, "Day05.txt")]
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
            List<(Coord c1, Coord c2, bool diagonal)> data = new List<(Coord, Coord, bool)>();
            if (source.Length < 20)
                LogLevel = 20;
            foreach (var s in source)
            {
                var parts = s.Split(' ');
                var c1 = Coord.Parse(parts[0]);
                var c2 = Coord.Parse(parts[2]);
                var diagonal = c1.Col != c2.Col && c1.Row != c2.Row;
                data.Add((c1, c2, diagonal));
            }

            LogAndReset("Parse", sw);

            var board1 = new SparseBuffer<int>();
            // draw the straight lines
            foreach (var coords in data.Where(d => !d.diagonal))
            {
                DrawLine(coords.c1, coords.c2, board1);
            }
            Log(board1.ToString(i => i.ToString()), 20);
            part1 = GetBoardValue(board1);
            LogAndReset("*1", sw);

            // draw the diagonal lines
            foreach (var coords in data.Where(d => d.diagonal))
            {
                DrawLine(coords.c1, coords.c2, board1);
            }
            part2 = GetBoardValue(board1);
            Log(board1.ToString(i => i.ToString()), 20);
            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private static long GetBoardValue(SparseBuffer<int> board)
        {
            var res = 0;
            foreach (var k in board.Keys)
            {
                if (board[k] >= 2)
                    res++;
            }

            return res;
        }

        private void DrawLine(Coord c1, Coord c2, SparseBuffer<int> board)
        {
            Log(() => $"Drawing line {c1} -> {c2}", 20);
            foreach (var c in c1.PathTo(c2))
                board[c]++;
        }
    }
}