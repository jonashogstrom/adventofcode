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
    class Day21 : TestBaseClass<Part1Type, Part2Type>
    {
        private int _p1Steps;
        private int _p2Steps;
        public bool Debug { get; set; }

        [Test]
        [TestCase(16, 16, "Day21_test.txt", 6, 6)]
        [TestCase(16, 50, "Day21_test.txt", 6, 10)]
        [TestCase(16, 1594, "Day21_test.txt", 6, 50)]
        [TestCase(16, 6536, "Day21_test.txt", 6, 100)]
        [TestCase(16, 668697, "Day21_test.txt", 6, 1000)]
        [TestCase(3773, null, "Day21.txt", 64, 26501365)]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName, int p1Steps, int p2Steps)
        {
            _p1Steps = p1Steps;
            _p2Steps = p2Steps;
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

            var map = source.ToSparseBuffer('#');
            var s = map.Keys.First(k => map[k] == 'S');
            map[s] = '.';
            LogAndReset("Parse", sw);

            var positions = new HashSet<Coord>() { s };
            // solve part 1 here
            for (var step = 0; step < _p1Steps; step++)
            {
                var newpos = new HashSet<Coord>();
                foreach (var pos in positions)
                    foreach (var n in pos.GenAdjacent4())
                        if (map[n] == '.')
                            newpos.Add(n);
                // var debugmap = source.ToSparseBuffer('#');
                // foreach (var p in newpos)
                //     debugmap[p] = 'O';
                // Log(debugmap.ToString(x => x.ToString()));
                positions = newpos;
            }

            part1 = positions.Count;
            LogAndReset("*1", sw);

            map = source.ToSparseBuffer('X');
            map[s] = '.';
            positions = new HashSet<Coord>() { s };
            var posCount = new List<long>();
            var diffs = new List<long>();
            var diffs3 = new List<long>();
            var diffs2 = new List<long>();
            var cycle = -1;
            var next = -1;
            for (var step = 0; step < _p2Steps; step++)
            {
                var newpos = new HashSet<Coord>();
                foreach (var pos in positions)
                    foreach (var n in pos.GenAdjacent4())
                    {
                        var newCol = n.Col % map.Width;
                        if (newCol < 0)
                            newCol += map.Width;
                        var newRow =n.Row % map.Height;
                        if (newRow < 0)
                            newRow += map.Height;
                        var n2 = new Coord(newRow, newCol);
                        if (map[n2] == '.')
                            newpos.Add(n);
                    }

                if (step < 10)
                {
                    var debugmap = source.ToSparseBuffer(' ');
                    foreach (var p in newpos)
                        debugmap[p] = 'O';
                    Log(() => $"======= {step + 1} ========");
                    Log(() => debugmap.ToString(x => x.ToString()));
                }

                posCount.Add(newpos.Count);
                var diff = newpos.Count - positions.Count;
                diffs.Add(diff);
                if (diffs.Count >= 2)
                {
                    var diff2 = diffs[^1] - diffs[^2];
                    diffs2.Add(diff2);
                }

                if (diffs2.Count > map.Width)
                {
                    var diff3 = diffs2[^1] - diffs2[^(map.Width+1)];
                    diffs3.Add(diff3);
                }
                if (diffs3.Any())
                    Log(()=>$"step: {step} positions: {posCount[^1]}, diff: {diffs[^1]}, diff2: {diffs2[^1]} diffs3: {diffs3[^1]}");

                positions = newpos;
                if (diffs3.FindCycle(out var c, out var f, map.Width, map.Width))
                {
                    cycle = c;
                    next = step+1;

                    break;
                }
            }

            LogAndReset("*2-1", sw);

            if (posCount.Count < _p2Steps)
            {
                for (int i = next; i < _p2Steps; i++)
                {
                    var diff3 = diffs3[^cycle];
                    diffs3.Add(diff3);
                    var diff2 = diffs2[^cycle] + diff3;
                    diffs2.Add(diff2);
                    var diff1 = diffs[^1] + diff2;
                    diffs.Add(diff1);
                    var newpos = posCount[^1] + diff1;
                    posCount.Add(newpos);
                }

                part2 = posCount[^1];
            }
            part2 = posCount[^1];
        
            // solve part 2 here

            LogAndReset("*2", sw);

            return (part1, part2);
        }
    }
}