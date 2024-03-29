﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2021
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day22 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(39, 39, "Day22_test.txt")]
        [TestCase(590784, null, "Day22_test2.txt")]
        [TestCase(474140, 2758514936282235, "Day22_test3.txt")]
        [TestCase(580810, 1265621119006734, "Day22.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
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

            var maxRange = new[] { int.MinValue, int.MaxValue };
            var range50 = new[] { -50, 50 };
            var world1 = new HashSet<Cuboid>() { new Cuboid(false, range50, range50, range50) };
            var world2 = new HashSet<Cuboid>() { new Cuboid(false, maxRange, maxRange, maxRange) };

            foreach (var s in source)
            {
                var p = s.Split(new[] { " ", "..", "=", "," }, StringSplitOptions.RemoveEmptyEntries);
                var xRange = new int[] { int.Parse(p[2]), int.Parse(p[3]) };
                var yRange = new int[] { int.Parse(p[5]), int.Parse(p[6]) };
                var zRange = new int[] { int.Parse(p[8]), int.Parse(p[9]) };
                var state = p[0][1] == 'n';
                var c = new Cuboid(state, xRange, yRange, zRange);
                ProcessCuboid(world1, c);
                ProcessCuboid(world2, c);
            }


            part1 = world1.Where(c => c.State).Sum(c => c.Size);
            part2 = world2.Where(c => c.State).Sum(c => c.Size);

            LogAndReset("Parse", sw);
            LogAndReset("*1", sw);
            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private static void ProcessCuboid(HashSet<Cuboid> world2, Cuboid c)
        {
            foreach (var x in world2.ToList())
                if (x.State != c.State)
                {
                    if (x.Overlaps(c))
                    {
                        world2.Remove(x);
                        foreach (var newX in x.Split7(c).ToList())
                            world2.Add(newX);
                    }
                }
        }

        // naive solution that works for *1
        private void ProcessStep(DicWithDefault<Coord3d, bool> world, int[] xRange, int[] yRange, int[] zRange,
            bool @on, int minRange, int maxRange)
        {
            for (int x = Math.Max(xRange[0], minRange); x <= Math.Min(xRange[1], maxRange); x++)
                for (int y = Math.Max(yRange[0], minRange); y <= Math.Min(yRange[1], maxRange); y++)
                    for (int z = Math.Max(zRange[0], minRange); z <= Math.Min(zRange[1], maxRange); z++)
                    {
                        var c = new Coord3d(x, y, z);
                        if (on)
                            world[c] = true;
                        else
                            world[c] = false;
                    }
        }
    }

    internal class Cuboid
    {
        public int[] XRange { get; }
        public int[] YRange { get; }
        public int[] ZRange { get; }
        public bool State { get; set; }
        public long Size => RangeSize(XRange) * RangeSize(YRange) * RangeSize(ZRange);

        private long RangeSize(int[] r)
        {
            return r[1] - r[0] + 1;
        }

        public Cuboid(bool state, int[] xRange, int[] yRange, int[] zRange)
        {
            XRange = xRange;
            YRange = yRange;
            ZRange = zRange;
            State = state;
        }

        public bool Overlaps(Cuboid cuboid)
        {
            return RangeOverlap(cuboid.XRange, XRange) &&
                   RangeOverlap(cuboid.YRange, YRange) &&
                   RangeOverlap(cuboid.ZRange, ZRange);
        }

        private bool RangeOverlap(int[] r1, int[] r2)
        {
            return r1[0] <= r2[1] && r1[1] >= r2[0];
        }

        public IEnumerable<Cuboid> Split(Cuboid cuboid)
        {
            var xSplitRanges = CreateSplitRanges(XRange, cuboid.XRange);
            var ySplitRanges = CreateSplitRanges(YRange, cuboid.YRange);
            var zSplitRanges = CreateSplitRanges(ZRange, cuboid.ZRange);
            foreach (var rX in xSplitRanges)
                foreach (var rY in ySplitRanges)
                    foreach (var rZ in zSplitRanges)
                    {
                        var newCuboid = new Cuboid(State, rX, rY, rZ);
                        if (Overlaps(newCuboid))
                        {
                            if (newCuboid.Overlaps(cuboid))
                                newCuboid.State = cuboid.State;
                            yield return newCuboid;
                        }
                    }
        }

        public IEnumerable<Cuboid> Split7(Cuboid cuboid)
        {
            var restCuboid = this;

            if (cuboid.YRange[1] < restCuboid.YRange[1]) // yield the area above
            {
                yield return new Cuboid(State, restCuboid.XRange, new[] { cuboid.YRange[1] + 1, restCuboid.YRange[1] },
                    restCuboid.ZRange);
                restCuboid = new Cuboid(State, restCuboid.XRange, new[] { restCuboid.YRange[0], cuboid.YRange[1] },
                    restCuboid.ZRange);
            }

            if (cuboid.YRange[0] > restCuboid.YRange[0]) // yield the area below
            {
                yield return new Cuboid(State, restCuboid.XRange, new[] { restCuboid.YRange[0], cuboid.YRange[0] - 1 },
                    restCuboid.ZRange);
                restCuboid = new Cuboid(State, restCuboid.XRange, new[] { cuboid.YRange[0], restCuboid.YRange[1] },
                    restCuboid.ZRange);
            }

            // ======================================

            if (cuboid.XRange[1] < restCuboid.XRange[1]) // yield the left
            {
                yield return new Cuboid(State, new[] { cuboid.XRange[1] + 1, restCuboid.XRange[1] }, restCuboid.YRange,
                    restCuboid.ZRange);
                restCuboid = new Cuboid(State, new[] { restCuboid.XRange[0], cuboid.XRange[1] }, restCuboid.YRange,
                    restCuboid.ZRange);
            }

            if (cuboid.XRange[0] > restCuboid.XRange[0]) // yield the right
            {
                yield return new Cuboid(State, new[] { restCuboid.XRange[0], cuboid.XRange[0] - 1 }, restCuboid.YRange,
                    restCuboid.ZRange);
                restCuboid = new Cuboid(State, new[] { cuboid.XRange[0], restCuboid.XRange[1] }, restCuboid.YRange,
                    restCuboid.ZRange);
            }

            // ======================================

            if (cuboid.ZRange[1] < restCuboid.ZRange[1]) // yield the front
            {
                yield return new Cuboid(State, restCuboid.XRange, restCuboid.YRange,
                    new int[] { cuboid.ZRange[1] + 1, restCuboid.ZRange[1] });
                restCuboid = new Cuboid(State, restCuboid.XRange, restCuboid.YRange,
                    new int[] { restCuboid.ZRange[0], cuboid.ZRange[1] });
            }

            if (cuboid.ZRange[0] > restCuboid.ZRange[0]) // yield the back
            {
                yield return new Cuboid(State, restCuboid.XRange, restCuboid.YRange,
                    new int[] { restCuboid.ZRange[0], cuboid.ZRange[0] - 1 });
                restCuboid = new Cuboid(State, restCuboid.XRange, restCuboid.YRange,
                    new int[] { cuboid.ZRange[0], restCuboid.ZRange[1] });
            }

            yield return new Cuboid(cuboid.State, restCuboid.XRange, restCuboid.YRange, restCuboid.ZRange);
        }

        private IEnumerable<int[]> CreateSplitRanges(int[] mainRange, int[] r2)
        {
            if (!RangeOverlap(mainRange, r2) || // no overlap
                RangeContains(r2, mainRange)) // complete overlap
                yield return mainRange;
            else
            {
                if (r2[0] <= mainRange[0])
                {
                    // range starts before mainRange
                    yield return new[] { mainRange[0], r2[1] };
                    yield return new[] { r2[1] + 1, mainRange[1] };
                }
                else if (r2[1] >= mainRange[1])
                {
                    // r2 ends after mainRange
                    yield return new[] { mainRange[0], r2[0] - 1 };
                    yield return new[] { r2[0], mainRange[1] };
                }
                else
                {
                    // r2 sits inside mainRange
                    yield return new[] { mainRange[0], r2[0] - 1 };
                    yield return r2;
                    yield return new[] { r2[1] + 1, mainRange[1] };
                }
            }
        }

        private static bool RangeContains(int[] range, int[] contained)
        {
            return range[0] <= contained[0] && range[1] >= contained[1];
        }
    }
}