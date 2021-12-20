using System;
using System.Diagnostics;
using System.Linq;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2021
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day20 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(35, 3351, "Day20_test.txt")]
        [TestCase(35, 3351, "Day20_test2.txt")]
        [TestCase(5483, 18732, "Day20.txt")]
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
            var g = source.AsGroups();
            var enhance = g.First().First().Select(c => c == '#' ? 1 : 0).ToArray();
            var img = g.Skip(1).First().ToArray().ToSparseBuffer<int>(0, c => c == '#' ? 1 : 0);
            Log(img.ToString(i => i == 1 ? "#" : "."));
            LogAndReset("Parse", sw);
            for (int gen = 1; gen <= 50; gen++)
            {
                var def = img.Default == 0 ? enhance[0] : enhance[511];
                img = Evolve(enhance, img, def);
                var pixelCount = img.Keys.Count(c => img[c] == 1);
                Log($"Gen {gen} pixelcount:" + pixelCount);
                Log(img.ToString(i => i == 1 ? "#" : "."));
                if (gen == 2)
                {
                    part1 = pixelCount;
                    LogAndReset("*1", sw);
                }
            }

            part2 = img.Keys.Count(c => img[c] == 1);
            LogAndReset("*2", sw);
            // not 20330 
            return (part1, part2);
        }

        private SparseBuffer<int> Evolve(int[] enhance, SparseBuffer<int> img, int def)
        {
            var newImg = new SparseBuffer<int>(def);
            var bbMargin = 1;
            for (var row = img.Top - bbMargin; row <= img.Bottom + bbMargin; row++)
                for (var col = img.Left - bbMargin; col <= img.Right + bbMargin; col++)
                {
                    var bin = 0;
                    for (var y = -1; y < 2; y++)
                        for (var x = -1; x < 2; x++)

                        {
                            var coord = Coord.FromXY(col + x, row + y);
                            var newPixel = img[coord];
                            bin *= 2;
                            bin += newPixel;
                        }

                    var c = Coord.FromXY(col, row);
                    newImg[c] = enhance[bin];
                }

            return newImg;
        }
    }
}