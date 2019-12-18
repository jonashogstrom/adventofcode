using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        public bool Debug { get; set; }

        [Test]
        [TestCase(8, null, "Day18_test.txt", "ab")]
        [TestCase(86, null, "Day18_test2.txt", "abcdef")]
        [TestCase(132, null, "Day18_test3.txt", "bacdefg")]
        [TestCase(136, null, "Day18_test4.txt", "afbjgnhdloepcikm")]
        [TestCase(81, null, "Day18_test5.txt", " acfidgbeh")]
        [TestCase(-1, null, "Day18.txt", "")]
        public void Test1(Part1Type exp1, Part2Type? exp2, string resourceName, string order)
        {
            var source = GetResource(resourceName);
            var res = Compute(source);
            DoAsserts(res, exp1, exp2);
        }

        private (Part1Type Part1, Part2Type Part2) Compute(string[] source)
        {
            //5424, 5420 Too high
            var res = BuildMap(source);
            PrintMap(res.map);
            _bestEver = int.MaxValue;
            _bestStates = new Dictionary<string, int>();
            var bestDist = Recurse(res.map, res.dic, 0, "");
            //            var start = res.dic['@'];
            return (bestDist, 0);
        }


        private int Recurse(SparseBuffer<char> map, Dictionary<char, Coord> dic, int sumDist, string path)
        {
            var reachableKeys = ExploreMap(map, dic);
            var remainingKeys = new List<char>();
            foreach (var x in map.Keys)
            {
                var c = map[x];
                if (c >= 'a' && c <= 'z')
                    remainingKeys.Add(c);
            }

            var remaining = new string(remainingKeys.OrderBy(c => c).ToArray());

            var pos = dic['@'];
            var state = remaining + ':' + pos.X + ':' + pos.Y;
            if (_bestStates.ContainsKey(state) && _bestStates[state] < sumDist)
            {
                return int.MaxValue;
            }

            _bestStates[state] = sumDist;

            if (!reachableKeys.Any())
            {
                if (Debug)
                    Log("found path: " + sumDist);
                if (sumDist < _bestEver)
                {

                    _bestEver = sumDist;
                    Log("Best so far: " + sumDist + " Path: " + path);
                }
                return sumDist;
            }
            var bestDist = int.MaxValue;
            foreach (var k in reachableKeys.Keys.OrderBy(key => reachableKeys[key]))
            {
                if (path.Contains(k))
                {
                    PrintMap(map);
                    Log("What da fuck");

                }
                var newDist = sumDist + reachableKeys[k];
                if (newDist >= _bestEver)
                    continue;
                var newMap = map.Clone();
                var keyCoord = dic[k];
                newMap[dic['@']] = '.';
                newMap[keyCoord] = '@';
                var newDic = new Dictionary<char, Coord>(dic);
                newDic['@'] = keyCoord;
                var door = char.ToUpper(k);
                if (newDic.ContainsKey(door))
                    newMap[dic[door]] = '.';
                PrintMap(newMap);
                var newPath = path + k;
//                Log(newPath);
                var res = Recurse(newMap, newDic, newDist, newPath);
                if (res < bestDist)
                    bestDist = res;
            }
            if (Debug)
                Log("BestSoFar: " + bestDist);

            return bestDist;
        }

        private void PrintMap(SparseBuffer<char> map)
        {
            if (Debug)
                Log(map.ToString(c => c.ToString()));
        }

        private void AllDistances(SparseBuffer<char> map)
        {
            foreach (var c in map.Keys.Where(k => map[k] != '#'))
            {
                
            }
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

        private (SparseBuffer<char> map, Dictionary<char, Coord> dic) BuildMap(string[] source)
        {
            var map = new SparseBuffer<char>();
            var dic = new Dictionary<char, Coord>();
            for (int y = 0; y < source.Length; y++)
                for (var x = 0; x < source[y].Length; x++)
                {
                    var c = source[y][x];
                    map[Coord.FromXY(x, y)] = c;
                    if (c != '#' && c != '.')
                        dic[c] = Coord.FromXY(x, y);
                }

            return (map, dic);

        }
    }
}