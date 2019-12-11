using System;
using System.Collections.Generic;
using AdventofCode.AoC_2018;
using NUnit.Framework;

namespace AdventofCode.AoC_2019
{
    using Part1Type = Int32;
    using Part2Type = Int32;

    [TestFixture]
    class Day11 : TestBaseClass<Part1Type, Part2Type>
    {
        [Test]
        //[TestCase(-1, null, "Day11_test.txt")]
        [TestCase(2392, null, "Day11.txt")]
        public void Test1(Part1Type exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = Compute(source);
            DoAsserts(res, exp1, exp2);
        }

        private (Part1Type Part1, Part2Type Part2) Compute(string[] source)
        {

            runRobot(source, out var paintedCoords1, out var hull1, 0);
            runRobot(source, out var paintedCoords2, out var hull2, 1);

            Log("Answer, part 2");
            Log(hull2.ToString(x =>
            {
                if (x == 0) return " ";
                if (x == 1) return "#";
                return "?";
            }));
            return (paintedCoords1.Count, 0);
        }

        private static Coord runRobot(string[] source, out HashSet<Coord> paintedCoords, out SparseBuffer<int> hull,
            int initialColor)
        {
            var brain = new IntCodeComputer(new List<long>(), source[0]);
            var robot = new Coord(0, 0);
            var robotdir = Coord.N;
            paintedCoords = new HashSet<Coord>();
            hull = new SparseBuffer<int>();
            hull[robot] = initialColor;
            while (!brain.Terminated)
            {
                var currentColor = hull[robot];
                brain.AddInput(currentColor);
                brain.Execute();
                var newColor = brain.OutputQ.Dequeue();
                var newDir = brain.OutputQ.Dequeue();
                hull[robot] = (int) newColor;
                paintedCoords.Add(robot);
                if (newDir == 0)
                    robotdir = robotdir.RotateCCW90();
                else if (newDir == 1)
                    robotdir = robotdir.RotateCW90();
                robot = robot.Move(robotdir);
            }

            return robot;
        }
    }
}