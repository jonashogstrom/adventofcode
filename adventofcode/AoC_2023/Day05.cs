using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2023
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day05 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(35, 46, "Day05_test.txt")]
        [TestCase(484023871, 46294175, "Day05.txt")]
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

            var groups = source.AsGroups();
            // parse input here

            var seeds = groups.First().First().Split(':').Last().Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(s => long.Parse(s)).ToList();
            var maps = groups.Skip(1).ToList();

            var intMaps = new List<List<MapLine>>();
            foreach (var m in maps)
            {
                var intMap = new List<MapLine>();
                foreach (var line in m.Skip(1))
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(x => long.Parse(x))
                        .ToArray();

                    var mapLine = new MapLine(parts[1], parts[0], parts[2]);
                    intMap.Add(mapLine);
                }

                intMap = intMap.OrderBy(x => x.Source).ToList();
                var fillers = new List<MapLine>();
                if (intMap.First().Source > 0)
                    fillers.Add(new MapLine(0, 0, intMap.First().Source));

                foreach (var p in intMap.AsPairs())
                {
                    var gapLength = p.Item2.Source - (p.Item1.Source + p.Item1.Length);
                    if (gapLength > 0)
                    {
                        fillers.Add(new MapLine(p.Item1.Source + p.Item1.Length, p.Item1.Source + p.Item1.Length, gapLength));
                    }
                }
                fillers.Add(new MapLine(intMap.Last().Source + intMap.Last().Length, intMap.Last().Source + intMap.Last().Length, (intMap.Last().Source + intMap.Last().Length) * 2));
                intMap.AddRange(fillers);
                intMap = intMap.OrderBy(x => x.Source).ToList();

                intMaps.Add(intMap);
                var sumTargets = intMap.Sum(x => x.Length);
                Log($"{m[0]}=>{sumTargets}");
            }

            LogAndReset("Parse", sw);

            var map = intMaps.First();
            foreach (var map2 in intMaps.Skip(1))
            {
                var temp = MergeMaps(map, map2);
                map = temp;
            }

            var bestLocation = long.MaxValue;
            foreach (var s in seeds)
            {
                var l = GetLocation(s, map);
                bestLocation = Math.Min(l, bestLocation);
            }
            part1 = bestLocation;
            LogAndReset("*1", sw);

            var seedmap = new List<MapLine>();
            for (var i = 0; i < seeds.Count / 2; i++)
            {
                var seedLine = new MapLine(seeds[i * 2], seeds[i * 2], seeds[i * 2 + 1]);
                seedmap.Add(seedLine);
            }

            var totalMap = MergeMaps(seedmap, map).OrderBy(x => x.Dest).ToList();

            part2 = totalMap.First().Dest;

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private List<MapLine> MergeMaps(List<MapLine> map, List<MapLine> map2)
        {
            var result = new List<MapLine>();
            var q = new PriorityQueue<MapLine, long>();
            foreach (var mapLine in map)
            {
                q.Enqueue(mapLine, mapLine.Source);
            }
            while (q.Count > 0)
            {
                var s = q.Dequeue();
                foreach (var s2 in map2)
                {
                    // Find the intersection between the target range in map1 and the source range in map 2 and add that range to the result map
                    var newLine = MergeMapLine(s, s2, q);
                    if (newLine != null)
                    {
                        // s has been handled and any residue is in the queue. keep moving.
                        result.Add(newLine);
                        break;
                    }
                }
            }
            result = result.OrderBy(m => m.Source).ToList();
            return result;
        }

        private static MapLine MergeMapLine(MapLine s, MapLine s2, PriorityQueue<MapLine, long> q)
        {
            var targetRangeStart = s.Dest;
            var targetRangeEnd = s.Dest + s.Length;
            var sourceRangeStart = s2.Source;
            var sourceRangeEnd = s2.Source + s2.Length;
            var intersectionStart = Math.Max(targetRangeStart, sourceRangeStart);
            var intersectionEnd = Math.Min(targetRangeEnd, sourceRangeEnd);
            if (intersectionEnd > intersectionStart)
            {
                // find the residue of unmapped parts for the source-mapline

                var residueLength1 = intersectionStart - s.Dest;
                if (residueLength1 > 0)
                {
                    q.Enqueue(new MapLine(s.Source, s.Dest, residueLength1), s.Source);
                }

                var residueLength2 = s.Dest + s.Length - intersectionEnd;
                if (residueLength2 > 0)
                {
                    var mapLine = new MapLine(s.Source + s.Length - residueLength2, s.Dest + s.Length - residueLength2, residueLength2);
                    q.Enqueue(mapLine, mapLine.Source);
                }

                var mapStart = intersectionStart >= sourceRangeStart
                    ? s.Source
                    : s.Source + (sourceRangeStart - intersectionStart);
                return new MapLine(mapStart, s2.Dest + intersectionStart - s2.Source, intersectionEnd - intersectionStart);
            }

            return null;

        }

        [Test]
        public void TestMapLine()
        {
            var m1 = new MapLine(10, 20, 20);
            var m2 = new MapLine(15, 7, 10);
            var q = new PriorityQueue<MapLine, long>();
            var merged = MergeMapLine(m1, m2, q);
            Assert.That(merged.Source, Is.EqualTo(10));
            Assert.That(merged.Dest, Is.EqualTo(12));
            Assert.That(merged.Length, Is.EqualTo(5));
            Assert.That(q.Count, Is.EqualTo(1));
            var x = q.Dequeue();
            Assert.That(x.Source, Is.EqualTo(15));
            Assert.That(x.Dest, Is.EqualTo(25));
            Assert.That(x.Length, Is.EqualTo(15));
        }

        private long GetLocation(long seed, List<MapLine> map)
        {
            var item = seed;

            foreach (var line in map)
            {
                if (line.Source <= item && line.Source + line.Length > item)
                {
                    item = line.Dest + item - line.Source;
                    break;
                }
            }

            return item;
        }
    }

    [DebuggerDisplay("{Source}-{SourceEnd} => {Dest}-{DestEnd} ({Length})")]
    internal class MapLine
    {
        public MapLine(long source, long dest, long length)
        {
            Source = source;
            Dest = dest;
            Length = length;
        }

        public long Source;
        public long SourceEnd => Source + Length - 1;
        public long DestEnd => Dest + Length - 1;
        public long Dest;
        public long Length;
    }
}