using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2021
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day19 : TestBaseClass<Part1Type, Part2Type>
    {
        private List<Coord3d> _globalOffsets = new List<Coord3d>();
        public bool Debug { get; set; }

        [Test]
        [TestCase(6, null, "Day19_test2.txt", 6)]
        [TestCase(3, null, "Day19_test2_tiny.txt", 2)]
        [TestCase(79, 3621, "Day19_test.txt", 12)]
        [TestCase(367, 11925, "Day19.txt", 12)]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName, int overlapCount)
        {
            OverlapCount = overlapCount;
            LogLevel = resourceName.Contains("test") ? 20 : 0;
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        public int OverlapCount { get; set; }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1 = 0;
            Part2Type part2 = 0;
            var sw = Stopwatch.StartNew();
            var groups = source.AsGroups();
            var scanners = new List<Scanner>();
            foreach (var g in groups)
                scanners.Add(ParseScanner(g, scanners.Count));

            LogAndReset("Parse", sw);

            var masterScanner = scanners.First();

            var queue = new Queue<Scanner>(scanners.Skip(1));
            var testedSinceMatch = 0;
            while (queue.Any())
            {
                var sc = queue.Dequeue();
                var sw2 = Stopwatch.StartNew();
                var matchRes = FindOverlap(masterScanner, sc);
                sw2.Stop();
                if (matchRes.MatchCount >= OverlapCount)
                {
                    MergeScanners(masterScanner, sc, matchRes);
                    Log($"Matched scanner {sc.Id}. Scanners left: {queue.Count}. Time: {sw2.ElapsedMilliseconds}");
                    testedSinceMatch = 0;
                }
                else
                {
                    testedSinceMatch++;
                    Log($"Requeueing scanner {sc.Id}. Time: {sw2.ElapsedMilliseconds}");
                    queue.Enqueue(sc);
                }

                if (testedSinceMatch > queue.Count)
                    throw new Exception("No matching scanner data, maybe we need to match against another scanner, not just the first");
            }

            part1 = scanners.First().Beacons.Count;
            LogAndReset("*1", sw);
            for (int i = 0; i < _globalOffsets.Count; i++)
                for (int j = 0; j < _globalOffsets.Count; j++)
                    part2 = Math.Max(part2, _globalOffsets[i].Distance(_globalOffsets[j]));
            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private void MergeScanners(Scanner masterScanner, Scanner sc, MatchRes matchRes)
        {
            foreach (var c in matchRes.RotatedAndTranslatedCoords)
            {
                if (!masterScanner.Beacons.Contains(c))
                    masterScanner.Beacons.Add(c);
            }
        }

        private MatchRes FindOverlap(Scanner masterScanner, Scanner sc)
        {
            // var allRotationsOfCoord0 = _rotations.Select(r => sc.Beacons.First().Rotate(r)).ToList();
            //
            // var x = allRotationsOfCoord0.ToHashSet();
            foreach (var r in sc.Rotations)
            {
                // if (r.First().Equals(masterScanner.Beacons.First()))
                // {
                //     Log("Found matching coord 0");
                // }
                var res = FindOverlapRotated(masterScanner, sc, r);
                if (res.MatchCount >= OverlapCount)
                    return res;
            }

            return new MatchRes(0, new List<Coord3d>());
        }

        private MatchRes FindOverlapRotated(Scanner masterScanner, Scanner sc, List<Coord3d> rotatedCoords)
        {
            Log($"Testing scanner {masterScanner.Id} and {sc.Id}, {masterScanner.Beacons.Count}x{rotatedCoords.Count} = {masterScanner.Beacons.Count*rotatedCoords.Count} ");
            foreach (var c in masterScanner.Beacons)
                foreach (var c2 in rotatedCoords)
                {
                    var res = FindRelativeMatches(masterScanner, c, rotatedCoords, c2);
                    if (res.MatchCount >= OverlapCount)
                        return res;
                }
            return new MatchRes(0, rotatedCoords);
        }

        private MatchRes FindRelativeMatches(Scanner masterScanner, Coord3d p1, List<Coord3d> relativeCoords, Coord3d p2)
        {
            var offset = new Coord3d(p1.x - p2.x, p1.y - p2.y, p1.z - p2.z);
            var translatedCoords = relativeCoords.Select(c => c.Move(offset)).ToList();

            var targetCoords = masterScanner.Beacons.ToHashSet();
            var matchCount = 0;
            var beaconCount = 0;
            foreach (var c in translatedCoords)
            {
                beaconCount++;
                if (targetCoords.Contains(c))
                    matchCount++;
                if (matchCount >= OverlapCount)
                {
                    _globalOffsets.Add(offset);
                    return new MatchRes(matchCount, translatedCoords);
                }

                if (relativeCoords.Count - beaconCount + matchCount < OverlapCount)
                    break;
            }

            return new MatchRes(matchCount, new List<Coord3d>());
        }


        private Scanner ParseScanner(IList<string> list, int id)
        {
            var beacons = new List<Coord3d>();
            foreach (var s in list.Skip(1))
                beacons.Add(Coord3d.Parse(s));
            return new Scanner(id, beacons);
        }
    }

    internal class MatchRes
    {
        public int MatchCount { get; }

        public MatchRes(int matchCount, List<Coord3d> rotatedAndTranslatedCoords)
        {
            MatchCount = matchCount;
            RotatedAndTranslatedCoords.AddRange(rotatedAndTranslatedCoords);
        }

        public List<Coord3d> RotatedAndTranslatedCoords { get; } = new List<Coord3d>();
    }

    internal class Scanner
    {
        private readonly List<List<Coord3d>> _rotations = new List<List<Coord3d>>();
        public int Id { get; }
        public List<Coord3d> Beacons { get; }
        public List<List<Coord3d>> Rotations => _rotations;

        public Scanner(int id, List<Coord3d> beacons)
        {
            Id = id;
            Beacons = beacons;
#if dotnet4x
            foreach (var r in Coord3d.AllRotations)
                _rotations.Add(beacons.Select(c => c.Rotate(r)).ToList());
#endif
        }

    }
}