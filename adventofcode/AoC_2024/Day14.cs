using System;
using System.Diagnostics;
using NUnit.Framework;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using AdventofCode.Utils;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;


namespace AdventofCode.AoC_2024
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day14 : TestBaseClass<Part1Type, Part2Type>
    {
        private int _rows;
        private int _cols;
        public bool Debug { get; set; }

        [Test]
        [TestCase(12, null, "Day14_test.txt", 11, 7)]
        [TestCase(233709840, null, "Day14.txt", 101, 103)]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName, int cols, int rows)
        {
            _cols = cols;
            _rows = rows;
            LogLevel = resourceName.Contains("test") ? 20 : -1;
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1 = 0;
            Part2Type part2 = 0;
            var sw = Stopwatch.StartNew();

            var robots = new List<Robot>();
            foreach (var line in source)
            {
                // p=0,4 v=3,-3
                var parts = line.Split([' ', '=']);
                robots.Add(new Robot(
                    CoordStr.Parse(parts[1]),
                    CoordStr.Parse(parts[3]),
                    _rows, _cols));
            }

            LogRobots(robots, 0);

            LogAndReset("Parse", sw);

            // solve part 1 here

            for (int i = 0; i < 100000; i++)
            {
                foreach (var r in robots)
                    r.MoveOnce();
                LogRobots(robots, i+1);
                
                
                if (i == 99)
                {
                    List<int> quadrants = [0, 0, 0, 0, 0];
                    foreach (var r in robots)
                        quadrants[r.CurrentQuadrant()]++;
                    part1 = quadrants[0] * quadrants[1] * quadrants[2] * quadrants[3];
                }
            }

            
            LogAndReset("*1", sw);


            // solve part 2 here

            
            
            LogAndReset("*2", sw);

            return (part1, part2);
        }
        
        

        private void LogRobots(List<Robot> robots, int generation)
        {
            var score = 0;
            var allRobots = robots.Select(r => r.Position).ToHashSet();
            foreach (var robot in robots)
                score += robot.Position.GenAdjacent8().Count(n => allRobots.Contains(n));
            if (score < 300)
                return;
            string filePath = $"day14images/score_{score:00000}_gen_{generation:00000}.png";
                
            if (!File.Exists(filePath))
            {
                if (!Directory.Exists("day14images"))
                    Directory.CreateDirectory("day14images");
                // Create a black bitmap
                using (Bitmap bitmap = new Bitmap(_cols, _rows))
                {
                    // Set the whole bitmap to black
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        if (score>100)
                            g.Clear(Color.Black);
                        else
                        {
                            g.Clear(Color.Red);
                            
                        }

                        
                        
                    }

                    foreach (Robot robot in robots)
                        bitmap.SetPixel(robot.Position.Col, robot.Position.Row, Color.White);

                    // Save the bitmap as a PNG
                    bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                }
            }

            // var map = new SparseBufferStr<int>();
            // foreach (var r in robots)
            //     map[r.Position]++;
            // Log(map.ToString((i, c) => i == 0 ? "." : i.ToString()), LogLevel);
        }

        internal class Robot(CoordStr initPos, CoordStr direction, int rows, int cols)
        {
            private readonly CoordStr _initPos = initPos;
            public CoordStr Position { get; private set; } = initPos;
            private readonly CoordStr _direction = direction;
            private readonly int _rows = rows;
            private readonly int _cols = cols;

            public void MoveOnce()
            {
                var r = Position.Row + _direction.Row;
                if (r < 0) r += _rows;
                if (r >= _rows) r -= _rows;
                var c = Position.Col + _direction.Col;
                if (c < 0) c += _cols;
                if (c >= _cols) c -= _cols;
                var p = new CoordStr(r, c);
                Position = p;
            }

            public int CurrentQuadrant()
            {
                if (Position.Row == _rows / 2 || Position.Col == _cols / 2)
                    return 4;
                var rowQ = Position.Row > _rows / 2 ? 1 : 0;
                var colQ = Position.Col > _cols / 2 ? 1 : 0;
                return rowQ * 2 + colQ;
            }
        }
        
    }
}