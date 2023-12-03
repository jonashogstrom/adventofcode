using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2023
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day02 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(8, 2286, "Day02_test.txt")]
        [TestCase(3059, null, "Day02.txt")]
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

            var games = new List<Game>();
            foreach (var s in source)
            {
                var p = s.Split(new[] { ':', ';' });
                var g = new Game(int.Parse(p[0].Split(' ').Last().Trim()));
                foreach (var d in p.Skip(1))
                {
                    var draw = new Draw();
                    var cubes = d.Split(',');
                    foreach (var c in cubes)
                    {
                        var info = c.Trim().Split(" ");
                        var count = int.Parse(info[0].Trim());
                        var color = info[1].Trim();
                        draw.cubes[color[0]] = count;
                    }

                    g.draws.Add(draw);
                }
                games.Add(g);
            }


            LogAndReset("Parse", sw);

            var solution = new DicWithDefault<char, int>();
            solution['r'] = 12;
            solution['g'] = 13;
            solution['b'] = 14;
            part1 = games.Where(g => g.Possible(solution)).Sum(g => g.Id);

            LogAndReset("*1", sw);

            part2 = games.Sum(g => g.Power());

            LogAndReset("*2", sw);

            return (part1, part2);
        }
    }

    internal class Game
    {
        public int Id { get; }
        public List<Draw> draws { get; } = new();

        public Game(int id)
        {
            Id = id;
        }

        public bool Possible(DicWithDefault<char, int> p)
        {
            var minCubes = GetMinCubes();
            foreach(var c in p.Keys)
                if (p[c] < minCubes[c])
                    return false;
            return true;
        }

        public int Power()
        {
            var minCubes = GetMinCubes();
            return minCubes['r'] * minCubes['g'] * minCubes['b'];
        }

        private DicWithDefault<char, int> GetMinCubes()
        {
            var minCubes = new DicWithDefault<char, int>();
            foreach (var d in draws)
            foreach (var color in d.cubes.Keys)
            {
                minCubes[color] = Math.Max(minCubes[color], d.cubes[color]);
            }

            return minCubes;
        }
    }

    internal class Draw
    {
        public DicWithDefault<char, int> cubes = new();
        public override string ToString()
        {
            return $"R={cubes['r']} G={cubes['g']} B={cubes['b']}";
        }
    }
}