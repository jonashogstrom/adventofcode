using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using ABI.Windows.ApplicationModel.Contacts.DataProvider;
using ABI.Windows.Media.Streaming.Adaptive;
using Accord;
//using Accord.Collections;
using AdventofCode.AoC_2018;
using AdventofCode.Utils;
using NUnit.Framework;
using WinRT.Interop;

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

        private List<BlizzardState> _blizzardStates = new List<BlizzardState>();

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
            _blizzardStates.Add(new BlizzardState(0, map, entry, target, blizzards));
            _blizzardStates.First().GetDebugMap(entry);

            LogAndReset("Parse", sw);

            // precalculate wind-states
            var maxDepth = (map.Height - 2) * (map.Width - 2);
            for (int minute = 1; minute < maxDepth; minute++)
            {
                var newState = new BlizzardState(minute, map, entry, target, MoveBlizzards(_blizzardStates.Last()));
                Log(() => $"Minute {minute}");
                Log(() => newState.GetDebugMap(null).ToString(c => c.ToString()));

                _blizzardStates.Add(newState);
            }
            var path = FindPath(entry, new Stack<Coord>(), 0, target);
            part1 = path;

            LogAndReset("*1", sw);

            // solve part 2 here

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private int FindPath(Coord pos, Stack<Coord> path, int minute, Coord target)
        {
            var initialMoveState = new MoveState(minute, pos, null);
            var queue = new Queue<MoveState>();
            queue.Enqueue(initialMoveState);
            var totalBest = int.MaxValue;
            var stateCounter = 0;
            var expStates = new Dictionary<MoveStateMod, MoveState>();
            // {
            //     [(pos, 0)] = initialMoveState
            // };
            var highestMinuteExamined = 0;
            while (queue.Count > 0)
            {
                var s = queue.Dequeue();
                var modState = new MoveStateMod(s, _blizzardStates.Count);
                if (expStates.TryGetValue(modState, out var oldState))
                    if (oldState.Minute < s.Minute)
                        continue;

                if (stateCounter > _blizzardStates.Count*_blizzardStates.Count)
                    Log("unexpected number of states examined");
                expStates[modState] = s;

                if (s.Minute == 1 && s.Pos.X == 1 && s.Pos.Y == 1)
                {
                    Log("examine this state");
                }
                stateCounter++;
                if (s.Minute > highestMinuteExamined)
                    highestMinuteExamined = s.Minute;

                if (s.Minute >= totalBest)
                    continue;

                if (s.Pos.Equals(target) )
                {
                    if (s.Minute <= totalBest)
                    {
                        Log($"Found solution: {s.Minute}");
                        Log($"Path = {s.Path}");
                        var x = s;
                        while (x != null)
                        {
                            Log($"Min: {x.Minute}, Row: {x.Pos.Row}, Col: {x.Pos.Col}");
                            x = x.Prev;
                        }

                        totalBest = s.Minute;
                    }

                    continue;
                }

                var nextMinute = s.Minute + 1;
                var nextBlizzardState = _blizzardStates[nextMinute % _blizzardStates.Count];
                var validNext = s.Pos.GenAdjacent4().Append(s.Pos).Where(x => nextBlizzardState.FreeCoords.Contains(x)).ToList();
                foreach (var nextPos in validNext)
                {
                    var newMoveState = new MoveState(nextMinute, nextPos, s);
                    queue.Enqueue(newMoveState);
                }
            }
            Log($"States examined: {stateCounter}");
            Log($"Highest Minute examined: {highestMinuteExamined}");
            return totalBest;
        }
        private int FindPath_recursive(Coord pos, Stack<Coord> path, int minute, Coord target, int best)
        {

            if (minute > best)
            {
                return int.MaxValue;
            }

            if (pos.Equals(target))
            {
                Log($"Found path of length {path.Count}");
                return path.Count;
            }

            var mapState = _blizzardStates[minute + 1];
            path.Push(pos);
            var foundValidPos = false;
            var validNext = pos.GenAdjacent4().Append(pos).Where(x => mapState.FreeCoords.Contains(x)).ToList();
            foreach (var next in validNext)
            {
                foundValidPos = true;
                var temp = FindPath_recursive(next, path, minute + 1, target, best);
                if (temp < best)
                    best = temp;
            }
            path.Pop();

            if (!foundValidPos)
                // No where to go/stay
                return int.MaxValue;

            return best;
        }

        private List<Blizzard> MoveBlizzards(BlizzardState blizzardState)
        {
            var newBlizzards = new List<Blizzard>();
            foreach (var k in blizzardState.Blizzards)
            {
                var newBlizzard = k.Move();
                newBlizzards.Add(newBlizzard);
            }
            return newBlizzards;
        }
    }

    internal class MoveStateMod
    {
        private readonly Coord _pos;
        private readonly int _blizzardStates;

        public MoveStateMod(MoveState moveState, int blizzardCount)
        {
            _pos = moveState.Pos;
            _blizzardStates = moveState.Minute % blizzardCount;
        }

        private sealed class PosBlizzardStatesEqualityComparer : IEqualityComparer<MoveStateMod>
        {
            public bool Equals(MoveStateMod x, MoveStateMod y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return Equals(x._pos, y._pos) && x._blizzardStates == y._blizzardStates;
            }

            public int GetHashCode(MoveStateMod obj)
            {
                return HashCode.Combine(obj._pos, obj._blizzardStates);
            }
        }

        //        public static IEqualityComparer<MoveStateMod> PosBlizzardStatesComparer { get; } = new PosBlizzardStatesEqualityComparer();
    }

    internal class MoveStateComparer : IComparer<MoveState>
    {
        public int Compare(MoveState x, MoveState y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;
            var res = x.DistanceFromTarget.CompareTo(y.DistanceFromTarget);
            if (res != 0)
                return res;
            res = x.Minute.CompareTo(y.Minute);
            return res;
        }
    }

    internal class MoveState
    {
        private readonly MoveState _prevState;
        public int Minute { get; }
        public Coord Pos { get; }
        //        public Coord Target { get; }
        public int DistanceFromTarget { get; set; }

        public MoveState(int minute, Coord pos, MoveState prevState)
        {
            _prevState = prevState;
            Minute = minute;
            Pos = pos;
            //            Target = target;
            //            DistanceFromTarget = pos.Dist(target);
        }

        public string Path
        {
            get
            {
                if (_prevState != null)
                {
                    var s = _prevState.Path;
                    if (_prevState.Pos.Equals(Pos))
                        return s + "*";
                    return s + Coord.trans2Arrow[Pos.Subtract(_prevState.Pos)];
                }
                return "";
            }
        }

        public MoveState Prev  => _prevState;
    }

    [DebuggerDisplay("Min {Minute} Free: {FreeCoords.Count}")]
    internal class BlizzardState
    {
        public int Minute { get; }
        private readonly SparseBuffer<char> _map;
        public List<Blizzard> Blizzards { get; }
        public HashSet<Coord> FreeCoords { get; }
        public DicWithDefault<Coord, int> Occupied { get; }

        public BlizzardState(int minute, SparseBuffer<char> map, Coord entry, Coord exit, List<Blizzard> blizzards)
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
            FreeCoords.Add(exit);
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