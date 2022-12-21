using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2019
{
    using Part1Type = Int32;
    using Part2Type = Int64;

    [TestFixture]
    class Day12 : TestBaseClass<Part1Type, Part2Type>
    {
        [Test]
        [TestCase(179, 2772, "Day12_test.txt", 10)]
        [TestCase(1940, 4686774924, "Day12_test2.txt", 100)]
        [TestCase(10028, 314610635824376, "Day12.txt", 1000)]
        public void Test1(Part1Type exp1, Part2Type? exp2, string resourceName, int steps)
        {
            var source = GetResource(resourceName);
            var res = Compute(source, steps);
            DoAsserts(res, exp1, exp2);

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

        private (int Part1, long Part2) Compute(string[] source, int steps)
        {
            var initialMoons = new List<Moon>();
            foreach (var tempStr in source)
            {
                var s = tempStr.Replace("<", "")
                    .Replace(">", "")
                    .Replace("=", "")
                    .Replace("x", "")
                    .Replace("y", "")
                    .Replace("z", "").Split(',').Select(v => int.Parse(v)).ToArray();
                var c = new Coord3d(s[0], s[1], s[2]);
                initialMoons.Add(new Moon() { position = c, velocity = new Coord3d(0, 0, 0) });
            }

            // Part1
            var moons = new List<Moon>(initialMoons.Select(m => m.clone()));

            for (int step = 0; step < steps; step++)
            {
                moons = IterateVelocities(moons, step+1);
            }
            var sumEnergy = 0;
            foreach (var m in moons)
            {
                var pot = m.position.absValue();
                var kin = m.velocity.absValue();
                sumEnergy += pot * kin;
            }

            // part2
            var cycles = FindCycles(initialMoons);
            
            var gcd_xy = GCD(cycles.x, cycles.y);
            var gcd_yz = GCD(cycles.y, cycles.z);
            var gcd_xz = GCD(cycles.x, cycles.z);

            var prod = 1L* cycles.x * cycles.y * cycles.z;

            var gcd = GCD(gcd_xy, gcd_yz);
            long xx = cycles.x / gcd;
            long yy = cycles.y / gcd;
            long zz = cycles.z / gcd;

            long xxx = cycles.x;
            long yyy = cycles.y;
            long zzz = cycles.z;
            while (xxx != yyy || yyy != zzz)
            {
                if (xxx <= yyy && xxx <= zzz)
                    xxx += cycles.x;
                else if (yyy <= xxx && yyy <= zzz)
                    yyy += cycles.y;
                else if (zzz <= xxx && zzz <= yyy)
                    zzz += cycles.z;
                else
                {
                    throw new Exception();
                }
            }

            return (sumEnergy, xxx);
        }

        private Coord3d FindCycles(IReadOnlyList<Moon> initialMoons)
        {
            var moons = new List<Moon>(initialMoons.Select(m => m.clone()));

            var stepCounter = 0;
            var lastx = 0;
            var lasty = 0;
            var lastz = 0;
            var xCycle = -1;
            var yCycle = -1;
            var zCycle = -1;
            while (true)
            {
                stepCounter++;
                moons = IterateVelocities(moons, stepCounter);
                var okx = true;
                var oky = true;
                var okz = true;

                for (int i = 0; i < moons.Count; i++)
                {
                    okx = okx && (moons[i].compareaxis(initialMoons[i], coord => coord.x));
                    oky = oky && (moons[i].compareaxis(initialMoons[i], coord => coord.y));
                    okz = okz && (moons[i].compareaxis(initialMoons[i], coord => coord.z));
                }

       
                if (okx)
                {
                    xCycle = stepCounter - lastx;
                    Log($"{stepCounter}: X-axis correct ({stepCounter - lastx})");
                    lastx = stepCounter;
                }

                if (oky)
                {
                    yCycle = stepCounter - lasty;
                    Log($"{stepCounter}: Y-axis correct ({stepCounter - lasty})");
                    lasty = stepCounter;
                }

                if (okz)
                {
                    zCycle = stepCounter - lastz;
                    Log($"{stepCounter}: Z-axis correct ({stepCounter - lastz})");
                    lastz = stepCounter;
                }

                if (xCycle != -1 && yCycle != -1 && zCycle != -1)
                    return new Coord3d(xCycle, yCycle, zCycle);
            }
        }

        public class Moon
        {
            public Coord3d position;
            public Coord3d velocity;

            public Moon clone()
            {
                return new Moon()
                {
                    position = this.position.Clone(),
                    velocity = this.velocity.Clone()
                };
            }

            public bool sameas(Moon m)
            {
                return position.Equals(m.position) && velocity.Equals(m.velocity);
            }

            public bool compareaxis(Moon initialMoon, Func<Coord3d, int> func)
            {
                return func(position) == func(initialMoon.position) &&
                       func(velocity) == func(initialMoon.velocity);
            }
        }
        private List<Moon> IterateVelocities(List<Moon> moons, int stepCounter)
        {
            var newMoons = new List<Moon>(moons.Select(m => m.clone()));
            for (int m1p = 0; m1p <= newMoons.Count - 2; m1p++)
                for (int m2p = m1p + 1; m2p <= newMoons.Count - 1; m2p++)
                {
                    var m1 = newMoons[m1p];
                    var m2 = newMoons[m2p];
                    var xDiffs = CalculateDiff(m1.position.x, m2.position.x);
                    var yDiffs = CalculateDiff(m1.position.y, m2.position.y);
                    var zDiffs = CalculateDiff(m1.position.z, m2.position.z);
                    m1.velocity = m1.velocity.Move(xDiffs.diff1, yDiffs.diff1, zDiffs.diff1);
                    m2.velocity = m2.velocity.Move(xDiffs.diff2, yDiffs.diff2, zDiffs.diff2);
                }

//            if (stepCounter % 10000 == 0)
//                Log("========= Step "+stepCounter);
            foreach (var m1 in newMoons)
            {
                m1.position = m1.position.Move(m1.velocity);
//                if (stepCounter%10000 == 0)
//                    Log($"pos=<x={m1.position.x}, y={m1.position.y}, z={m1.position.z}>, vel=<x={m1.velocity.x}, y={m1.velocity.y}, z={m1.velocity.z}>");
            }

            return newMoons;
        }

        private (int diff1, int diff2) CalculateDiff(int v1, int v2)
        {
            if (v1 > v2)
                return (-1, +1);
            if (v1 < v2)
                return (+1, -1);
            return (0, 0);
        }
    }
}