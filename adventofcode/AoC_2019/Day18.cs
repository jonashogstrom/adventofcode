using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace AdventofCode.AoC_2019
{


    using Part1Type = Int32;
    using Part2Type = Int32;

    [TestFixture]
    class Day18 : TestBaseClass<Part1Type, Part2Type>
    {
        private int _bestEver;
        private Dictionary<string, int> _bestStates;
        private Dictionary<Coord, Dictionary<Coord, int>> _allDistances;

        private DateTime _ts;
        private DateTime _lastPrint;
        private int _recurseCounter;
        private int _stateabort;

        //        private Dictionary<string, List<char>> _unlockings;
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
            _bestStates = new Dictionary<string, int>();
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

        private int Recurse(Dictionary<char, Coord> coordDic,
            int sumDist, string path,
            HashSet<char> reachableKeys, HashSet<char> unlockedDoors,
            Dictionary<string, HashSet<char>> unlockings, HashSet<char> remainingKeys, List<Coord> bots)
        {
            _recurseCounter++;
            var time = (DateTime.Now - _lastPrint).TotalSeconds;
            if (time > 10)
            {
                Log($"{path} Time: {(DateTime.Now - _ts).TotalSeconds:F4} Counter: {_recurseCounter}, aborts: {_stateabort}");
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


            var state = new string(remainingKeys.OrderBy(c => c).ToArray());
            foreach(var p in bots)
                state += ':' + p.X + ':' + p.Y;
            if (_bestStates.ContainsKey(state) && _bestStates[state] < sumDist)
            {
                _stateabort += 1;
                return int.MaxValue;
            }


            _bestStates[state] = sumDist;
            var bestDist = int.MaxValue;

            foreach (var k in reachableKeys) //.OrderBy(key => _allDistances[pos][coordDic[key]]))
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
                var pos = bots[botId];

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

                var newUnlockedDoors = new HashSet<char>(unlockedDoors);

                var newUnlockings = new Dictionary<string, HashSet<char>>(unlockings);
                newUnlockedDoors.Add(k);
                if (coordDic.ContainsKey(door))
                {
                    newUnlockedDoors.Add(door);
                }

                foreach (var x in unlockings.Keys)
                {
                    if (x.All(ch => newUnlockedDoors.Contains(ch)))
                    {
                        foreach (var un in newUnlockings[x])
                            newReachableKeys.Add(un);
                        newUnlockings.Remove(x);
                    }
                }
                var newPath = path + k;
                var newRemainingKeys = new HashSet<char>(remainingKeys);
                newRemainingKeys.Remove(k);

                var newBots = new List<Coord>(bots) {[botId] = coordDic[k] };
                var res = Recurse(coordDic, newDist, newPath, newReachableKeys, newUnlockedDoors, newUnlockings, newRemainingKeys, newBots);
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

        private Dictionary<char, int> ExploreMap(SparseBuffer<char> map, Dictionary<char, Coord> dic)
        {
            var reachablekeys = new Dictionary<char, int>();
            var start = dic['@'];
            var distances = new Dictionary<Coord, int>();
            distances[start] = 0;

            var pending = new Queue<Coord>();
            pending.Enqueue(start);
            while (pending.Any())
            {
                var coord = pending.Dequeue();
                var newDist = distances[coord] + 1;
                foreach (var neigbour in coord.GenAdjacent4())
                {
                    var c = map[neigbour];
                    if (distances.ContainsKey(neigbour) && newDist < distances[neigbour])
                        distances[neigbour] = newDist; // return to 
                    else if (c == '#')
                        ; // wall
                    else if (c >= 'A' && c <= 'Z')
                        ; // locked door
                    else if (c == '.')
                    {
                        if (!distances.ContainsKey(neigbour))
                        {
                            distances[neigbour] = newDist;
                            pending.Enqueue(neigbour);
                        }
                    }
                    else if (c >= 'a' && c <= 'z')
                    {
                        if (!distances.ContainsKey(neigbour))
                        {
                            reachablekeys[map[neigbour]] = newDist;
                            distances[neigbour] = newDist;
                            pending.Enqueue(neigbour);
                        }
                    }
                }
            }
            if (Debug)
                foreach (var k in reachablekeys.Keys)
                    Log($"Reachable: {k}={reachablekeys[k]}");
            return reachablekeys;
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

                        if (explored.Contains(neigbour))
                            ;
                        else
                        {
                            var c = map[neigbour];
                            if (c == '#')
                                ; // wall
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

        public struct World
        {
            public SparseBuffer<char> map;
            public Dictionary<char, Coord> locationDictionary;
            public List<Coord> botCoords;

        }
    }
}