using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2025
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    // 38750 too low
    [TestFixture]
    internal class Day08 : TestBaseClass<Part1Type, Part2Type>
    {
        private int _connectCount;
        public bool Debug { get; set; }

        [Test]
        [TestCase(40, 25272, 10, "<day>_test.txt")]
        [TestCase(52668, 1474050600, 1000, "<day>.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, int connectCount, string resourceName)
        {
            _connectCount = connectCount;
            LogLevel = resourceName.Contains("test") ? 20 : -1;
            var source = GetResource2(ref resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1 = 0;
            Part2Type part2 = 0;
            var sw = Stopwatch.StartNew();

            var boxes = source.Select(s => new Box(new Coord3d(s))).ToArray();

            LogAndReset("Parse", sw);

            var pairs = new List<BoxPair>();
            for (var i = 0; i < boxes.Length; i++)
            for (var j = i + 1; j < boxes.Length; j++)
            {
                var boxPair = new BoxPair(boxes[i], boxes[j]);
                pairs.Add(boxPair);
            }

            pairs = pairs.OrderBy(p => p.Distance).ToList();
            LogAndReset("MakeAndOrderPairs", sw);

            var connected = 0;
            var circuits = new HashSet<HashSet<Box>>(boxes.Select(b => b.Circuit));

            for (int i = 0; i < pairs.Count; i++)
            {
                var pair = pairs[i];

                if (pair.B1.Circuit == pair.B2.Circuit)
                {
                    // already same circuit
                    Log(() => $"{pair.B1} and {pair.B2} already belong to the same circuit");
                    connected++;
                }
                else if (pair.B1.Circuit != pair.B2.Circuit)
                {
                    // merge circuits
                    var circ1 = pair.B1.Circuit;
                    foreach (var box in circ1)
                    {
                        box.Circuit = pair.B2.Circuit;
                    }

                    circuits.Remove(circ1);
                    connected++;
                    Log(() => $"Merged circuits of {pair.B1} and {pair.B2}");

                    if (circuits.Count == 1)
                    {
                        part2 = pair.B1.Coord.x * pair.B2.Coord.x;
                        Log(() => $"Last connection: {pair.B1} => {pair.B2}, x-dist = {part2}, totalConnections = {connected}", -10);
                        LogAndReset("*2", sw);
                        break;
                    }

                }
                else
                {
                    throw new Exception("what?");
                }

                if (connected == _connectCount)
                {
                    var threeLargest = circuits.OrderByDescending(c => c.Count).Take(3).ToArray();
                    part1 = threeLargest[0].Count * threeLargest[1].Count * threeLargest[2].Count;
                    LogAndReset("*1", sw);
                }
            }


            return (part1, part2);
        }
    }

    [DebuggerDisplay("{B1.Coord.x},{B1.Coord.y},{B1.Coord.z} => {B2.Coord.x},{B2.Coord.y},{B2.Coord.z} = {Distance}")]
    internal class BoxPair
    {
        public readonly Box B1;
        public readonly Box B2;

        public BoxPair(Box b1, Box b2)
        {
            B1 = b1;
            B2 = b2;
            Distance = B1.Coord.StraightLineDist(B2.Coord);
        }

        public double Distance { get; init; }
    }

    [DebuggerDisplay("{ToString()}")]
    internal class Box
    {
        private HashSet<Box> _circuit;

        public Box(Coord3d coord)
        {
            Coord = coord;
            Circuit = [];
        }

        public Coord3d Coord { get; }

        public HashSet<Box> Circuit
        {
            get => _circuit;
            set
            {
                _circuit = value;
                _circuit.Add(this);
            }
        }

        public override string ToString()
        {
            return $"{Coord.x},{Coord.y},{Coord.z}";
        }
    }
}