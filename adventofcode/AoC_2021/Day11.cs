using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2021
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day11 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(1656, 195, "Day11_test.txt")]
        [TestCase(1594, 437, "Day11.txt")]
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

            var map = source.ToSparseBuffer(9, c => int.Parse(c.ToString()));


            LogAndReset("Parse", sw);
            for (int i = 0; i < 100; i++)
                part1 += Evolve(map);
            LogAndReset("*1", sw);

            var gen = 0;
            map = source.ToSparseBuffer(9, c => int.Parse(c.ToString()));
//            var videoGenerator = new VideoGen<int>("Day11.mp4", map.Width, map.Height, ValueToColor);
            var expCount = map.Keys.Count();
            while (Evolve(map) != expCount)
            {
                gen++;
//                videoGenerator.AddSparseMap(map);
            }

            //videoGenerator.Flush();
            part2 = gen + 1;
            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private static Color ValueToColor(int value)
        {
            var col = value == 0 ? 
                Color.FromArgb(0, 0, 255, 0) : 
                Color.FromArgb(0, (byte)(value * 20), 0, 0);
            return col;
        }


        private int Evolve(SparseBuffer<int> map)
        {
            var flashCount = 0;
            var flashQueue = new Queue<Coord>();
            foreach (var c in map.Keys.ToList())
            {
                map[c]++;
                if (map[c] > 9)
                    flashQueue.Enqueue(c);
            }

            var resetList = new List<Coord>();
            while (flashQueue.Any())
            {
                var c = flashQueue.Dequeue();
                resetList.Add(c);
                foreach (var n in c.GenAdjacent8())
                {
                    if (map.InsideBounds(n)) 
                        map[n]++;
                    if (map[n] == 10)
                        flashQueue.Enqueue(n);
                }

                flashCount++;
            }

            foreach (var c in resetList)
                map[c] = 0;
            return flashCount;
        }
    }
}