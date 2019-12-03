using System;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Windows.Forms;

namespace adventofcode.AoC_2019
{
    class Day3 : BaseDay
    {
        protected override void Setup()
        {
            Source = InputSource.test;
            // Source = InputSource.prod;

            LogLevel = UseTestData ? 5 : 0;

            Part1TestSolution = 135;
            Part2TestSolution = 410;

//            Part1TestSolution = 6;
//            Part2TestSolution = 30;

            Part1Solution = 232;
            Part2Solution = 6084;
        }

        protected override void DoRun(string[] input)
        {
            var start = new Coord(0,0);

            var board = new SparseBuffer<int>();
            var path1 = input[0].Split(',');
            var path2 = input[1].Split(',');
            var closestManhattan = int.MaxValue;
            var closestSignal = int.MaxValue;
            var pos = start;
            var counter = 0;
            foreach (var p in path1.Select(ParseMove))
            {
                var dir = Coord.trans2Coord[p.Item1];
                for (int i = 0; i < p.Item2; i++)
                {
                    counter++;
                    pos = pos.Move(dir);
                    board[pos] = counter;
                    
                }
            }

            pos = start;
            counter = 0;

            foreach (var p in path2.Select(ParseMove))
            {
                var dir = Coord.trans2Coord[p.Item1];
                for (int i = 0; i < p.Item2; i++)
                {
                    counter++;
                    pos = pos.Move(dir);
                    if (board[pos] != 0)
                    {
                        closestManhattan = Math.Min(closestManhattan, start.Dist(pos));
                        closestSignal = Math.Min(closestSignal, counter + board[pos]);
                    }
                }
            }

            Part1 = closestManhattan;
            Part2 = closestSignal;
        }

        private static Tuple<char, int> ParseMove(string s)
        {
            return new Tuple<char, int>(s[0], int.Parse(s.Substring(1)));
        }
    }
}