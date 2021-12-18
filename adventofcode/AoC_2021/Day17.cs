using System;
using System.Diagnostics;
using NUnit.Framework;

namespace AdventofCode.AoC_2021
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day17 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(45, 112, "Day17_test.txt")]
        [TestCase(4753, 1546, "Day17.txt")]
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
            // target area: x=20..30, y=-10..-5
            var p = source[0].Split(new[] { ",", "..", "=" }, StringSplitOptions.RemoveEmptyEntries);

            var x1 = int.Parse(p[1]);
            var x2 = int.Parse(p[2]);
            var y1 = int.Parse(p[4]);
            var y2 = int.Parse(p[5]);
            var r = new Rect(x1, y1, x2, y2);
            LogAndReset("Parse", sw);


            var bestyVel = int.MinValue;

            var minXVelocity = 1;
            var xSum = 1;
            while (xSum < r.Left)
            {
                minXVelocity++;
                xSum += minXVelocity;
            }
            var yVel = r.Bottom;
            part1 = int.MinValue;
            var highestY = int.MinValue;
    
            while (TryThrow(minXVelocity, yVel, r).res != ThrowResult.WayOver && yVel < -r.Bottom)
            {
                var xVel = minXVelocity;
                
                var res = TryThrow(xVel, yVel, r);
                while (res.res == ThrowResult.Miss)
                {
                    xVel++;
                    res = TryThrow(xVel, yVel, r);
                }

                if (res.res == ThrowResult.Hit)
                {
                    part1 = Math.Max(part1, res.maxY);
                    highestY = yVel;
                }

                yVel++;
            }

            LogAndReset("*1", sw);
            yVel = r.Bottom;
            while (TryThrow(minXVelocity, yVel, r).res != ThrowResult.WayOver && yVel < -r.Bottom)
            {
                for (var xvel2 = minXVelocity; xvel2 <= r.Right; xvel2++)
                {
                    var res2 = TryThrow(xvel2, yVel, r);
                    if (res2.res == ThrowResult.Hit)
                        part2++;
                    if (res2.res == ThrowResult.WayOver)
                        break;
                }
                yVel++;
            }

            Log($"Highest Y with a hit: {highestY}");
            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private (ThrowResult res, int maxY, int hits) TryThrow(int xVel, int yVel, Rect rect)
        {
            var x = 0;
            var y = 0;
            var maxY = int.MinValue;
            var hits = 0;
            while (y >= rect.Bottom && x <= rect.Right)
            {
                maxY = Math.Max(y, maxY);
                if (x >= rect.Left && x <= rect.Right && y <= rect.Top && y >= rect.Bottom)
                    hits++;
                x += xVel;
                y += yVel;
                yVel--;
                if (xVel > 0)
                    xVel--;
            }

            if (hits > 0)
                return (ThrowResult.Hit, maxY, hits);
            if (x > rect.Right && y > rect.Top) 
                return (ThrowResult.WayOver, -1, -1);
            return (ThrowResult.Miss, -1, -1);
        }
    }

    internal enum ThrowResult
    {
        Hit,
        WayOver,
        Miss
    }

    internal class Rect
    {
        public int Left { get; }
        public int Bottom { get; }
        public int Right { get; }
        public int Top { get; }

        public Rect(int left, int bottom, int right, int top)
        {
            Left = left;
            Bottom = bottom;
            Right = right;
            Top = top;
        }
    }
}