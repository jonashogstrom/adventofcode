using System;
using System.Diagnostics;
using NUnit.Framework;
using System.Collections.Generic;
using AdventofCode.Utils;
using System.Linq;


namespace AdventofCode.AoC_2023
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day17 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(102, 94, "Day17_test.txt")]
        [TestCase(1039, 1201, "Day17.txt")]
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

            var map = source.ToSparseBuffer<int>(-1, c=>int.Parse(c.ToString()));
            Log("input map");
            Log(map.ToString());

            LogAndReset("Parse", sw);

            var startState = new State(map.TopLeft, "");
            var targetCoord = map.BottomRight;
            var res = SolvePart1(targetCoord, map, startState);

            DebugLogMap(source, res);

            part1 = res.Skip(1).Select(s => map[s.Coord]).Sum();

            LogAndReset("*1", sw);

            var res2 = SolvePart2(targetCoord, map, startState);

            DebugLogMap(source, res2);

            part2 = res2.Skip(1).Select(s => map[s.Coord]).Sum();

            LogAndReset("*2", sw);
            // 1196 too low
            return (part1, part2);
        }

        private void DebugLogMap(string[] source, List<State> res)
        {
            var debug = source.ToSparseBuffer();
            foreach (var s in res)
                if (!string.IsNullOrEmpty(s.LastMoves))
                    debug[s.Coord] = s.LastMoves[^1];
            Log(debug.ToString());
        }

        private static List<State> SolvePart1(Coord targetCoord, SparseBuffer<int> map, State startState)
        {
            var a = new GenAStar<State>(
                s =>
                {
                    if (s.Coord.Equals(targetCoord))
                        return new State[] { };
                    var res = new List<State>();
                    foreach (var dir in Coord.NSWE)
                    {
                        var nextPos = s.Coord.Move(dir);
                        if (!map.InsideBounds(nextPos))
                            continue; // don't move off board
                        if (!string.IsNullOrEmpty(s.LastMoves) &&
                            Coord.trans2Arrow[dir.RotateCWDegrees(180)] == s.LastMoves[^1])
                            continue; // don't move backwards
                        if (s.LastMoves.Length == 3 && s.LastMoves.All(c => c == Coord.trans2Arrow[dir]))
                            continue; // can't move in same dir 4 times in a row
                        res.Add(new State(nextPos,
                            (s.LastMoves.Length == 3 ? s.LastMoves.Substring(1) : s.LastMoves) + Coord.trans2Arrow[dir]));
                    }

                    return res;
                },
                state => state.Coord.Dist(targetCoord),
                (from, to) => map[to.Coord],
                state => state.Coord.Equals(targetCoord));

            var res = a.FindPath(startState);
            return res;
        }

        private static List<State> SolvePart2(Coord targetCoord, SparseBuffer<int> map, State startState)
        {
            var a = new GenAStar<State>(
                s =>
                {
                    if (s.Coord.Equals(targetCoord))
                        return new State[] { };

                    var allowedMoves = Coord.NSWE;
                    if ((s.LastMoves.Length > 0 && s.LastMoves.Length < 4) || (!string.IsNullOrEmpty(s.LastMoves) && s.LastMoves.TakeLast(4).Distinct().Count() > 1))
                    {
                        // the last four moves contains a turn, need to repeat last move
                        allowedMoves = new Coord[] { Coord.trans2Coord[s.LastMoves[^1]] };
                    }

                    var res = new List<State>();
                    foreach (var dir in allowedMoves)
                    {
                        var nextPos = s.Coord.Move(dir);
                        if (!map.InsideBounds(nextPos))
                            continue; // don't move off board
                        if (!string.IsNullOrEmpty(s.LastMoves) &&
                            Coord.trans2Arrow[dir.RotateCWDegrees(180)] == s.LastMoves[^1])
                            continue; // don't move backwards
                        if (s.LastMoves.Length == 10 && s.LastMoves.All(c => c == Coord.trans2Arrow[dir]))
                            continue; // can't move in same dir 10 times in a row
                        res.Add(new State(nextPos,
                            (s.LastMoves.Length == 10 ? s.LastMoves.Substring(1) : s.LastMoves) + Coord.trans2Arrow[dir]));
                    }

                    return res;
                },
                state => state.Coord.Dist(targetCoord),
                (from, to) => map[to.Coord],
                state => state.Coord.Equals(targetCoord));

            var res = a.FindPath(startState);
            return res;
        }

        [DebuggerDisplay("{Coord} - {LastMoves}")]
        internal class State
        {
            private readonly int _hash;

            public State(Coord coord, string moves)
            {
                Coord = coord;
                LastMoves = moves;
                _hash = HashCode.Combine(Coord, LastMoves);
            }

            public Coord Coord { get; }
            public string LastMoves { get; }

            protected bool Equals(State other)
            {
                return Equals(Coord, other.Coord) && LastMoves == other.LastMoves;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((State)obj);
            }

            public override int GetHashCode()
            {
                return _hash;
            }
        }
    }
}