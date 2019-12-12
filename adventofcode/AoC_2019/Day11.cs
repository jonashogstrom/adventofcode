using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms.VisualStyles;
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
        [TestCase(2392, null, "Day11.txt")]
        public void Test1(Part1Type exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = Compute(source);
            DoAsserts(res, exp1, exp2);
        }

        private (Part1Type Part1, Part2Type Part2) Compute(string[] source)
        {
            Log($"============== Part1 ================");
            var hull1 = RunRobot(source, 0);
            Log($"============== Part2 ================");
            var hull2 = RunRobot(source, 1);

            Log($"Answer, part 2 (painted cells: {hull2.Keys.Count()}");
            PrintHull(hull2);

            return (hull1.Keys.Count(), 0);
        }

        private SparseBuffer<int> RunRobot(string[] source, int initialColor)
        {
            var brain = new IntCodeComputer(source[0]);
            var robot = Coord.Origin;
            var robotDir = Coord.N;
            var hull = new SparseBuffer<int> { [robot] = initialColor };
            while (!brain.Terminated)
            {
                brain.AddInput(hull[robot]);
                brain.Execute();
                hull[robot] = (int)brain.OutputQ.Dequeue();
                robotDir = brain.OutputQ.Dequeue() == 0 ? robotDir.RotateCCW90() : robotDir.RotateCW90();
                robot = robot.Move(robotDir);
                PrintHull(hull, robot, robotDir);
            }

            Log("Executed operations: " + brain.OpCounter);

            PrintHull(hull, robot, robotDir);

            return hull;
        }

        private void PrintHull(SparseBuffer<int> hull, Coord robot=null, Coord robotDir=null)
        {
            Log(hull.ToString((value, coord) =>
            {
                if (robot != null && coord.Equals(robot))
                    return Coord.trans2Arrow[robotDir].ToString();
                return value == 0 ? " " : "█";
            }));
        }
    }
}