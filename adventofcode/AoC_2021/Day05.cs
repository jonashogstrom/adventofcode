using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    class Day05 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(5, 12, "Day05_test.txt")]
        [TestCase(5197, 18605, "Day05.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1 = 0;
            Part2Type part2 = 0;
            var sw = Stopwatch.StartNew();
            List<(Coord c1, Coord c2)> data = new List<(Coord, Coord)>();
            if (source.Length < 20)
                LogLevel = 20;
            foreach (var s in source)
            {
                var parts = s.Split(' ');
                var c1 = Coord.Parse(parts[0]);
                var c2 = Coord.Parse(parts[2]);
                data.Add((c1, c2));
            }

            LogAndReset("Parse", sw);

            var board1 = new SparseBuffer<int>();
            foreach (var coords in data)
            {
                if (coords.c1.Col == coords.c2.Col || coords.c1.Row == coords.c2.Row)
                {
                    DrawLine(coords.c1, coords.c2, board1);
                }
            }
            Log(board1.ToString(i => i.ToString()), 20);
            part1 = GetBoardValue(board1);
            LogAndReset("*1", sw);

            var board2 = new SparseBuffer<int>();
            foreach (var coords in data)
            {
                DrawLine(coords.c1, coords.c2, board2);
            }
            part2 = GetBoardValue(board2);
            Log(board2.ToString(i => i.ToString()), 20);
            LogAndReset("*2", sw);


            // inte 18629;
            return (part1, part2);
        }

        private static long GetBoardValue(SparseBuffer<int> board1)
        {
            var res = 0;
            foreach (var k in board1.Keys)
            {
                if (board1[k] >= 2)
                    res++;
            }

            return res;
        }

        private void DrawLine(Coord c1, Coord c2, SparseBuffer<int> board)
        {
            Log($"Drawing line {c1} -> {c2}", 20);
            foreach(var c in c1.PathTo(c2))
                board[c]++;
        }
    }
}