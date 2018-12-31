using System;
using System.Text;

namespace adventofcode.AoC_2018
{
    internal class Day17 : BaseDay
    {
        // 44729 too low
        // 44743
        protected int _minCol = int.MaxValue;
        protected int _minRow = int.MaxValue;
        private int _maxCol = int.MinValue;
        private int _maxRow = int.MinValue;
        protected char WaterAtRest = '~';
        protected char Wall = '#';
        protected char Empty = '.';
        protected char Dropping = '|';
        protected char DroppingWithSupport = '!';

        protected override void Setup()
        {
            Source = InputSource.test;
            Source = InputSource.prod;
            LogLevel = UseTestData ? 5 : 1;

            Part1TestSolution = 57;
            Part2TestSolution = 29;
            Part1Solution = 44743;
            Part2Solution = 34172;
        }

        protected override void DoRun(string[] input)
        {
            var world = ParseInput(input);
            if (LogLevel >= 1)
                PrintWorld(world);
            TraceWaterFlow(world, 1, 500);
            if (LogLevel >= 1)
                PrintWorld(world);
            var res = 0;
            var res2 = 0;
            for (int row = _minRow; row <= _maxRow; row++)
                for (int col = _minCol - 1; col <= _maxCol + 1; col++)
                {
                    if (world[row][col] == WaterAtRest || world[row][col] == DroppingWithSupport || world[row][col] == Dropping)
                        res++;
                    if (world[row][col] == WaterAtRest)
                        res2++;

                }
            Part1 = res;
            Part2 = res2;
        }

        private void TraceWaterFlow(char[][] world, int row, int col)
        {
            world[row][col] = Dropping;
            if (LogLevel >= 2)
                PrintWorld(world);
            if (row <= _maxRow)
                TryFlowDown(world, row + 1, col);
            if (world[row + 1][col] == Wall || world[row + 1][col] == WaterAtRest)
            {
                TryFlowHorizontal(world, row, col, +1);
                TryFlowHorizontal(world, row, col, -1);
                if (LogLevel >= 3)
                    PrintWorld(world, row, col);
                if ((world[row][col + 1] == DroppingWithSupport || world[row][col + 1] == Wall) &&
                    (world[row][col - 1] == DroppingWithSupport || world[row][col - 1] == Wall))
                {
                    ExpandSolidWater(world, row, col);
                }

            }
        }

        private void TryFlowDown(char[][] world, int row, int col)
        {
            if (world[row][col] == Empty)
                TraceWaterFlow(world, row, col);
        }

        private void ExpandSolidWater(char[][] world, int row, int col)
        {
            var colx = col - 1;
            while (world[row][colx] == DroppingWithSupport)
            {
                world[row][colx] = WaterAtRest;
                if (LogLevel >= 4)
                    PrintWorld(world);
                colx--;

            }
            colx = col + 1;
            while (world[row][colx] == DroppingWithSupport)
            {
                world[row][colx] = WaterAtRest;
                if (LogLevel >= 4)
                    PrintWorld(world);
                colx++;
            }

            world[row][col] = WaterAtRest;
            if (LogLevel >= 2)
                PrintWorld(world);

        }

        private void TryFlowHorizontal(char[][] world, int row, int col, int xDir)
        {
            if (world[row][col + xDir] == Empty)
            {
                TraceWaterFlow(world, row, col + xDir);
                if (world[row][col + xDir] == DroppingWithSupport)
                {
                    world[row][col] = DroppingWithSupport;
                    if (LogLevel >= 2)
                        PrintWorld(world);
                }
            }
            else if (world[row][col + xDir] == Wall)
            {
                world[row][col] = DroppingWithSupport;
                if (LogLevel >= 3)
                    PrintWorld(world);
            }
        }

        private void PrintWorld(char[][] world, int rowx = -1, int colx = -1)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("===================================");
            sb.AppendLine();
            for (var row = _minRow; row <= _maxRow; row++)
            {
                for (var col = _minCol; col <= _maxCol; col++)
                {
                    if (row == rowx && col == colx)
                        sb.Append("X");
                    else
                        sb.Append(world[row][col]);
                }

                sb.AppendLine();
            }

            Log(() => sb.ToString(), 1);
        }

        private char[][] ParseInput(string[] input)
        {
            foreach (var r in input)
            {
                var values = GetIntArr(r);
                if (r[0] == 'x')
                {
                    _maxCol = Math.Max(_maxCol, values[0]);
                    _minCol = Math.Min(_minCol, values[0]);
                    _minRow = Math.Min(_minRow, values[1]);
                    _maxRow = Math.Max(_maxRow, values[2]);
                }
                else
                {
                    _maxRow = Math.Max(_maxRow, values[0]);
                    _minRow = Math.Min(_minRow, values[0]);
                    _minCol = Math.Min(_minCol, values[1]);
                    _maxCol = Math.Max(_maxCol, values[2]);
                }
            }

            var world = EmptyArr(_maxRow + 3, _maxCol + 3, '.');
            foreach (var r in input)
            {
                var values = GetIntArr(r);
                if (r[0] == 'x')
                {
                    for (int y = values[1]; y <= values[2]; y++)
                        world[y][values[0]] = Wall;
                }
                else
                {
                    for (int x = values[1]; x <= values[2]; x++)
                        world[values[0]][x] = Wall;
                }
            }

            return world;
        }
    }
}