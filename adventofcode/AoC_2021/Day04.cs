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
    class Day04 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(4512, 1924, "Day04_test.txt")]
        [TestCase(10680, 31892, "Day04.txt")]
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
            var groups = source.AsGroups();
            var nums = GetIntArr(groups.First()[0]);
            var boards = groups.Skip(1);
            var bb = new List<Board>();
            foreach (var board in boards)
            {
                var b = new Board(board);
                bb.Add(b);
            }
            LogAndReset("Parse", sw);

            var boardsWon = 0;
            foreach (var num in nums)
            {
                if (part2 == 0)
                {
                    Log($"Eliminating num: {num}");
                    foreach (var b in bb)
                    {
                        if (part2 == 0 && !b.Bingo && b.Eliminate(num))
                        {
                            boardsWon++;
                            Log($"FoundBoard: {b.BoardScore}, {num}");
                            if (part1 == 0)
                            {
                                part1 = b.BoardScore * num;
                                Log($"Part 1 Complete: {b.BoardScore}, {num} = {part1}");
                                LogAndReset("*1", sw);
                            }

                            if (boardsWon == bb.Count)
                            {
                                part2 = b.BoardScore * num;
                                Log($"Part 2 Complete: {b.BoardScore}, {num} = {part2}");
                                LogAndReset("*2", sw);
                            }
                        }
                    }
                }
            }
            return (part1, part2);
        }
    }

    internal class Board
    {
        private readonly Dictionary<int, Coord> _dic = new Dictionary<int, Coord>();
        private int[] _colCount = new int[5];
        private int[] _rowCount = new int[5];
        private bool _bingo;
        private int _boardScore;
        private int _totalMarked;
        private readonly int _totalSum;

        public Board(IList<string> board)
        {
            for (var row = 0; row < 5; row++)
            {
                var strings = board[row].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var numbers = strings.Select(s => int.Parse(s)).ToList();
                for (var col = 0; col < 5; col++)
                {
                    var number = numbers[col];
                    _dic[number] = new Coord(row, col);
                    _totalSum += number;
                }
            }
        }

        public bool Bingo => _bingo;
        public long BoardScore => _boardScore;

        public bool Eliminate(int num)
        {
            if (_dic.TryGetValue(num, out var pos))
            {
                _colCount[pos.Col]++;
                _rowCount[pos.Row]++;
                _totalMarked += num;
                if (_colCount[pos.Col] == 5)
                {
                    _bingo = true;
                    _boardScore = _totalSum - _totalMarked;
                }
                if (_rowCount[pos.Row] == 5)
                {
                    _bingo = true;
                    _boardScore = _totalSum - _totalMarked;
                }
            }
            return _bingo;
        }
    }
}