using System;
using System.Diagnostics;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;


namespace AdventofCode.AoC_2023
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day22 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(5, 7, "Day22_test.txt")]
        [TestCase(2, 2, "Day22_test2.txt")]
        [TestCase(461, 74074, "Day22.txt")]
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

            // parse input here

            var bricks = new List<Brick>();
            var ix = 1;
            foreach (var s in source)
            {
                var x = new SplitNode(s, '~', ',');
                var brick = new Brick(x.First.IntChildren.ToArray(), x.Second.IntChildren.ToArray(), ix);
                ix++;
                bricks.Add(brick);
            }

            foreach (var b in bricks)
            {
                Log(() => b.Dimensions + " -  " + b.Volume.ToString() + "");
                if (b.Dimensions > 1)
                    Log("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
            }

            LogAndReset("Parse", sw);

            for (int i = 0; i < bricks.Count - 1; i++)
                for (int j = i + 1; j < bricks.Count; j++)
                    if (bricks[i].XYCoords.Any(x => bricks[j].XYCoords.Contains(x)))
                    {
                        if (bricks[i].C1.z > bricks[j].C1.z)
                            bricks[i].CanLandOn.Add(bricks[j]);
                        else
                            bricks[j].CanLandOn.Add(bricks[i]);
                    }

            var moving = new List<Brick>(bricks);

            while (moving.Any())
            {
                var newMoving = new List<Brick>();
                foreach (var b in moving.OrderBy(b => b.C1.z))
                {
                    if (b.CanMoveDown(bricks))
                    {
                        b.MoveDown(1);
                    }

                    if (!b.SupportedByStable)
                        newMoving.Add(b);

                }

                moving = newMoving;
            }

            foreach (var b in bricks)
                if (!b.SupportedBricks.Any(x => x.SupportedBy.Count == 1))
                {
                    part1++;
                    Log(() => $"Brick {b.Name} can be disintegrated");
                }


 

            LogAndReset("*1", sw);
            foreach (var b in bricks)
            {
                var x = b.DisintegrateAndCount();
                Log(() => $"Disintegrating {b.Name} would bring down {x} blocks");
                part2 += x;
            }
            LogAndReset("*2", sw);

            return (part1, part2);
        }
    }

    [DebuggerDisplay("{Name}: {C1.x},{C1.y},{C1.z}~{C2.x},{C2.y},{C2.z}")]
    internal class Brick
    {
        public int Ix { get; }
        public Coord3d C1 { get; set; }
        public Coord3d C2 { get; set; }
        public int Volume => (Math.Abs(C1.x - C2.x) + 1) * (Math.Abs(C1.y - C2.y) + 1) * (Math.Abs(C1.z - C2.z) + 1);
        public int Dimensions => C1.x == C2.x ? 0 : 1 + C1.y == C2.y ? 0 : 1 + C1.z == C2.z ? 0 : 1;

        public Brick(int[] start, int[] end, int ix)
        {
            Ix = ix;

            Name = ix < 20 ? ((char)(ix + 64)).ToString() : ix.ToString();
            C1 = new Coord3d(Math.Min(start[0], end[0]), Math.Min(start[1], end[1]), Math.Min(start[2], end[2]));
            C2 = new Coord3d(Math.Max(start[0], end[0]), Math.Max(start[1], end[1]), Math.Max(start[2], end[2]));
            XYCoords = new HashSet<Coord>();
            for (int x = C1.x; x <= C2.x; x++)
                for (int y = C1.y; y <= C2.y; y++)
                    XYCoords.Add(Coord.FromXY(x, y));
        }

        public HashSet<Coord> XYCoords { get; }

        public string Name { get; }

        public bool CanMoveDown(List<Brick> bricks)
        {
            if (C1.z == 1)
            {
                // on level ground
                SupportedByStable = true;
                return false;
            }

            foreach (var b in CanLandOn)
            {
                if (b.C2.z == C1.z - 1)
                {
                    // Intersects
                    if (b.SupportedByStable)
                    {
                        SupportedByStable = true;
                    }
                    b.SupportedBricks.Add(this);
                    SupportedBy.Add(b);
                }
            }

            return !SupportedByStable && !SupportedBy.Any();
        }

        public void MoveDown(int steps)
        {
            C1 = C1.Move(0, 0, -steps);
            C2 = C2.Move(0, 0, -steps);

            foreach (var x in SupportedBricks)
                x.SupportedBy.Remove(this);
            SupportedBricks.Clear();
        }

        public bool SupportedByStable { get; set; }
        public List<Brick> CanLandOn { get; } = new();

        public List<Brick> SupportedBricks = new();
        public List<Brick> SupportedBy = new();

        public long DisintegrateAndCount()
        {
            var toProcess= new Queue<Brick>();
            toProcess.Enqueue(this);

            var disintegrated = new HashSet<Brick> {this};

            while (toProcess.Any())
            {
                var b = toProcess.Dequeue();
                foreach (var x in b.SupportedBricks)
                {
                    if (x.SupportedBy.All(x => disintegrated.Contains(x)))
                    {
                        disintegrated.Add(x);
                        toProcess.Enqueue(x);
                    }
                }
            }

            return disintegrated.Count-1;
        }
    }
}