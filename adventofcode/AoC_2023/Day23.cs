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
    class Day23 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(94, 154, "Day23_test.txt")]
        [TestCase(2294, 6418, "Day23.txt")]
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
            var map = source.ToSparseBuffer('X');
            var start = new Coord(map.Top, map.Left + 1);
            var goal = new Coord(map.Bottom, map.Right - 1);
            Assert.That(map[start], Is.EqualTo('.'));
            Assert.That(map[goal], Is.EqualTo('.'));

            LogAndReset("Parse", sw);

            part1 = Solve(source, start, map, goal, false);

            LogAndReset("*1", sw);

            var junctions = new List<Coord>();

            foreach (var c in map.Keys)
            {
                if (map[c] != '.' && map[c] != '#')
                    map[c] = '.';
            }

            foreach (var c in map.Keys)
            {

                if (map[c] == '.' && c.GenAdjacent4().Count(n => map[n] == '.') > 2)
                    junctions.Add(c);
            }
            junctions.Add(start);
            junctions.Add(goal);

            var edges = new List<Edge>();
            var edgeDic = new Dictionary<Coord, List<Edge>>();
            foreach (var c in junctions)
                foreach (var pathStart in c.GenAdjacent4().Where(n => map[n] == '.'))
                {
                    FindNextJunction(pathStart, c, junctions, edges, map);
                }

            foreach (var e in edges)
            {
                if (!edgeDic.TryGetValue(e.Start, out var list))
                    edgeDic[e.Start] = list = new List<Edge>();
                list.Add(e);
            }

            var juncIndex = new Dictionary<Coord, long>();
            for (int i = 0; i < junctions.Count; i++)
                juncIndex[junctions[i]] = 1L << i;

            foreach (var x in edges)
                x.EndIndex = juncIndex[x.End];

            var max = edges.Sum(e => e.PathLength) / 2;

            var p2State = new State2
            {
                Position = start
            };

            p2State.visited = juncIndex[start];
            var q2 = new Queue<State2>();
            q2.Enqueue(p2State);
            var qmax = 0;
            while (q2.Count > 0)
            {
                qmax = Math.Max(qmax, q2.Count);
                var s2 = q2.Dequeue();
                foreach (var e in edgeDic[s2.Position])
                {
                    if (e.End.Equals(goal))
                    {
                        var newLength = s2.PathLength + e.PathLength;
                        if (newLength > part2)
                        {
                            part2 = newLength;
                            Log("Part2 Best so far: " + part2, -1);
                        }
                    }
                    else if ((s2.visited & e.EndIndex) == 0)

                    {
                        var newState = new State2
                        {
                            visited = s2.visited | e.EndIndex,
                            Position = e.End,
                            PathLength = s2.PathLength + e.PathLength
                        };

                        q2.Enqueue(newState);
                    }
                }
            }
            // 4714 is too low, but 9424 is an upper limit
            LogAndReset("*2", sw);
            Log($"Max queue size for *2: {qmax}", -1);

            return (part1, part2);
        }

        private void FindNextJunction(Coord pathStart, Coord startJunction, List<Coord> junctions, List<Edge> edges,
            SparseBuffer<char> map)
        {
            var visited = new HashSet<Coord> { startJunction, pathStart };
            var pos = pathStart;
            var steps = 1;
            while (!junctions.Contains(pos))
            {
                pos = pos.GenAdjacent4().Single(c => map[c] == '.' && !visited.Contains(c));
                visited.Add(pos);
                steps++;
            }
            edges.Add(new Edge()
            {
                Start = startJunction,
                End = pos,
                PathLength = steps
            });
        }

        [DebuggerDisplay("{Start} => {End} ({PathLength} steps)")]
        internal class Edge
        {
            public Coord Start;
            public Coord End;
            public int PathLength;
            public long EndIndex { get; set; }
        }

        private Part1Type Solve(string[] source, Coord start, SparseBuffer<char> map, Coord goal, bool canClimbSlopes)
        {
            var res = 0L;
            var initState = new State();
            initState.Position = start;
            initState.Visited.Add(start);

            var q = new Queue<State>();
            q.Enqueue(initState);
            var stateCounter = 0;
            while (q.Count > 0)
            {
                stateCounter++;
                var s = q.Dequeue();
                var options = s.Position.GenAdjacent4();
                var slope = !canClimbSlopes && Coord.trans2Coord.TryGetValue(map[s.Position], out var dir) ? dir : null;
                var slopeNext = slope != null ? s.Position.Move(slope) : null;
                var listOfNext = new List<Coord>();
                foreach (var v in options)
                {
                    if (!map.InsideBounds(v))
                        continue;
                    if (map[v] == '#')
                        continue;
                    if (s.Visited.Contains(v))
                        continue;
                    if (slope != null && !v.Equals(slopeNext))
                        continue;
                    if (v.Equals(goal))
                    {
                        res = Math.Max(res, s.Visited.Count);
                    }
                    else
                    {
                        listOfNext.Add(v);
                    }
                }

                if (listOfNext.Count == 1)
                {
                    s.Position = listOfNext[0];
                    s.Visited.Add(s.Position);
                    q.Enqueue(s);
                }
                else
                {
                    foreach (var v in listOfNext)
                    {
                        var newState = new State
                        {
                            Position = v,
                            Visited = new HashSet<Coord>(s.Visited) { v }
                        };
                        q.Enqueue(newState);
                    }
                }
            }

            return res;
        }

        private void LogState(string[] source, State s)
        {
            if (LogLevel > 5)
            {
                var debugmap = source.ToSparseBuffer('X');
                foreach (var c in s.Visited)
                    debugmap[c] = 'O';
                Log(() => debugmap.ToString(c => c.ToString()));
            }
        }

        internal class State
        {
            public HashSet<Coord> Visited = new();
            public Coord Position { get; set; }
        }

        internal class State2
        {
            public long visited;
            //            public HashSet<Coord> Visited = new();
            public Coord Position { get; set; }
            public List<Edge> EdgesTravelled { get; set; }
            //            public Dictionary<Coord, List<Edge>> ValidEdges { get; set; }

            public int PathLength;
        }
    }
}