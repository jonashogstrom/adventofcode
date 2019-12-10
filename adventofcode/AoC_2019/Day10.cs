using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using AdventofCode.AoC_2018;
using NUnit.Framework;

namespace AdventofCode.AoC_2019
{
    class Day10 : BaseDay
    {
        [Test]
        [TestCase(3, 4, 8, null, "_test")]
        [TestCase(11, 13, 210, 802,"_test2")]
        [TestCase(26, 28, 267, 1309, "")]
        public void Test1(int expX, int expY, int expPart1, int? expPart2, string suffix)
        {
            var source = GetResource(suffix);
            var res = Compute(source);

            Assert.That(res.Part1.X, Is.EqualTo(expX));
            Assert.That(res.Part1.Y, Is.EqualTo(expY));
            Log("Part1: " + res.Part1.X + ", "+res.Part1.Y);

            if (expPart2.HasValue)
            {
                Assert.That(res.Part2, Is.EqualTo(expPart2.Value));
                Log("Part2: " + res.Part2);
            }
        }

        private static int GCD(int a, int b)
        {
            while (a != 0 && b != 0)
            {
                if (a > b)
                    a %= b;
                else
                    b %= a;
            }

            return a == 0 ? b : a;
        }
        private (Coord Part1, int Part2) Compute(string[] source)
        {
            var bestRow = -1;
            var bestCol = -1;

            var board = new SparseBuffer<bool>();
            var asteroids = new HashSet<Coord>();
            for (var row = 0; row < source.Length; row++)
            {
                for (var col = 0; col < source[row].Length; col++)
                    if (source[row][col] == '#')
                        asteroids.Add(new Coord(row, col));
            }

            var maxVisible = 0;
            Coord bestLocation = null;
            var bestangles = new Dictionary<Coord, double>();
            foreach (var a in asteroids)
            {
                var visible = new HashSet<Coord>(asteroids);
                visible.Remove(a);
                var angles = new Dictionary<Coord, double>();
                foreach (var v in visible.ToArray())
                {
                    var xDiff = v.X - a.X;
                    var yDiff = v.Y - a.Y;
                    var x2 = 0;
                    var y2 = 0;
                    double angleforVisible = Math.Atan2(yDiff, xDiff) * 180 / Math.PI;
                    angleforVisible += 90;
                    if (angleforVisible >= 360)
                        angleforVisible -= 360;
                    if (angleforVisible < 0)
                        angleforVisible += 360;

                    if (xDiff == 0)
                    {
                        y2 = yDiff > 0 ? 1 : -1;
                    }
                    else
                    if (yDiff == 0)
                    {
                        x2 = xDiff > 0 ? 1 : -1;
                    }
                    else
                    {
                        var gcd = GCD(Math.Abs(xDiff), Math.Abs(yDiff));
                        x2 = xDiff / gcd;
                        y2 = yDiff / gcd;
                        var dist = Math.Sqrt(Math.Pow(xDiff, 2) + Math.Pow(yDiff, 2));
                    }

                    angles[v] = angleforVisible;

                    var x = v.X;
                    var y = v.Y;
                    while (x >= 0 && x <= source[0].Length &&
                           y >= 0 && y <= source.Length)
                    {
                        x += x2;
                        y += y2;
                        visible.Remove(new Coord(y, x));
                    }
                }

                if (visible.Count > maxVisible)
                {
                    maxVisible = visible.Count;
                    bestLocation = a;
                    bestangles = angles;
                }
            }

            var visible2 = new HashSet<Coord>(asteroids);
            visible2.Remove(bestLocation);
            var targetangle = bestangles.Values.Min();
            var counter = 0;

            Log($"Location: {bestLocation.X},{bestLocation.Y}");

            var p2 = -1;
            while (visible2.Any())
            {
                Coord next = null;
                double? nearestAngle = null;
                var targets = visible2.Where(x => bestangles[x] == targetangle).ToArray();

                var nearestTarget = targets.OrderBy(t => t.Dist(bestLocation)).First();
                visible2.Remove(nearestTarget);
                var nonTargets = visible2.ToArray();

                counter++;
                Log($"{counter} Destroying {nearestTarget.X},{nearestTarget.Y}");
                if (counter == 200)
                    p2 = nearestTarget.X * 100 + nearestTarget.Y;


                double smallestDiff = 360;
                double nextTargetAngle = targetangle; // just in case we have to loop 360 degrees
                foreach (var x in nonTargets)
                {
                    if (bestangles[x] != targetangle)
                    {
                        var angleDiff = bestangles[x] - targetangle;
                        if (angleDiff < 0)
                            angleDiff += 360;
                        if (angleDiff < smallestDiff)
                        {
                            smallestDiff = angleDiff;
                            nextTargetAngle = bestangles[x];
                        }

                    }
                }

                targetangle = nextTargetAngle;
            }

            var angle = 0;

            return (bestLocation, p2);
        }

        protected override void Setup()
        {
            //            Source = InputSource.test;
            //            //Source = InputSource.prod;
            //
            //            LogLevel = UseTestData ? 5 : 0;
            //
            //            Part1TestSolution = null;
            //            Part2TestSolution = null;
            //            Part1Solution = null;
            //            Part2Solution = null;
        }

        protected override void DoRun(string[] input)
        {
            //            var res = Compute(input);
            //            Part1 = res.Part1;
            //
            //            Part2 = res.Part2;
        }
    }
}