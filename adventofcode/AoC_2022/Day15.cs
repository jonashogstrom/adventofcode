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
    class Day15 : TestBaseClass<Part1Type, Part2Type>
    {
        private int _row;
        private int _distLimit;
        public bool Debug { get; set; }

        [Test]
        [TestCase(26, 56000011, 10, 20, "Day15_test.txt")]
        [TestCase(5838453, 12413999391794, 2000000, 4000000, "Day15.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, int row, int distLimit, string resourceName)
        {
            LogLevel = resourceName.Contains("test") ? 20 : -1;
            var source = GetResource(resourceName);
            _row = row;
            _distLimit = distLimit;
            var res = ComputeWithTimer(source);

            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1 = 0;
            Part2Type part2 = 0;
            var sw = Stopwatch.StartNew();
            var pairs = new List<(Coord sensor, Coord beacon)>();
            foreach (var s in source)
            {
                var parts = s.Split(' ', ',', ':', '=');
                var sensor = new Coord(int.Parse(parts[6]), int.Parse(parts[3]));
                var beacon = new Coord(int.Parse(parts[16]), int.Parse(parts[13]));
                pairs.Add((sensor, beacon));
            }
            // parse input here

            LogAndReset("Parse", sw);

            // solve part 1 here

            LogAndReset("*1", sw);

            var distances = new Dictionary<Coord, int>();
            foreach (var p in pairs)
            {
                var dist = p.sensor.Dist(p.beacon);
                if (distances.TryGetValue(p.sensor, out var oldDist))
                {
                    distances[p.sensor] = Math.Min(oldDist, dist);
                }
                else
                {
                    distances[p.sensor] = dist;
                }
            }

            var blockedCoords = new HashSet<Coord>();
            foreach (var sensor in distances.Keys)
            {
                var distToRow = Math.Abs(sensor.Row - _row);
                var spaceAroundSensor = distances[sensor];
                var width = (spaceAroundSensor - distToRow) + 1;
                for (var i = 0; i < width; i++)
                {
                    blockedCoords.Add(new Coord(_row, sensor.X + i));
                    blockedCoords.Add(new Coord(_row, sensor.X - i));
                }
            }

            foreach (var p in pairs)
                blockedCoords.Remove(p.beacon);

            part1 = blockedCoords.Count;

            for (var y = 0; y < _distLimit; y++)
            {
                var intervals = new List<(int start, int stop)>();

                foreach (var sensor in distances.Keys)
                {
                    var distToRow = Math.Abs(sensor.Row - y);
                    var spaceAroundSensor = distances[sensor];
                    var width = (spaceAroundSensor - distToRow);
                    if (width >= 0)
                    {
                        var start = Math.Max(0, sensor.X - width);
                        var stop = Math.Min(_distLimit, sensor.X + width);
                        AddInterval(intervals, (start, stop));
                        if (intervals[0].start == 0 && intervals[0].stop == _distLimit)
                            break;
                        //Log(() => $"Row {y}, Sensor {sensor}: covers {start}>{stop} (sensorspace: {spaceAroundSensor})");
                    }

                }

                if (intervals.Count > 1)
                {
                    var x = intervals.First().stop + 1;
                    part2 = x * 4000000L + y;
                    Log($"Found location: x={x} y={y} freq = {part2}");
                    break;
                }
            }

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private void AddInterval(List<(int start, int stop)> intervals, (int start, int stop) newInterval)
        {
            for (int i = 0; i < intervals.Count; i++)
            {
                var (start, stop) = intervals[i];
                // no overlap
                if (newInterval.stop < start || newInterval.start > stop)
                    continue;
                var mergedInterval = (Math.Min(newInterval.start, start),
                    Math.Max(newInterval.stop, stop));
                intervals.RemoveAt(i);
                AddInterval(intervals, mergedInterval);
                return;
            }
            intervals.Add(newInterval);
        }
    }
}
