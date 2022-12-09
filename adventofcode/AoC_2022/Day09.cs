using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2022
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day09 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(13, 1, "Day09_test.txt")]
        [TestCase(null, 36, "Day09_test2.txt")]
        [TestCase(null, null, "Day09_testjay1.txt")]
        //[TestCase(6376, 2607, "Day09.txt")]
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

            var knots1 = new List<Knot>();

            LogAndReset("Parse", sw);
            knots1.Add(new Knot(Coord.Origin, 'H'));
            knots1.Add(new Knot(Coord.Origin, 'T'));
            part1 = MakeMoves(source, knots1);

            LogAndReset("*1", sw);

            var knots2 = new List<Knot>();
            var ropeLength = 10;
            knots2.Add(new Knot(Coord.Origin, 'H'));
            for (int x = 1; x < ropeLength; x++)
            {
                knots2.Add(new Knot(Coord.Origin, x.ToString()[0]));
            }

            part2 = MakeMoves(source, knots2);

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private int MakeMoves(string[] source, List<Knot> knots)
        {
            var visited = new SparseBuffer<bool>();
            visited[knots.Last().Pos] = true;

            var stepCounter = 0;
            var instrCounter = 0;
            foreach (var instr in source)
            {
                instrCounter++;
                var parts = instr.Split(' ');
                var dir = Coord.trans2Coord[parts[0][0]];
                var dist = int.Parse(parts[1]);
                for (var i = 0; i < dist; i++)
                {
                    var head = knots.First();
                    head.Pos = head.Pos.Move(dir);
                    var prev = head.Pos;
                    foreach (var k in knots.Skip(1))
                    {
                        if (!prev.Equals(k.Pos) && !prev.GenAdjacent8().Contains(k.Pos))
                        {
                            k.Pos = k.Pos.Move1Toward(prev);
                        }
                        prev = k.Pos;
                    }

                    visited[knots.Last().Pos] = true;
                    stepCounter++;
                }

                LogMap(visited, knots, stepCounter, instr, instrCounter);
            }

            return visited.Keys.Count();
        }

        private void LogMap(SparseBuffer<bool> visited, List<Knot> knots, int stepCounter, string instr,
            int instrCounter)
        {
            var map = new SparseBuffer<char>('.');
            foreach (var x in visited.Keys)
                map[x] = '#';

            map[Coord.Origin] = 's';
            foreach (var k in knots.Skip(0).Reverse())
            {
                map[k.Pos] = k.Name;
            }

            Log("=======");
            Log($"Instr: {instr} (#{instrCounter}) - step {stepCounter} Head: {knots.First().Pos}");
            Log(() => map.ToString(c => c.ToString()));
        }
    }

    internal class Knot
    {
        public Coord Pos { get; set; }
        public char Name { get; }

        public Knot(Coord pos, char name)
        {
            Pos = pos;
            Name = name;
        }
    }
}