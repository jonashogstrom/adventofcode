using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using NUnit.Framework;

namespace AdventofCode.AoC_2019
{
    using Part1Type = Int32;
    using Part2Type = Int32;

    [TestFixture]
    class Day24 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(2129920, 99, "Day24_test.txt", 10)]
        [TestCase(23846449, 1934, "Day24.txt", 200)]
        public void Test1(Part1Type exp1, Part2Type? exp2, string resourceName, int generations)
        {
            var source = GetResource(resourceName);
            int? part1 = Compute(source);
            int? part2 = ComputePart2(source, generations);
            DoAsserts((part1, part2), exp1, exp2);
        }

        [Test]
        public void TestBio()
        {
            var x = new SparseBuffer<char>();
            x[Coord.FromXY(0, 3)] = '#';
            x[Coord.FromXY(1, 4)] = '#';
            x[Coord.FromXY(0, 0)] = '.';
            x[Coord.FromXY(4, 4)] = '.';
            Assert.That(BioDiversity(x), Is.EqualTo(2129920));

        }

        private Part2Type ComputePart2(string[] source, int generations)
        {
            var initLevel0 = Parse(source);
            var boards = new Dictionary<int, SparseBuffer<char>>();
            boards[0] = initLevel0;

       //     PrintAllCellsAndCount(boards);

            for (int i = 0; i < generations; i++)
            {
                boards = Mutate2(boards, i);
                /*
                Log("************************************************");
                Log("Generation: "+(i+1));
                PrintAllCellsAndCount(boards);
            */
            }

            var sumCells = PrintAllCellsAndCount(boards);
            return sumCells;
        }

        private int PrintAllCellsAndCount(Dictionary<int, SparseBuffer<char>> boards)
        {
            var sumCells = 0;

            foreach (var level in boards.Keys.OrderBy(k => k))
            {
                Log($"=========");
                Log($"Level: {level}");
                var board = boards[level];

                var boardCells = 0;
                foreach (var k in board.Keys)
                    if (board[k] == '#')
                        boardCells++;
                //if (boardCells > 0)
                Log(board.ToString(c => c.ToString()));
                sumCells += boardCells;
                Log("Cells alive: "+sumCells);
            }

            return sumCells;
        }

        private Dictionary<int, SparseBuffer<char>> Mutate2(Dictionary<int, SparseBuffer<char>> boards, int generation)
        {
            var size = generation / 2 + 1;
            var res = new Dictionary<int, SparseBuffer<char>>();

            for (var level = -size; level <= size; level++)
                res[level] = Mutate2inner(boards, level);

            return res;
        }

        private SparseBuffer<char> Mutate2inner(Dictionary<int, SparseBuffer<char>> boards, int level)
        {
            if (!boards.ContainsKey(level))
                boards[level] = EmptyBoard(5);
            var board = boards[level];
            if (!boards.ContainsKey(level - 1))
                boards[level - 1] = EmptyBoard(5);
            if (!boards.ContainsKey(level + 1))
                boards[level + 1] = EmptyBoard(5);

            var outerBoard = boards[level - 1];

            var innerBoard = boards[level + 1];

            var res = new SparseBuffer<char>();
            foreach (var k in board.Keys)
            {
                if (k.X == 2 && k.Y == 2)
                {
                    res[k] = '?';
                    continue;
                }

                var ownneighbours = k.GenAdjacent4().Count(n => board[n] == '#');
                var innerNeighbours = 0;
                var outerNeighbours = 0;

                if (k.X == 0 && outerBoard[Coord.FromXY(1, 2)] == '#')
                    outerNeighbours++;
                if (k.X == 4 && outerBoard[Coord.FromXY(3, 2)] == '#')
                    outerNeighbours++;
                if (k.Y == 0 && outerBoard[Coord.FromXY(2, 1)] == '#')
                    outerNeighbours++;
                if (k.Y == 4 && outerBoard[Coord.FromXY(2, 3)] == '#')
                    outerNeighbours++;

                if (k.X == 1 && k.Y == 2)
                {
                    for (var y = 0; y < 5; y++)
                        if (innerBoard[Coord.FromXY(0, y)] == '#')
                            innerNeighbours++;
                }
                else if (k.X == 3 && k.Y == 2)
                {
                    for (var y = 0; y < 5; y++)
                        if (innerBoard[Coord.FromXY(4, y)] == '#')
                            innerNeighbours++;

                }
                else if (k.X == 2 && k.Y == 1)
                {
                    for (var x = 0; x < 5; x++)
                        if (innerBoard[Coord.FromXY(x, 0)] == '#')
                            innerNeighbours++;

                }
                else if (k.X == 2 && k.Y == 3)
                {
                    for (var x = 0; x < 5; x++)
                        if (innerBoard[Coord.FromXY(x, 4)] == '#')
                            innerNeighbours++;

                }


                var neighbours = ownneighbours + innerNeighbours + outerNeighbours;


                if (board[k] == '#')
                    res[k] = neighbours == 1 ? '#' : '.';
                else
                    res[k] = (neighbours == 1 || neighbours == 2) ? '#' : '.';
            }

            return res;
        }

        private SparseBuffer<char> EmptyBoard(int i)
        {
            var res = new SparseBuffer<char>();
            for (int x = 0; x < i; x++)
                for (int y = 0; y < i; y++)
                    res[Coord.FromXY(x, y)] = '.';
            return res;
        }


        private Part1Type Compute(string[] source)
        {
            var board = Parse(source);
            var bio = BioDiversity(board);
            var seenBio = new HashSet<int>();
            var minute = 0;
            while (true)
            {
                board = mutate(board);
                bio = BioDiversity(board);
                if (seenBio.Contains(bio))
                    return bio;
                seenBio.Add(bio);
                minute++;
                // Log($"After {minute} minutes");
                // Log(board.ToString(c => c.ToString()));
                // Log($"BioDiversity: {bio}");
            }
        }



        private int BioDiversity(SparseBuffer<char> board)
        {
            var bio = 0;
            foreach (var c in board.Keys)
                if (board[c] == '#')
                {
                    var bioForCell = 1 << c.Row * board.Width + c.Col;
                    bio += bioForCell;
                }

            return bio;
        }

        private SparseBuffer<char> mutate(SparseBuffer<char> board)
        {
            var res = new SparseBuffer<char>();
            foreach (var k in board.Keys)
            {
                var neighbours = k.GenAdjacent4().Count(n => board[n] == '#');
                if (board[k] == '#')
                    res[k] = neighbours == 1 ? '#' : '.';
                else
                    res[k] = (neighbours == 1 || neighbours == 2) ? '#' : '.';
            }

            return res;
        }

        private SparseBuffer<char> Parse(string[] source)
        {
            var res = new SparseBuffer<char>();
            for (int row = 0; row < source.Length; row++)
                for (int col = 0; col < source[row].Length; col++)
                    res[new Coord(row, col)] = source[row][col];
            return res;
        }
    }
}