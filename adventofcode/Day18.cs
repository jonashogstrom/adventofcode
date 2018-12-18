using System.Collections.Generic;
using System.Linq;

namespace adventofcode
{
    internal class Day18 : BaseDay
    {
        protected override void Setup()
        {
            Source = InputSource.test;
            Source = InputSource.prod;

            LogLevel = UseTestData ? 5 : 3;

            Part1TestSolution = 1147;
            Part2TestSolution = null;
            Part1Solution = 545600;
            Part2Solution = 202272;
        }

        protected override void DoRun(string[] input)
        {
            var rows = input.Length;
            var cols = input[0].Length;
            var area = Parse(input, rows, cols);

            var values = new Dictionary<int, long>();

            var totalGens = 1000000000;

            var lastCycles = new Queue<int>();
            for (long gen = 1; gen <= totalGens; gen++)
            {
                var treeCount = 0;
                var lumberCount = 0;

                area = Mutate(rows, cols, area);

                for (int row = 0; row < rows; row++)
                    for (int col = 0; col < cols; col++)
                        if (area[row][col] == '|')
                            treeCount++;
                        else if (area[row][col] == '#')
                            lumberCount++;
                var res = lumberCount * treeCount;

                if (gen == 10)
                    Part1 = res;

                var s = "Gen=" + gen + " L=" + lumberCount + " T=" + treeCount + " ResourceValue=" + res + "";

                if (values.ContainsKey(res))
                {
                    var prevGen = values[res];
                    var cycle = (int)(gen - prevGen);
                    s += " Cycle=" + cycle;
                    lastCycles.Enqueue(cycle);
                    while (lastCycles.Count > cycle) lastCycles.Dequeue();
                    if (cycle > 5 && lastCycles.Count == cycle)
                    {
                        var cycleCount = lastCycles.Distinct().Count();
                        if (cycleCount == 1)
                        {
                            var left = totalGens - gen;

                            if (left % cycle == 0)
                            {
                                Log(s);
                                Log("Prediction : " + res);
                                Part2 = res;
                                return;
                            }
                        }
                    }
                }
                Log(s, 2);

                values[res] = gen;
            }
        }

        private char[][] Mutate(int rows, int cols, char[][] area)
        {
            var newArea = EmptyArr(rows, cols, '.');
            for (int row = 0; row < rows; row++)
                for (int col = 0; col < cols; col++)
                {
                    var treecount = 0;
                    var lumbercount = 0;


                    for (var x = -1; x <= +1; x++)
                    {
                        if (col + x >= 0 && col + x < cols)
                        {
                            for (var y = -1; y <= +1; y++)
                            {
                                if (row + y >= 0 && row + y < rows && !(x == 0 && y == 0))
                                {
                                    var c = area[row + y][col + x];
                                    if (c == '|') treecount++;
                                    else if (c == '#') lumbercount++;
                                }
                            }
                        }
                    }

                    var prev = area[row][col];
                    if (prev == '.')
                    {
                        if (treecount >= 3)
                            newArea[row][col] = '|';
                    }
                    else if (prev == '|')
                    {
                        if (lumbercount >= 3)
                            newArea[row][col] = '#';
                        else
                            newArea[row][col] = '|';
                    }
                    else if (prev == '#')
                    {
                        if (lumbercount >= 1 && treecount >= 1)
                            newArea[row][col] = '#';
                        else
                            newArea[row][col] = '.';
                    }
                    else
                    {
                    }
                }

            return newArea;
        }


        private static char[][] Parse(string[] input, int rows, int cols)
        {
            var area = EmptyArr(input.Length, input[0].Length, '.');
            // init
            for (int row = 0; row < rows; row++)
                for (int col = 0; col < cols; col++)
                    area[row][col] = input[row][col];
            return area;
        }
    }
}