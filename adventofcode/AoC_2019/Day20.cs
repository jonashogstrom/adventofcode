using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2019
{
    using Part1Type = Int32;
    using Part2Type = Int32;

    [TestFixture]
    class Day20 : TestBaseClass<Part1Type, Part2Type>
    {
        private DateTime _ts;
        public bool Debug { get; set; }

        // 515, too low 516
        // 6500 too high, 5906 wrong

        [Test]
        [TestCase(23, 26, "Day20_test.txt")]
        [TestCase(58, null, "Day20_testlarge.txt")]
        [TestCase(77, 396, "Day20_testpart2.txt")]
        [TestCase(516, 5966, "Day20.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = Compute(source, exp2.HasValue);
            DoAsserts(res, exp1, exp2);
        }

        private (int? Part1, int? Part2) Compute(string[] source, bool doPart2)
        {
            var world = ParseMap(source);
            var start = world.portalNames["AA"];
            var goal = world.portalNames["ZZ"];
            Log("=== solving part 1 ===");
            _bestSolution = int.MaxValue;
            _bestPath = "";

            SearchPath3(world, start, 0, new HashSet<Coord>(), goal, 0, "", false, 0);
            var part1 = _bestSolution;
            Log($"=== Solution part 1: {_bestSolution}, {_bestPath} ===");

            if (doPart2)
            {
                Log("=== solving part 2 ===");
                _ts = DateTime.Now;
                var limit = 64;
                while (true)
                {
                    _bestSolution = limit;
                    _bestPath = "";

                    SearchPath3(world, start, 0, new HashSet<Coord>(), goal, 0, "", true, 0);
                    if (_bestSolution < limit)
                    {
                        Log($"=== Solution part 2: {_bestSolution}, {_bestPath} ===");
                        return (part1, _bestSolution);
                    }

                    limit = limit * 2;
                }

            }

            return (part1, null);

        }


        private int SearchPath(World19 world, Coord start, Coord goal)
        {
            var distances = new Dictionary<Coord, int>();
            var queue = new Queue<Coord>();
            distances[start] = 0;
            queue.Enqueue(start);
            while (queue.Any())
            {
                var p = queue.Dequeue();
                var newDist = distances[p] + 1;
                var neighbours = p.GenAdjacent4().Where(x => world.corridorCoords.Contains(x)).ToList();
                if (world.portals.ContainsKey(p))
                    neighbours.Add(world.portals[p]);

                foreach (var n in neighbours)
                {
                    if (!distances.ContainsKey(n) || distances[n] > newDist)
                    {
                        queue.Enqueue(n);
                        distances[n] = newDist;
                    }
                }
            }

            return distances[goal];
        }

        private List<Tuple<Coord, int, string>> SearchPath2(World19 world,
            Coord start, int level, HashSet<Coord> portalsUsed, Coord goal, int initialDist, string path, bool recurse)
        {
            if ((DateTime.Now - _ts).TotalSeconds > 10)
            {
                _ts = DateTime.Now;
                Log(path);
            }

            if (level > 25 || initialDist > 6500)
                return new List<Tuple<Coord, int, string>>();

            var distances = new Dictionary<Coord, int>();
            var queue = new Queue<Tuple<Coord, string>>();
            distances[start] = initialDist;
            queue.Enqueue(new Tuple<Coord, string>(start, path));
            var exits = new List<Tuple<Coord, int, string>>();
            while (queue.Any())
            {
                var tuple = queue.Dequeue();
                var pxxx = tuple.Item1;

                //                var newDist = distances[p] + 1;
                var reachable = world.reachable[pxxx];

                foreach (var nextPortal in reachable)
                {
                    var newDist = distances[pxxx] + nextPortal.Item2;
                    if (nextPortal.Item1.Equals(goal))
                    {
                        Log($"Found solution: {newDist}, path: {tuple.Item2}");
                        exits.Add(new Tuple<Coord, int, string>(nextPortal.Item1, newDist, tuple.Item2));
                    }

                    if (!portalsUsed.Contains(nextPortal.Item1) && world.portals.ContainsKey(nextPortal.Item1))
                    {
                        var portalName = world.portalNames2[nextPortal.Item1];
                        var portalDestination = world.portals[nextPortal.Item1];
                        if (recurse && world.portalDirections[nextPortal.Item1] == -1)
                        {
                            if (goal == null || portalDestination == goal)
                                exits.Add(new Tuple<Coord, int, string>(portalDestination, newDist, tuple.Item2));
                        }
                        else
                        {
                            var enterPath = $"{tuple.Item2}(L{level}, D{newDist}){portalName}v ";
                            if (Debug)
                                Log($"Enter level {level + 1} at coord {nextPortal.Item1}=>{portalDestination} via {portalName}, currentDist {newDist + 1} path: {enterPath}");
                            var newPortalsUsed = new HashSet<Coord>(portalsUsed);
                            newPortalsUsed.Add(nextPortal.Item1);

                            List<Tuple<Coord, int, string>> res;
                            if (recurse)
                            {
                                res = SearchPath2(world, portalDestination, level + 1, newPortalsUsed,
                                    null, newDist + 1,
                                    enterPath, recurse);
                            }
                            else
                            {
                                res = new List<Tuple<Coord, int, string>>();
                                res.Add(new Tuple<Coord, int, string>(portalDestination, newDist + 1,
                                    enterPath));
                            }

                            foreach (var t in res)
                            {
                                if (!distances.ContainsKey(t.Item1) || distances[t.Item1] > t.Item2)
                                {
                                    var returnPath = $"{t.Item3}(L{level} D{t.Item2}){world.portalNames2[t.Item1]}^ ";
                                    if (Debug)
                                        Log($"Return to level {level} at coord {t.Item1} via {world.portalNames2[t.Item1]} currentDist: {t.Item2} path: {returnPath}");
                                    distances[t.Item1] = t.Item2;
                                    queue.Enqueue(new Tuple<Coord, string>(t.Item1, returnPath));
                                }
                            }
                        }

                    }
                }
            }

            return exits;
        }


        private int _bestSolution = int.MaxValue;
        private string _bestPath = "";

        private void SearchPath3(World19 world,
            Coord start, int level, HashSet<Coord> portalsUsed,
            Coord goal, int initialDist, string path, bool recurse, int depth)
        {
            if ((DateTime.Now - _ts).TotalSeconds > 10)
            {
                _ts = DateTime.Now;
                Log(path);
            }

            if (level < 0  || initialDist > _bestSolution) 
                return;
            var reachable = world.reachable[start];

            foreach (var nextPortal in reachable)
            {
                var newDist = initialDist + nextPortal.Item2;
                if (level == 0 && nextPortal.Item1.Equals(goal))
                {
                    if (initialDist + nextPortal.Item2 < _bestSolution)
                    {
                        Log($"Found new solution: Dist: {newDist}, Depth: {depth} path: {path}");
                        _bestSolution = initialDist + nextPortal.Item2;
                        _bestPath = path;
                    }
                }
                else if (world.portals.TryGetValue(nextPortal.Item1, out var nextStart))
                {
                    var direction = world.portalDirections[nextPortal.Item1];

                    var newPath = path + $"({level}) {world.portalNames2[start]}=>{world.portalNames2[nextPortal.Item1]}";
                    var newLevel = level + (recurse ? direction : 0);
                    SearchPath3(world, nextStart, newLevel, portalsUsed, goal, newDist + 1, newPath, recurse, depth+1);
                }
            }
        }

        private World19 ParseMap(string[] source)
        {
            var world = new World19();
            world.board = new SparseBuffer<char>();

            var portalNameCoords = new HashSet<Coord>();
            world.corridorCoords = new HashSet<Coord>();
            for (int y = 0; y < source.Length; y++)
                for (int x = 0; x < source[y].Length; x++)
                {
                    var ch = source[y][x];
                    var coord = Coord.FromXY(x, y);
                    world.board[coord] = ch;
                    if (ch == '#')
                    {

                    }
                    else if (ch == '.')
                    {
                        world.corridorCoords.Add(coord);
                    }
                    else if (ch != ' ')
                    {
                        portalNameCoords.Add(coord);
                    }
                }

            world.portalNames = new Dictionary<string, Coord>();
            world.portals = new Dictionary<Coord, Coord>();
            world.portalDirections = new Dictionary<Coord, int>();
            world.portalNames2 = new Dictionary<Coord, string>();
            var center = Coord.FromXY(world.board.Width / 2, world.board.Height / 2);
            while (portalNameCoords.Any())
            {
                var p1 = portalNameCoords.First();
                var p2 = p1.GenAdjacent4().Single(p => portalNameCoords.Contains(p));
                if (p1.Y > p2.Y)
                {
                    var temp = p1;
                    p1 = p2;
                    p2 = temp;
                }

                var portalCoord = p1.GenAdjacent4().Where(x => world.corridorCoords.Contains(x)).Union(
                    p2.GenAdjacent4().Where(x => world.corridorCoords.Contains(x))).Single();
                var portalName = world.board[p1].ToString() + world.board[p2].ToString();
                world.portalNames2[portalCoord] = portalName;
                if (portalCoord.X == 2 || portalCoord.X == world.board.Width - 3 ||
                    portalCoord.Y == 2 || portalCoord.Y == world.board.Height - 3)
                    world.portalDirections[portalCoord] = -1;
                else
                {
                    world.portalDirections[portalCoord] = +1;
                }

                if (world.portalNames.ContainsKey(portalName))
                {
                    world.portals[portalCoord] = world.portalNames[portalName];

                    world.portals[world.portalNames[portalName]] = portalCoord;

                }
                else
                {
                    world.portalNames[portalName] = portalCoord;
                }

                portalNameCoords.Remove(p1);
                portalNameCoords.Remove(p2);

            }

            world.reachable = new Dictionary<Coord, List<Tuple<Coord, int>>>();

            foreach (var c in world.portalNames2.Keys)
            {
                var distances = new Dictionary<Coord, int>();
                var q = new Queue<Coord>();
                q.Enqueue(c);
                distances[c] = 0;

                world.reachable[c] = new List<Tuple<Coord, int>>();
                while (q.Any())
                {
                    var p = q.Dequeue();
                    var newDist = distances[p] + 1;
                    foreach (var n in p.GenAdjacent4().Where(x => world.corridorCoords.Contains(x)))
                    {
                        if (!distances.ContainsKey(n) || distances[n] > newDist)
                        {
                            q.Enqueue(n);
                            distances[n] = newDist;

                            if (world.portalNames2.ContainsKey(n))
                                world.reachable[c].Add(new Tuple<Coord, int>(n, newDist));
                        }
                    }

                }
            }
            // foreach (var c in corridorCoords)
            // {
            //     if (c.GenAdjacent4().Any(n => portalNameCoords.Contains(n)))
            //         portalCoords.Add(c);
            // }

            foreach (var x in world.reachable.Keys.OrderBy(p => world.portalNames2[p]))
            {
                var dir1 = world.portalDirections[x] == -1 ? "OUTSIDE" : "INSIDE";

                Log($"Can go from {world.portalNames2[x]} ({x.X},{x.Y} {dir1})");
                foreach (var y in world.reachable[x])
                {
                    var dir2 = world.portalDirections[y.Item1] == -1 ? "OUTSIDE" : "INSIDE";
                    Log($"                     To {world.portalNames2[y.Item1]} ({y.Item1.X},{y.Item1.Y} {dir2} in {y.Item2} steps");
                }
            }
            return world;
        }
    }

    // internal struct State19
    // {
    //     public int level;
    //     public Coord coord;
    //     public int distance;
    // }

    internal class World19
    {
        public SparseBuffer<char> board;
        public HashSet<Coord> corridorCoords;
        public Dictionary<Coord, Coord> portals;
        public Dictionary<Coord, int> portalDirections;
        public Dictionary<string, Coord> portalNames;
        public Dictionary<Coord, string> portalNames2;
        public Dictionary<Coord, List<Tuple<Coord, int>>> reachable;
    }
}