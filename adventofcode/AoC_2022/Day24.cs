using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using ABI.Windows.ApplicationModel.Contacts.DataProvider;
using ABI.Windows.Media.Streaming.Adaptive;
using AdventofCode.AoC_2018;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2022
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day24 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(18, null, "Day24_test.txt")]
        [TestCase(9999999, null, "Day24.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            LogLevel = resourceName.Contains("test") ? 20 : -1;
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        private List<MapState> _mapStates = new List<MapState>();

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1 = 0;
            Part2Type part2 = 0;
            var sw = Stopwatch.StartNew();
            var map = source.ToSparseBuffer(' ');
            // parse input here
            Log(map.ToString(x => x.ToString()));
            List<Blizzard> blizzards = new();
            foreach (var k in map.Keys.ToArray())
                if (Coord.TryCharToDir(map[k], out var blizzardDir))
                {
                    blizzards.Add(new Blizzard(k, blizzardDir, map.Width, map.Height));
                    map[k] = '.';
                }
            map.RemoveDefaults();
            Log(map.ToString(x => x.ToString()));

            var entry = map.Keys.Single(k => k.Row == 0 && map[k] == '.');
            var target = map.Keys.Single(k => k.Row == map.Bottom && map[k] == '.');
            _mapStates.Add(new MapState(0, map, entry, blizzards));
            _mapStates.First().GetDebugMap(entry);

            LogAndReset("Parse", sw);

            // precalculate wind-states
            var maxDepth = 50;
            for (int minute = 1; minute < maxDepth + 5; minute++)
            {
                var newState = new MapState(minute, map, entry, MoveBlizzards(_mapStates.Last()));
                Log(() => $"Minute {minute}");
                Log(() => newState.GetDebugMap(null).ToString(c => c.ToString()));

                _mapStates.Add(newState);
            }
            var path = FindPath(entry, new Stack<Coord>(), 0, target, maxDepth);
            part1 = path;

            LogAndReset("*1", sw);

            // solve part 2 here

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private int FindPath(Coord pos, Stack<Coord> path, int minute, Coord target, int best)
        {
            //var initialMoveState = new MoveState(minute, pos);
            if (minute > best)
            {
                return int.MaxValue;
            }

            if (pos.Equals(target))
            {
                Log($"Found path of length {path.Count}");
                return path.Count;
            }

            var mapState = _mapStates[minute + 1];
            path.Push(pos);
            var foundValidPos = false;
            var validNext = pos.GenAdjacent4().Append(pos).Where(x => mapState.FreeCoords.Contains(x)).ToList();
            foreach (var next in validNext)
            {
                foundValidPos = true;
                var temp = FindPath(next, path, minute + 1, target, best);
                if (temp < best)
                    best = temp;
            }
            path.Pop();

            if (!foundValidPos)
                // No where to go/stay
                return int.MaxValue;

            return best;
        }

        private List<Blizzard> MoveBlizzards(MapState mapState)
        {
            var newBlizzards = new List<Blizzard>();
            foreach (var k in mapState.Blizzards)
            {
                var newBlizzard = k.Move();
                newBlizzards.Add(newBlizzard);
            }
            return newBlizzards;
        }
    }

    internal class MapState
    {
        public int Minute { get; }
        private readonly SparseBuffer<char> _map;
        public List<Blizzard> Blizzards { get; }
        public HashSet<Coord> FreeCoords { get; }
        public DicWithDefault<Coord, int> Occupied { get; }

        public MapState(int minute, SparseBuffer<char> map, Coord entry, List<Blizzard> blizzards)
        {
            Minute = minute;
            _map = map;
            Blizzards = blizzards;
            Occupied = new DicWithDefault<Coord, int>();
            foreach (var b in blizzards)
                Occupied[b.Coord]++;
            FreeCoords = new HashSet<Coord>();
            FreeCoords.Add(entry);
            for (int x = 1; x < map.Width; x++)
            {
                for (int y = 1; y < map.Height; y++)
                {
                    var c = Coord.FromXY(x, y);
                    if (Occupied[c] == 0)
                        FreeCoords.Add(c);
                }
            }
        }

        public SparseBuffer<char> GetDebugMap(Coord pos)
        {
            var debugMap = new SparseBuffer<char>();

            foreach (var k in _map.Keys)
                debugMap[k] = _map[k];

            foreach (var k in Blizzards)
            {
                debugMap[k.Coord] = Coord.trans2Arrow[k.Dir];
            }

            foreach (var k in Occupied.Keys)
            {
                var count = Occupied[k];
                if (count is > 1 and <= 9)
                    debugMap[k] = Occupied[k].ToString()[0];
                if (count > 9)
                    debugMap[k] = '*';
            }

            if (pos != null)
                debugMap[pos] = 'E';

            return debugMap;
        }
    }

    [DebuggerDisplay("{Coord}: {Coord.trans2Arrow[Dir].ToString()}")]
    internal class Blizzard
    {
        private readonly int _mapWidth;
        private readonly int _mapHeight;
        public Coord Coord { get; }
        public Coord Dir { get; }

        public Blizzard(Coord coord, Coord dir, int mapWidth, int mapHeight)
        {
            _mapWidth = mapWidth;
            _mapHeight = mapHeight;
            Coord = coord;
            Dir = dir;
        }

        public Blizzard Move()
        {
            var newCoord = Coord.Move(Dir);
            if (newCoord.Row == 0)
            {
                newCoord = Coord.FromXY(newCoord.X, _mapHeight - 2);
            }
            else if (newCoord.Row == _mapHeight - 1)
            {
                newCoord = Coord.FromXY(newCoord.X, 1);
            }
            else if (newCoord.Col == 0)
            {
                newCoord = Coord.FromXY(_mapWidth - 2, newCoord.Y);
            }
            else if (newCoord.Col == _mapWidth - 1)
            {
                newCoord = Coord.FromXY(1, newCoord.Y);
            }

            return new Blizzard(newCoord, Dir, _mapWidth, _mapHeight);
        }
    }
}