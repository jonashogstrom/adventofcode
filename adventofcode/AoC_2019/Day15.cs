using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace AdventofCode.AoC_2019
{
    using Part1Type = Int32;
    using Part2Type = Int32;

    [TestFixture]
    class Day15 : TestBaseClass<Part1Type, Part2Type>
    {
        [Test]
        [TestCase(404, 406, "Day15.txt")]
        public void Test1(Part1Type exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = Compute(source);
            DoAsserts(res, exp1, exp2);
        }

        private (Part1Type Part1, Part2Type Part2) Compute(string[] source)
        {
            var dirs = new Dictionary<int, Coord> { [1] = Coord.N, [2] = Coord.S, [3] = Coord.W, [4] = Coord.E };
            var reverseDirs = new Dictionary<Coord, int>();
            foreach (var i in dirs.Keys)
                reverseDirs[dirs[i]] = i;

            var comp = new IntCodeComputer(source[0]);
            var droidPos = Coord.FromXY(0, 0);
            Coord oxyPos = null;
            var world = new SparseBuffer<bool?>(null);
            world[droidPos] = false;
            var droidDir = 1;
            var r = new Random();
            var stepCounter = 0;
            var distFromOxy = new SparseBuffer<int>(int.MaxValue / 2);
            var distToOxy = new SparseBuffer<int>(int.MaxValue / 2);
            distToOxy[droidPos] = 0;

            var maxDistFromOxy = 0;

            // 354 too low
            // 404, 402, not right
            var measureDistFromOxy = false;
            var mapIsExplored = false;
            var tileCount = -1;
            var finished = false;
            while (!finished)
            {
                stepCounter++;
                comp.AddInput(droidDir);
                comp.Execute();
                var res = comp.OutputQ.Dequeue();
                if (res == 0)
                {
                    var wallPos = droidPos.Move(dirs[droidDir]);
                    mapIsExplored = SetWorldValue(world, wallPos, mapIsExplored, true);
                }
                else if (res == 1)
                {
                    droidPos = droidPos.Move(dirs[droidDir]);
                    mapIsExplored = SetWorldValue(world, droidPos, mapIsExplored, false);
                }
                else if (res == 2)
                {
                    droidPos = droidPos.Move(dirs[droidDir]);
                    mapIsExplored = SetWorldValue(world, droidPos, mapIsExplored, false);
                    oxyPos = droidPos;
                    measureDistFromOxy = true;
                    distFromOxy[droidPos] = 0;
                }

                if (mapIsExplored && tileCount == -1)
                    tileCount = world.Keys.Count(k => world[k].HasValue && world[k].Value == false);
                UpdateDist(distToOxy, droidPos, -1);

                if (measureDistFromOxy)
                    maxDistFromOxy = UpdateDist(distFromOxy, droidPos, maxDistFromOxy);

                if (mapIsExplored && distFromOxy.Keys.Count() == tileCount)
                    finished = true;

                if (stepCounter % 100000 == 0 || finished)
                {
                    Log("=======================");

                    Log("Steps: " + stepCounter);
                    PrintWorld(world, droidPos, oxyPos);
                    PrintDIST(distFromOxy, droidPos, oxyPos);
                    Log("MaxDist: " + maxDistFromOxy);
                    Log("Explored: " + mapIsExplored);
                    Log("Tiles: " + tileCount);
                }

                if (!mapIsExplored)
                {
                    Coord unexploredDir = Coord.NSWE.FirstOrDefault(dir => !world[droidPos.Move(dir)].HasValue);
                    droidDir = unexploredDir != null ? reverseDirs[unexploredDir] : r.Next(1, 5);
                }
                else
                {
                    var unexploredDir = Coord.NSWE.FirstOrDefault(dir =>
                    {
                        var pos = droidPos.Move(dir);
                        return world[pos].HasValue &&
                               world[pos].Value == false &&
                               distFromOxy[pos] == int.MaxValue / 2;
                    });
                    droidDir = unexploredDir != null ? reverseDirs[unexploredDir] : r.Next(1, 5);
                }
            }
            return (distToOxy[oxyPos], maxDistFromOxy);
        }

        private static bool SetWorldValue(SparseBuffer<bool?> world, Coord pos, bool mapIsExplored, bool value)
        {
            if (!world[pos].HasValue)
            {
                world[pos] = value;
                mapIsExplored = CheckExplored(world);
            }

            return mapIsExplored;
        }

        private static bool CheckExplored(SparseBuffer<bool?> world)
        {
            bool mapIsExplored;
            mapIsExplored = true;
            foreach (var k in world.Keys)
            {
                if (world[k].HasValue && world[k] == false)
                {
                    foreach (var x in k.GenAdjacent4())
                        if (!world[x].HasValue)
                            mapIsExplored = false;
                }
            }

            return mapIsExplored;
        }

        private static int UpdateDist(SparseBuffer<int> dist, Coord droidPos, int oldMax)
        {
            var newDists = dist[droidPos];
            foreach (var adj in droidPos.GenAdjacent4())
                newDists = Math.Min(newDists, dist[adj] + 1);
            if (dist[droidPos] != newDists)
            {
                dist[droidPos] = newDists;
                var maxDist = dist.Keys.Select(k => dist[k]).Max();
                return maxDist;
            }

            return oldMax;
        }

        private void PrintWorld(SparseBuffer<bool?> world, Coord droidPos, Coord oxyPos)
        {
            Log("DroidPos: " + droidPos);
            Log("oxypos: " + oxyPos);
            Log(world.ToString((b, c) =>
            {
                if (c.Equals(oxyPos))
                    return "O";
                if (c.Equals(droidPos))
                    return "D";
                if (c.Equals(Coord.Origin))
                    return "X";
                if (b.HasValue)
                    return b.Value ? "█" : " ";
                return "?";
            }));
        }
        private void PrintDIST(SparseBuffer<int> world, Coord droidPos, Coord oxyPos)
        {
            Log("DroidPos: " + droidPos);
            Log("oxypos: " + oxyPos);
            Log(world.ToString((b, c) =>
            {
                if (c.Equals(oxyPos))
                    return "  O  ";
                if (c.Equals(droidPos))
                    return "  D  ";
                if (b > 10000)
                    return "█████";

                return $" {b:D3} ";
            }, 5));
        }

    }
}