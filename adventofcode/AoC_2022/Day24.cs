using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AdventofCode.Utils;
using NUnit.Framework;


// NOT 172 - too low
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
        [TestCase(18, 54, "Day24_test.txt")]
        [TestCase(221, 739, "Day24.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            LogLevel = resourceName.Contains("test") ? 20 : -1;
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        private readonly List<BlizzardState> _blizzardStates = new();

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
            var startToTarget = FindPath(entry, new Stack<Coord>(), 0, target);

            part1 = startToTarget;

            LogAndReset("*1", sw);
            var backToStart = FindPath(target, null, startToTarget, entry);
            var backToTarget = FindPath(entry, null, backToStart, target);
            part2 = backToTarget;

            // solve part 2 here

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private int FindPath(Coord pos, Stack<Coord> path, int minute, Coord target)
        {
            var initialMoveState = new MoveStateMod(pos, minute, _blizzardStates.Count, null);
            var queue = new Queue<MoveStateMod>();
            queue.Enqueue(initialMoveState);
            var totalBest = int.MaxValue;
            var stateCounter = 0;
            var exploredStates = new HashSet<MoveStateMod>();
            while (queue.Count > 0)
            {
                var s = queue.Dequeue();

                if (exploredStates.Contains(s))
                    continue;

                exploredStates.Add(s);

                stateCounter++;

                if (s.Minute >= totalBest)
                    continue;

                if (s.Pos.Equals(target))
                {
                    if (s.Minute < totalBest)
                    {
                        Log($"Found solution: {s.Minute}", -1);
                        Log($"Path = {s.Path}", -1);
                        var x = s;
                        while (x != null)
                        {
                            Log(()=>$"Min: {x.Minute}, Row: {x.Pos.Row}, Col: {x.Pos.Col}");
                            x = x.Prev;
                        }

                        totalBest = s.Minute;
                    }
                    continue;
                }

                var nextMinute = s.Minute + 1;
                var nextBlizzardState = _blizzardStates[nextMinute % _blizzardStates.Count];
                var validNext = s.Pos.GenAdjacent4().Append(s.Pos).Where(x => nextBlizzardState.FreeCoords.Contains(x));
                foreach (var nextPos in validNext)
                {
                    var newMoveState = new MoveStateMod(nextPos, nextMinute, _blizzardStates.Count, s);
                    queue.Enqueue(newMoveState);
                }
            }
            Log($"States examined: {stateCounter}", -1);
            return totalBest;
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
        private readonly int _blizzardState;
        
        public MoveStateMod(Coord pos, int minute, int blizzardCount, MoveStateMod prevState)
        {
            _blizzardState = minute % blizzardCount;
            Pos = pos;
            Minute = minute;
            Prev = prevState;
        }

        protected bool Equals(MoveStateMod other)
        {
            return _blizzardState == other._blizzardState && Equals(Pos, other.Pos);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MoveStateMod)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_blizzardState, Pos);
        }

        public MoveStateMod Prev { get; }
        public int Minute { get; }
        public Coord Pos { get; }

        public string Path
        {
            get
            {
                if (Prev != null)
                {
                    var s = Prev.Path;
                    if (Prev.Pos.Equals(Pos))
                        return s + "*";
                    return s + Coord.trans2Arrow[Pos.Subtract(Prev.Pos)];
                }
                return "";
            }
        }

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
            for (int x = 1; x < map.Width-1; x++)
            {
                for (int y = 1; y < map.Height-1; y++)
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