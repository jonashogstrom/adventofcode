using System;
using System.Diagnostics;
using NUnit.Framework;
using System.Collections.Generic;
using AdventofCode.Utils;
using System.Linq;
using AdventofCode.AoC_2019;


namespace AdventofCode.AoC_2024
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day16 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(7036, 45, "Day16_test1.txt")]
        [TestCase(11048, 64, "Day16_test2.txt")]
        [TestCase(103512, 554, "Day16.txt")]
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


            var map = source.ToSparseBuffer();
            // var done = false;
            // // plug dead ends
            // var plugged = 0;
            // while (!done)
            // {
            //     done = true;
            //     foreach (var k in map.Keys)
            //     {
            //         if (map[k] == '.' && Coord.Directions4.Count(d => map[k.Move(d)] == '#') == 3)
            //         {
            //             map[k] = '#';
            //             done = false;
            //             plugged++;
            //         }
            //     }
            // }

            // Log($"Plugged {plugged} squares", LogLevel);
            
            Log(map.ToString(), LogLevel);

            LogAndReset("Parse", sw);
            part1 = long.MaxValue;
            var start = map.Keys.Single(k => map[k] == 'S');
            var end = map.Keys.Single(k => map[k] == 'E');
            var q = new Queue<State>();
            q.Enqueue(new State(start, Coord.E, 0, [start]));
            var visited = new Dictionary<(Coord pos, Coord dir), State>();
            var part2States = new HashSet<Coord>();
            while (q.Count > 0)
            {
                var s = q.Dequeue();
                if (s.Pos.Equals(end))
                {
                    // did we find the best path?
                    if (s.Score < part1)
                    {
                        part2States = s.Path;
                        part1 = s.Score;
                    }
                    else if (s.Score == part1)
                    {
                        foreach (var p in s.Path)
                            part2States.Add(p);
                    }
                    continue;
                }

                var backwards = s.Dir.RotateCWDegrees(180);
                foreach (var d in Coord.Directions4)
                {
                    if (d.Equals(backwards))
                        continue; // no point in moving backwards
                    
                    var next = s.Pos.Move(d);
                    if (map[next] == '#')
                        continue;

                    var newScore = s.Score + 1 + (d.Equals(s.Dir) ? 0 : 1000);
                    if (visited.TryGetValue((next, d), out var state))
                        if (state.Score < newScore)
                            continue;

                    if (newScore > part1)
                        continue;
                    var nextList = new HashSet<Coord>(s.Path);
                    nextList.Add(next);
                    var nextState = new State(next, d, newScore, nextList); 
                    q.Enqueue( nextState );
                    visited[(next, d)] = nextState;
                }
            }
            LogAndReset("*1", sw);

            part2 = part2States.Count();

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        internal class State
        {
            public Coord Pos { get; }
            public Coord Dir { get; }
            public long Score { get; }
            public HashSet<Coord> Path { get; }

            public State(Coord pos, Coord dir, long score, HashSet<Coord> path)
            {
                Pos = pos;
                Dir = dir;
                Score = score;
                Path = path;
            }
        }
    }
}