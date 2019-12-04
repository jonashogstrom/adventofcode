using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AdventofCode.AoC_2018
{
    internal class Day10 : BaseDay
    {
        protected override void Setup()
        {
            UseTestData = false;
            Part1TestSolution = null;
            Part2TestSolution = 3;
            Part1Solution = null;
            Part2Solution = 10333;
        }

        protected override void DoRun(string[] input)
        {
            var positions = new Point[input.Length];
            var speeds = new Point[input.Length];
            var minTimeToCenter = int.MaxValue;
            for (int i = 0; i < input.Length; i++)
            {

                var ints = GetIntArr(input[i], true);
                positions[i] = new Point(ints[0], ints[1]);
                speeds[i] = new Point(ints[2], ints[3]);
                if (speeds[i].X > 0 && speeds[i].Y > 0)
                {
                    minTimeToCenter = Math.Min(minTimeToCenter, Math.Abs(positions[i].X / speeds[i].X));
                    minTimeToCenter = Math.Min(minTimeToCenter, Math.Abs(positions[i].Y / speeds[i].Y));
                }
            }

            for (int i = 0; i < input.Length; i++)
            {
                positions[i].X += speeds[i].X * minTimeToCenter;
                positions[i].Y += speeds[i].Y * minTimeToCenter;
            }

            int minxSize = int.MaxValue;
            int minySize = int.MaxValue;
            var seconds = minTimeToCenter;
            var lastPositions = positions;
            while (true)
            {
                for (int i = 0; i < input.Length; i++)
                {
                    positions[i].X += speeds[i].X;
                    positions[i].Y += speeds[i].Y;
                }

                seconds++;
                var xmin = positions.Min(p => p.X);
                var xmax = positions.Max(p => p.X);
                var ymin = positions.Min(p => p.Y);
                var ymax = positions.Max(p => p.Y);

                var xsize = xmax - xmin;
                var ysize = ymax - ymin;
                if (xsize > minxSize || ysize > minySize)
                {
                    var lastMsg = new StringBuilder();
                    xmin = lastPositions.Min(p => p.X);
                    xmax = lastPositions.Max(p => p.X);
                    ymin = lastPositions.Min(p => p.Y);
                    ymax = lastPositions.Max(p => p.Y);
                    xsize = xmax - xmin + 1;
                    ysize = ymax - ymin + 1;

                    var sky = new char[ysize][];
                    for (int row = 0; row < ysize; row++)
                    {
                        sky[row] = new char[xsize];
                        for (var col = 0; col < xsize; col++)
                            sky[row][col] = ' ';
                    }

                    for (int i = 0; i < input.Length; i++)
                    {
                        sky[lastPositions[i].Y - ymin][lastPositions[i].X - xmin] = 'X';
                    }

                    for (int row = 0; row < ysize; row++)
                        lastMsg.AppendLine((new string(sky[row]).TrimEnd()));

                    var x = lastMsg.ToString();

                    Log(lastMsg.ToString());
                    Part2 = seconds - 1;
                    return;
                }

                minxSize = Math.Min(minxSize, xsize);
                minySize = Math.Min(minySize, ysize);
                lastPositions = positions.Select(p => new Point(p.X, p.Y)).ToArray();
            }

        }
    }

    [DebuggerDisplay("{X},{Y}")]
    internal class Point
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}