﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace AdventofCode.AoC_2019
{
    using Part1Type = Int32;
    using Part2Type = Int32;

    [TestFixture]
    class Day18 : TestBaseClass<Part1Type, Part2Type>
    {
        private int _bestEver;
        private Dictionary<state, int> _bestStates;
        private Dictionary<Coord, Dictionary<Coord, int>> _allDistances;

        private DateTime _ts;
        private DateTime _lastPrint;
        private int _recurseCounter;
        private int _stateAbortCounter;

        public bool Debug { get; set; }

        [Test]
        [TestCase(8, null, "Day18_test.txt", "ab")]
        [TestCase(86, null, "Day18_test2.txt", "abcdef")]
        [TestCase(132, null, "Day18_test3.txt", "bacdefg")]
        [TestCase(136, null, "Day18_test4.txt", "afbjgnhdloepcikm")]
        [TestCase(81, null, "Day18_test5.txt", " acfidgbeh")]
        [TestCase(5392, null, "Day18.txt", "")]
        [TestCase(1684, null, "Day18_2.txt", "")]
        public void Test1(Part1Type exp1, Part2Type? exp2, string resourceName, string order)
        {
            var source = GetResource(resourceName);
            var res = Compute(source);
            DoAsserts(res, exp1, exp2);
        }

        private (Part1Type Part1, Part2Type Part2) Compute(string[] source)
        {
            //5424, 5420 Too high
            // 4384 too low
            var world = BuildMap(source);
            var botPos = world.botCoords.First();

            _ts = DateTime.Now;
            _lastPrint = DateTime.Now;
            PrintMap(world.map);
            _allDistances = AllDistances(world.map);
            var unlockings = ExploreUnlocking(world.map, world.locationDictionary, world.botCoords);
            _bestEver = int.MaxValue;
            _recurseCounter = 0;
            _bestStates = new Dictionary<state, int>();
            var reachableKeys = unlockings[""];
            unlockings.Remove("");

            var remainingKeys = new HashSet<char>();
            foreach (var x in world.map.Keys)
            {
                var c = world.map[x];
                if (c >= 'a' && c <= 'z')
                    remainingKeys.Add(c);
            }

            var bestDist = Recurse(world.locationDictionary, 0, "", reachableKeys, new HashSet<char>(), unlockings, remainingKeys, world.botCoords);

            Log($"Time: {(DateTime.Now - _ts).TotalSeconds:F4} Counter: {_recurseCounter}");

            return (bestDist, 0);
        }

        private int Recurse(
            Dictionary<char, Coord> coordDic,
            int sumDist, 
            string path,
            HashSet<char> reachableKeys, 
            HashSet<char> unlockedDoors,
            Dictionary<string, HashSet<char>> unlockings, 
            HashSet<char> remainingKeys, 
            List<Coord> bots)
        {
            _recurseCounter++;
            var time = (DateTime.Now - _lastPrint).TotalSeconds;
            if (time > 10)
            {
                Log($"{path} Time: {(DateTime.Now - _ts).TotalSeconds:F4} Counter: {_recurseCounter}, aborts: {_stateAbortCounter}");
                _lastPrint = DateTime.Now;
            }

            if (remainingKeys.Count > 0 && reachableKeys.Count == 0)
                throw new Exception();

            if (!reachableKeys.Any())
            {
                if (Debug)
                    Log("found path: " + sumDist);
                if (sumDist < _bestEver)
                {

                    _bestEver = sumDist;
                    Log($"Best so far: {sumDist} Path: {path}: {(DateTime.Now - _ts).TotalSeconds} counter: {_recurseCounter}");
                }
                return sumDist;
            }


            var state = new state(remainingKeys, bots.ToArray());
            if (_bestStates.ContainsKey(state) && _bestStates[state] < sumDist)
            {
                _stateAbortCounter += 1;
                return int.MaxValue;
            }

            _bestStates[state] = sumDist;
            var bestDist = int.MaxValue;

            foreach (var k in reachableKeys)
            {
                var botId = 0;
                var dist = 0;
                for (int i = 0; i < bots.Count; i++)
                {
                    if (_allDistances.TryGetValue(bots[i], out var distanceDic2) &&
                        distanceDic2.TryGetValue(coordDic[k], out var d))
                    {
                        botId = i;
                        dist = d;
                    }
                }

                //                if (path.Contains(k))
                //                {
                //                    Log("What da fuck");
                //                }

                var newDist = sumDist + dist;
                if (newDist >= _bestEver)
                    continue;
                var door = char.ToUpper(k);
                var newReachableKeys = new HashSet<char>(reachableKeys);
                newReachableKeys.Remove(k);

                var newUnlockings = new Dictionary<string, HashSet<char>>(unlockings);
                unlockedDoors.Add(k);
                if (coordDic.ContainsKey(door))
                {
                    unlockedDoors.Add(door);
                }

                foreach (var x in unlockings.Keys)
                {
                    if (x.All(ch => unlockedDoors.Contains(ch)))
                    {
                        foreach (var un in newUnlockings[x])
                            newReachableKeys.Add(un);
                        newUnlockings.Remove(x);
                    }
                }
                var newPath = path + k;
                remainingKeys.Remove(k);

                var oldBotPos = bots[botId];
                bots[botId] = coordDic[k];
                var res = Recurse(coordDic, newDist, newPath, newReachableKeys, unlockedDoors, newUnlockings, remainingKeys, bots);
                // restore the old data
                bots[botId] = oldBotPos;
                remainingKeys.Add(k);
                unlockedDoors.Remove(k);
                if (coordDic.ContainsKey(door))
                {
                    unlockedDoors.Remove(door);
                }

                if (res < bestDist)
                    bestDist = res;
            }

            return bestDist;
        }

        private string OrderDoors(string unlockedDoors, char door)
        {
            return new string((unlockedDoors + door).ToCharArray().OrderBy(ch => char.ToUpper(ch)).ToArray());
        }

        private void PrintMap(SparseBuffer<char> map)
        {
            if (Debug)
                Log(map.ToString(c => c.ToString()));
        }

        private Dictionary<Coord, Dictionary<Coord, int>> AllDistances(SparseBuffer<char> map)
        {
            var res = new Dictionary<Coord, Dictionary<Coord, int>>();
            foreach (var c in map.Keys.Where(k => map[k] != '#'))
            {
                res[c] = GetDistancesFrom(map, c);
            }

            return res;
        }

        private Dictionary<Coord, int> GetDistancesFrom(SparseBuffer<char> map, Coord coord)
        {
            var pending = new Queue<Coord>();
            pending.Enqueue(coord);
            var res = new Dictionary<Coord, int>();
            res[coord] = 0;
            while (pending.Any())
            {
                var c = pending.Dequeue();
                var newDist = res[c] + 1;
                foreach (var neigbour in c.GenAdjacent4())
                {
                    if (!res.ContainsKey(neigbour) && map[neigbour] != '#')
                    {
                        res[neigbour] = newDist;
                        pending.Enqueue(neigbour);
                    }
                }
            }

            return res;
        }

        private Dictionary<string, HashSet<char>> ExploreUnlocking(SparseBuffer<char> map, Dictionary<char, Coord> dic, List<Coord> coords)
        {
            var res = new Dictionary<string, HashSet<char>>();
            var explored = new HashSet<Coord>();
            res[""] = new HashSet<char>();
            foreach (var start in coords)
            {
                var pending = new Queue<(Coord c, string doors)>();
                pending.Enqueue((start, ""));
                while (pending.Any())
                {
                    var x = pending.Dequeue();
                    explored.Add(x.c);
                    foreach (var neigbour in x.c.GenAdjacent4())
                    {
                        if (!explored.Contains(neigbour))
                        {
                            var c = map[neigbour];
                            if (c == '#')
                            {
                                // wall
                            }
                            else if (c >= 'A' && c <= 'Z')
                            {
                                var newDoors = OrderDoors(x.doors, c);
                                res[newDoors] = new HashSet<char>();
                                pending.Enqueue((neigbour, newDoors)); // locked door
                            }
                            else if (c >= 'a' && c <= 'z')
                            {
                                if (!res[x.doors].Contains(c))
                                    res[x.doors].Add(c);
                                var newDoors = OrderDoors(x.doors, c);
                                res[newDoors] = new HashSet<char>();
                                pending.Enqueue((neigbour, newDoors));
                            }
                            else if (c == '.')
                            {
                                pending.Enqueue((neigbour, x.doors));
                            }
                        }
                    }
                }
            }

            foreach (var x in res.Keys.Where(k => res[k].Count == 0).ToArray())
                res.Remove(x);

            var sb = new StringBuilder();
            foreach (var door in res.Keys)
            {
                sb.Append($"Door: {door} unlocks ");
                foreach (var k in res[door])
                    sb.Append(k);
                sb.AppendLine();
            }
            Log(sb.ToString());

            return res;
        }

        private World BuildMap(string[] source)
        {
            var res = new World();
            res.map = new SparseBuffer<char>();
            res.locationDictionary = new Dictionary<char, Coord>();
            res.botCoords = new List<Coord>();
            for (int y = 0; y < source.Length; y++)
                for (var x = 0; x < source[y].Length; x++)
                {
                    var c = source[y][x];
                    var coord = Coord.FromXY(x, y);
                    if (c == '@')
                    {
                        res.botCoords.Add(coord);
                        res.map[coord] = '.';
                    }
                    else
                    {
                        res.map[coord] = c;
                        if (c != '#' && c != '.')
                            res.locationDictionary[c] = coord;
                    }
                }

            return res;
        }

        public struct state
        {
            private readonly int _hash;
            private readonly char[] _keys;
            private readonly Coord[] _coords;
            public state(HashSet<char> remainingKeys, Coord[] botCoords)
            {
                _coords = new Coord[botCoords.Length];
                _keys = remainingKeys.OrderBy(c => c).ToArray();
                _hash = 0;
                unchecked
                {
                    foreach (var k in _keys)
                        _hash = _hash ^ k.GetHashCode() * 397;
                    for (int i = 0; i < botCoords.Length; i++)
                    {
                        _hash = _hash ^ botCoords[i].GetHashCode() * 397;
                        _coords[i] = botCoords[i];
                    }
                }
            }

            public bool Equals(state other)
            {
                return _keys.SequenceEqual(other._keys) && _coords.SequenceEqual(_coords);
            }

            public override bool Equals(object obj)
            {
                return obj is state other && Equals(other);
            }

            public override int GetHashCode()
            {
                return _hash;
            }
        }

        public struct World
        {
            public SparseBuffer<char> map;
            public Dictionary<char, Coord> locationDictionary;
            public List<Coord> botCoords;
        }
    }
}