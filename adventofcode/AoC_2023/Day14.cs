using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Devices.Bluetooth.Advertisement;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2023
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day14 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(136, 64, "Day14_test.txt")]
        [TestCase(108641, 95467, "Day14.txt")]
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


            var map = source.ToSparseBuffer('.');

            LogAndReset("Parse", sw);

            TiltMapN(map);
            part1 += CalculateLoad(map);

            LogAndReset("*1", sw);

            map = source.ToSparseBuffer('.');
            //
            var loads = new List<long>();

            var iteration = -1;
            var cycle = -1;
            while (true)
            {
                //                for (var i = 0L; i < max - skipcycles * cycle; i++)
                //            {
                TiltMapN(map);
                TiltMapW(map);
                TiltMapS(map);
                TiltMapE(map);
                var load = CalculateLoad(map);
                loads.Add(load);
                iteration++;
                Log($"Round: {iteration}, load: {load}", -1);
                if (FindCycle(loads, out cycle))
                    break;
            }
            var max = 1000000000;
            var skipcycles = max / cycle;

            var part2Pos = max - skipcycles * cycle -1;
            part2 = loads[part2Pos];

            //part2 = CalculateLoad(map);

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private bool FindCycle(List<long> loads, out int cycle)
        {
            for (int i = 2; i < loads.Count / 2; i++)
            {
                var last = loads.GetRange(loads.Count - i, i);
                var prev = loads.GetRange(loads.Count - i*2, i);
                if (Enumerable.SequenceEqual(last, prev))
                {
                    cycle = i;
                    return true;
                }
            }

            cycle = -1;
            return false;
        }

        private static long CalculateLoad(SparseBuffer<char> map)
        {
            var load = 0L;
            foreach (var k in map.Keys)
                if (map[k] == 'O')
                    load += map.Bottom - k.Row + 1;
            return load;
        }

        private void TiltMapN(SparseBuffer<char> map)
        {
            for (int r = 0; r <= map.Bottom; r++)
                for (int c = 0; c <= map.Right; c++)
                    CheckMove(map, new Coord(r, c), Coord.N);
        }

        private void TiltMapW(SparseBuffer<char> map)
        {
            for (int c = 0; c <= map.Right; c++)
                for (int r = 0; r <= map.Bottom; r++)
                    CheckMove(map, new Coord(r, c), Coord.W);
        }

        private void TiltMapS(SparseBuffer<char> map)
        {
            for (int r = map.Bottom; r >= 0; r--)
                for (int c = 0; c <= map.Right; c++)
                    CheckMove(map, new Coord(r, c), Coord.S);
        }
        private void TiltMapE(SparseBuffer<char> map)
        {
            for (int c = map.Right; c >= 0; c--)
                for (int r = 0; r <= map.Bottom; r++)
                    CheckMove(map, new Coord(r, c), Coord.E);
        }

        private void CheckMove(SparseBuffer<char> map, Coord coord, Coord dir)
        {
            if (map[coord] == 'O')
            {
                var next = coord.Move(dir);
                while (next.Row >= 0 && next.Row <= map.Bottom &&
                       next.Col >= 0 && next.Col <= map.Right && map[next] == '.')
                {
                    map[coord] = '.';
                    map[next] = 'O';
                    coord = next;
                    next = coord.Move(dir);
                }
            }
        }
    }
}