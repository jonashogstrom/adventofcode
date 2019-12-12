using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Api;

namespace AdventofCode.AoC_2016
{
    [TestFixture]
    internal class Day1 : BaseDay
    {
        [Test]
        [TestCase(InputSource.test, "R2, L3", 5)]
        [TestCase(InputSource.test, "R5, L5, R5, R3", 12)]
        [TestCase(InputSource.test, "R2, R2, R2", 2)]
        [TestCase(InputSource.test, "R8, R4, R4, R8", null, 4)]
        [TestCase(InputSource.prod, "R1, R3, L2, L5, L2, L1, R3, L4, R2, L2, L4, R2, L1, R1, L2, R3, L1, L4, R2, L5, R3, R4, L1, R2, L1, R3, L4, R5, L4, L5, R5, L3, R2, L3, L3, R1, R3, L4, R2, R5, L4, R1, L1, L1, R5, L2, R1, L2, R188, L5, L3, R5, R1, L2, L4, R3, R5, L3, R3, R45, L4, R4, R72, R2, R3, L1, R1, L1, L1, R192, L1, L1, L1, L4, R1, L2, L5, L3, R5, L3, R3, L4, L3, R1, R4, L2, R2, R3, L5, R3, L1, R1, R4, L2, L3, R1, R3, L4, L3, L4, L2, L2, R1, R3, L5, L1, R4, R2, L4, L1, R3, R3, R1, L5, L2, R4, R4, R2, R1, R5, R5, L4, L1, R5, R3, R4, R5, R3, L1, L2, L4, R1, R4, R5, L2, L3, R4, L4, R2, L2, L4, L2, R5, R1, R4, R3, R5, L4, L4, L5, L5, R3, R4, L1, L3, R2, L2, R1, L3, L5, R5, R5, R3, L4, L2, R4, R5, R1, R4, L3", 307, 165)]
        public void Test_Day1(InputSource source, string fileName, object expectedPart1 = null, object expectedPart2 = null)
        {
            RunTest(source, fileName, expectedPart1, expectedPart2);
        }

        public int TestLogLevel = 5;

        public int ProdLogLevel = 0;

        private void RunTest(InputSource source, string fileName, object expectedPart1, object expectedPart2)
        {
            LogLevel = source == InputSource.test ? TestLogLevel : ProdLogLevel;
            string[] input;
            if (fileName.EndsWith(".txt"))
            {
                if (!File.Exists(fileName))
                {
                    File.WriteAllText(fileName, "");
                    return;
                }

                input = File.ReadAllLines(fileName);
            }
            else
                input = new[] { fileName };

            _addLogHeader = false;

            var sw = new Stopwatch();
            sw.Start();
            DoRun(input);

            sw.Stop();
            _addLogHeader = true;
            PrintSplitter();
            var p1 = LogAndCompareExpected("Part1", Part1, expectedPart1);
            var p2 = LogAndCompareExpected("Part2", Part2, expectedPart2);
            PrintFooter(sw);
            File.WriteAllText(GetType().Name + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "_" + (UseTestData ? "TEST" : "PROD") + _fileNameSuffix + ".log", _log.ToString());
            Assert.That(p1, Is.EqualTo(true), "Part 1 failed");
            Assert.That(p2, Is.EqualTo(true), "Part 2 failed");
        }

        protected override void DoRun(string[] input)
        {
            //Part2, 310 TOO HIGH
            var pos = Coord.Origin;
            var p = input[0].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
            var dir = Coord.N;
            var i = 0;
            HashSet<Coord> places = new HashSet<Coord>();
            foreach (var instr in p)
            {
                if (instr[0] == 'R')
                    dir = dir.RotateCW90();
                else
                    dir = dir.RotateCCW90();
                var d = int.Parse(instr.Substring(1));
                for (int x = 0; x < d; x++)
                {
                    pos = pos.Move(dir, 1);
                    if (Part2 == null && places.Contains(pos))
                    {
                        Log($"{i}: REPEAT", 1);
                        Part2 = Coord.Origin.Dist(pos);
                    }

                    places.Add(pos);
                }
                Log($"{i}: {instr} => {pos.Row},{pos.Col}", 2);
                i++;
            }

            Part1 = Coord.Origin.Dist(pos);
        }
    }
}