using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media.Media3D;
using Accord;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2021
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day19 : TestBaseClass<Part1Type, Part2Type>
    {
        private List<AxisAngleRotation3D> _rotations = new List<AxisAngleRotation3D>();
        private List<Coord3d> _globalOffsets = new List<Coord3d>();
        public bool Debug { get; set; }

        public Day19()
        {
            GenerateRotations();
        }

        [Test]
        [TestCase(6, null, "Day19_test2.txt", 6)]
        [TestCase(3, null, "Day19_test2_tiny.txt", 2)]
        [TestCase(79, 3621, "Day19_test.txt", 12)]
        [TestCase(367, null, "Day19.txt", 12)]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName, int overlapCount)
        {
            OverlapCount = overlapCount;
            LogLevel = resourceName.Contains("test") ? 20 : -1;
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

            while (scanners.Count > 1)
            {
                var masterScanner = scanners.First();
                var found = false;
                foreach (var sc in scanners.Skip(1))
                {
                    var matchRes = FindOverlap(masterScanner, sc);
                    if (matchRes.MatchCount >= OverlapCount)
                    {
                        MergeScanners(masterScanner, sc, matchRes);
                        scanners.Remove(sc);
                        found = true;
                        break;
                    }
                }

                if (!found)
                    throw new Exception("No matching scanner data, maybe we need to match any scanner, not just the first");
            }

            part1 = scanners.First().Coords.Count;
            LogAndReset("*1", sw);
            for (int i=0; i<_globalOffsets.Count; i++)
            for (int j = 0; j < _globalOffsets.Count; j++)
                part2 = Math.Max(part2, _globalOffsets[i].Distance(_globalOffsets[j]));
            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private void MergeScanners(Scanner masterScanner, Scanner sc, MatchRes matchRes)
        {
            foreach (var c in matchRes.RotatedAndTranslatedCoords)
            {
                if (!masterScanner.Coords.Contains(c))
                    masterScanner.Coords.Add(c);
            }
        }

        private MatchRes FindOverlap(Scanner masterScanner, Scanner sc)
        {
            var allRotationsOfCoord0 = _rotations.Select(r => sc.Coords.First().Rotate(r)).ToList();

            var x = allRotationsOfCoord0.ToHashSet();
            foreach (var r in _rotations)
            {
                var rotatedCoords = sc.Coords.Select(c => c.Rotate(r)).ToList();
                if (rotatedCoords.First().Equals(masterScanner.Coords.First()))
                {
                    Log("Found matching coord 0");
                }
                var res = FindOverlapRotated(masterScanner, sc, rotatedCoords);
                if (res.MatchCount >= OverlapCount)
                    return res;
            }

            return new MatchRes(0, new List<Coord3d>());
        }

        private MatchRes FindOverlapRotated(Scanner masterScanner, Scanner sc, List<Coord3d> rotatedCoords)
        {
            foreach (var c in masterScanner.Coords)
                foreach (var c2 in rotatedCoords)
                {
                    var res = FindRelativeMatches(masterScanner, c, rotatedCoords, c2);
                    if (res.MatchCount >= OverlapCount)
                        return res;
                }
            var matching = new List<Coord3d>();
            return new MatchRes(0, rotatedCoords);
        }

        private MatchRes FindRelativeMatches(Scanner masterScanner, Coord3d p1, List<Coord3d> relativeCoords, Coord3d p2)
        {
            var offset = new Coord3d(p1.x - p2.x, p1.y - p2.y, p1.z - p2.z);
            var translatedCoords = relativeCoords.Select(c => c.Move(offset)).ToHashSet();
            var matchCount = 0;
            foreach (var c in masterScanner.Coords)
            {
                if (translatedCoords.Contains(c))
                    matchCount++;
                if (matchCount >= OverlapCount)
                {
                    _globalOffsets.Add(offset);
                    return new MatchRes(matchCount, translatedCoords.ToList());
                }
            }

            return new MatchRes(matchCount, new List<Coord3d>());
        }

        private void GenerateRotations()
        {
            // var vectors = new List<Vector3D>();
            // vectors.Add(new Vector3D(1, 0, 0));
            // vectors.Add(new Vector3D(-1, 0, 0));
            // vectors.Add(new Vector3D(0, 1, 0));
            // vectors.Add(new Vector3D(0, -1, 0));
            // vectors.Add(new Vector3D(0, 0, 1));
            // vectors.Add(new Vector3D(0, 0, -1));
            // foreach (var axis in vectors)
            // {
            //     for (int i = 0; i <= 3; i++)
            //         _rotations.Add(new AxisAngleRotation3D(axis, i * 90));
            // }

            _rotations.Add(new AxisAngleRotation3D(new Vector3D(1,0,0), 0));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(0,1,0), 90));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(0,1,0), 180));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(0,1,0), -90));

            var a = 0.5774;
            var b = 0.7071;
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(0,0,1), 90));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(a,a,a), 120));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(b,b,0), 180));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(-a,-a,a), 120));

            _rotations.Add(new AxisAngleRotation3D(new Vector3D(0,0,-1), 90));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(-a,a,-a), 120));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(-b,b,0), 180));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(a,-a,-a), 120));

            _rotations.Add(new AxisAngleRotation3D(new Vector3D(1,0,0), 90));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(a,a,-a), 120));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(0,b,-b), 180));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(a,-a,a), 120));

            _rotations.Add(new AxisAngleRotation3D(new Vector3D(1,0,0), 180));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(b,0,-b), 180));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(0,0,1), 180));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(b,0,b), 180));

            _rotations.Add(new AxisAngleRotation3D(new Vector3D(-1,0,0), 90));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(-a,a,a), 120));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(0,b,b), 180));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(-a,-a,-a), 120));
        }

        private Scanner ParseScanner(IList<string> list, int id)
        {
            var scanner = new Scanner(id);
            foreach (var s in list.Skip(1))
                scanner.Coords.Add(Coord3d.Parse(s));
            return scanner;
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
        public int Id { get; }
        public List<Coord3d> Coords { get; } = new List<Coord3d>();

        public Scanner(int id)
        {
            Id = id;
        }
    }
}