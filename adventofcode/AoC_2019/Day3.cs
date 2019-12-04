using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Windows.Forms;

namespace AdventofCode.AoC_2019
{
    class Day3 : BaseDay
    {
        protected override void Setup()
        {
            Source = InputSource.test;
            Source = InputSource.prod;

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
            var start = new Coord(0, 0);

            var board = new SparseBuffer<int>();

            Log(input[0]);
            ExecutePath(start, input[0].Split(','), board, (pos, dist) =>
            {
                board[pos] = dist;
                //Log($"X: {pos.X} Y: {pos.Y}");
                //Log(board.ToString(c => c != 0 ? "X" : " "));
            });

            var closestManhattan = int.MaxValue;
            var closestSignal = int.MaxValue;
            ExecutePath(start, input[1].Split(','), board, (pos, dist) =>
            {
                if (board[pos] != 0)
                {
                    closestManhattan = Math.Min(closestManhattan, start.Dist(pos));
                    closestSignal = Math.Min(closestSignal, dist + board[pos]);
                }
                else
                {
                    board[pos] = -dist;
                }
            });

//            File.WriteAllText("Day3-2019.txt", board.ToString(c => c > 0 ? "X" : (c < 0? "Y": " ")));

            Part1 = closestManhattan;
            Part2 = closestSignal;
        }

        private static void ExecutePath(Coord start, string[] path1, SparseBuffer<int> board, Action<Coord, int> action)
        {
            var pos = start;
            var dist = 0;
            foreach (var p in path1.Select(s => (dir: s[0], dist: int.Parse(s.Substring(1)))))
            {
                var dir = Coord.trans2Coord[p.dir];
                for (var i = 0; i < p.dist; i++)
                {
                    dist++;
                    pos = pos.Move(dir);
                    action(pos, dist);
                }
            }
        }

        private static (char dir, int dist) ParseMove(string s)
        {
            return (s[0], int.Parse(s.Substring(1)));
        }
    }
}