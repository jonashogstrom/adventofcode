using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace AdventofCode.AoC_2019
{
    [TestFixture]
    class Day8 : BaseDay
    {
        protected override void Setup()
        {
            Source = InputSource.test;
            Source = InputSource.prod;

            LogLevel = UseTestData ? 5 : 0;

            Part1TestSolution = null;
            Part2TestSolution = null;
            Part1Solution = 1485;
            Part2Solution = null;
        }

        protected override void DoRun(string[] input)
        {
            //var layer = new SparseBuffer<int>();

            Part1 = ParseImage(input[0], 25, 6);
        }

        [Test]
        [TestCase("Day8_test2.txt", 2, 3, 1)]
        [TestCase("Day8.txt", 25, 6, 1485)]
        public void test1(string suffix, int width, int height, int exp)
        {
            var input = GetResource(suffix)[0];

            var res = ParseImage(input, width, height);
            Assert.That(res.Item1, Is.EqualTo(exp));
        }


        private (int, SparseBuffer<char>) ParseImage(string input, int width, int height)
        {
            var layerSize = width * height;
            var layerCount = input.Length / layerSize;
            var min0 = int.MaxValue;
            var factor = 0;

            // not 2491
            var resLayer = new SparseBuffer<char>('2');

            for (int l = 0; l < layerCount; l++)
            {
                var layer = input.Substring(l * layerSize, layerSize);
                var freq = new Dictionary<char, int>();
                freq['0'] = 0;
                freq['1'] = 0;
                freq['2'] = 0;

                foreach (var c in layer)
                {
                    if (!freq.ContainsKey(c))
                        freq[c] = 0;
                    freq[c] = freq[c] + 1;
                }

                Log($"0: {freq['0']} 1: {freq['1']} 2: {freq['2']}");
                if (freq['0'] < min0)
                {
                    factor = freq['1'] * freq['2'];
                    min0 = freq['0'];
                    Log($"===> min {min0} Fac: {factor}");
                }

                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        var coord = new Coord(y, x);
                        var c = layer[x + y * width];
                        if (c != '2' && resLayer[coord] == '2')
                            resLayer[coord] = c;
                    }

                PrintLayer(width, height, resLayer);
            }

            PrintLayer(width, height, resLayer);

            return (factor, resLayer);
        }

        private void PrintLayer(int width, int height, SparseBuffer<char> resLayer)
        {
            var sb = new StringBuilder();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var c = resLayer[new Coord(y, x)];
                    if (c == '1')
                        sb.Append('X');
                    if (c == '0')
                        sb.Append(' ');
                    if (c == '2')
                        sb.Append('?');
                }

                sb.AppendLine();
            }
            Log(new string('=', width));
            Log(sb.ToString());
            Log(new string('=', width));
        }
    }
}