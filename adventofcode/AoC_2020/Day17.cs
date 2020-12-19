using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;

namespace AdventofCode.AoC_2020
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day17 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(112, 848, "Day17_test.txt")]
        [TestCase(273, 1504, "Day17.txt")]
        [Repeat(5)]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {

            var sw = Stopwatch.StartNew();

            var genWorld3d = new DicWithDefault<GenCoord, char>('.');
            var genWorld4d = new DicWithDefault<GenCoord, char>('.');
            for (int x = 0; x < source[0].Length; x++)
                for (int y = 0; y < source.Length; y++)
                {
                    var ch = source[y][x];
                    genWorld3d[new GenCoord(new List<int> { x, y, 0 })] = ch;
                    genWorld4d[new GenCoord(new List<int> { x, y, 0, 0 })] = ch;
                }

            LogAndReset("Parse", sw);

            for (int i = 0; i < 6; i++)
            {
                genWorld3d = ExecuteGenDLife(genWorld3d);
            }

            var part1 = genWorld3d.Count('#');

            LogAndReset("*1", sw);

            for (int i = 0; i < 6; i++)
            {
                genWorld4d = ExecuteGenDLife(genWorld4d);
            }

            var part2 = genWorld4d.Count('#');

            LogAndReset("*2", sw);
            return (part1, part2);
        }

        private DicWithDefault<GenCoord, char> ExecuteGenDLife(DicWithDefault<GenCoord, char> world)
        {
            var res = new DicWithDefault<GenCoord, char>('.');
            var neighborCounter = new DicWithDefault<GenCoord, int>(0);
            foreach (var c in world.Keys)
            {
                if (world[c] == '#')
                    foreach (var n in c.Neighbors())
                    {
                        neighborCounter[n]++;
                    }
            }

            foreach (var c in neighborCounter.Keys)
            {
                var neighbors = neighborCounter[c];
                if (neighbors == 3 && world[c] == '.')
                    res[c] = '#';
                else if ((neighbors == 2 || neighbors == 3) && world[c] == '#')
                    res[c] = '#';
            }

            return res;
        }
    }
}
