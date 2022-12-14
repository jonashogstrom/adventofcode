using System;
using System.Diagnostics;
using System.Linq;
using AdventofCode.AoC_2018;
using NUnit.Framework;

namespace AdventofCode.AoC_2022
{
    using static System.Net.Mime.MediaTypeNames;
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day14 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(24, 93, "Day14_test.txt")]
        [TestCase(674, null, "Day14.txt")]
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
            var sourceOfSand = new Coord(0, 500);
            var map = new SparseBuffer<char>('.', sourceOfSand);
            // parse input here
            foreach (var s in source)
            {
                var coords = s.Split(new string[] { " -> " }, StringSplitOptions.None).Select(x => Coord.Parse(x.Trim())).ToArray();
                var pos = coords.First();
                foreach (var next in coords.Skip(1))
                {
                    foreach (var x in pos.PathTo(next, true))
                    {
                        map[x] = '#';
                    }

                    pos = next;
                }
            }

            map[sourceOfSand] = '+';
            Log(map.ToString(c => c.ToString()));
            LogAndReset("Parse", sw);

            var count = 0;
            while (true)
            {
                if (!Pour(map, sourceOfSand))
                    break;
                count++;
                Log(() => map.ToString(c => c.ToString()));
            }



            part1 = count;
            // solve part 1 here

            LogAndReset("*1", sw);

            var maxDepth = map.Bottom + 2;
            while (true)
            {
                if (!Pour2(map, sourceOfSand, maxDepth, sourceOfSand))
                    break;
                count++;
                Log(() => map.ToString(c => c.ToString()));
            }
            // solve part 2 here
            part2 = count;

            LogAndReset("*2", sw);

            return (part1, part2);
        }
        private bool Pour(SparseBuffer<char> map, Coord sandPos)
        {
            while (true)
            {
                if (sandPos.Row > map.Bottom)
                    return false;
                if (map[sandPos.Move(Coord.S)] == '.')
                    sandPos = sandPos.Move(Coord.S);
                else if (map[sandPos.Move(Coord.SW)] == '.')
                    sandPos = sandPos.Move(Coord.SW);
                else if (map[sandPos.Move(Coord.SE)] == '.')
                    sandPos = sandPos.Move(Coord.SE);
                else
                {
                    map[sandPos] = 'O';
                    return true;
                }
            }
        }

        private bool Pour2(SparseBuffer<char> map, Coord sandPos, int maxDepth, Coord sourceOfSand)
        {
            if (map[sourceOfSand] == 'O')
                return false;
            while (true)
            {
                if (sandPos.Y == maxDepth - 1)
                {
                    map[sandPos.Move(Coord.S)] = '-';
                    map[sandPos.Move(Coord.SE)] = '-';
                    map[sandPos.Move(Coord.SW)] = '-';
                }
                if (map[sandPos.Move(Coord.S)] == '.')
                    sandPos = sandPos.Move(Coord.S);
                else if (map[sandPos.Move(Coord.SW)] == '.')
                    sandPos = sandPos.Move(Coord.SW);
                else if (map[sandPos.Move(Coord.SE)] == '.')
                    sandPos = sandPos.Move(Coord.SE);
                else
                {
                    map[sandPos] = 'O';
                    return true;
                }
            }
        }
    }
}