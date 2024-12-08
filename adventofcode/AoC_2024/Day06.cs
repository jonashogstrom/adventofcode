﻿using System;
using System.Diagnostics;
using NUnit.Framework;
using System.Collections.Generic;
using AdventofCode.Utils;
using System.Linq;


namespace AdventofCode.AoC_2024
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    // *2, 1845 is too high
    [TestFixture]
    class Day06 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(41, 6, "Day06_test.txt")]
        [TestCase(5086, null, "Day06.txt")]
        [TestCase(4789, 1304, "Day06_jay1.txt")]
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
            var start = map.Keys.Single(k => map[k] != '#');
            var startDir = Coord.CharToDir(map[start]);
            
            var dir = startDir;
            var pos = start;
            LogAndReset("Parse", sw);
            var path = new HashSet<Coord>();
            while (map.InsideBounds(pos))
            {
                path.Add(pos);
                if (map[pos.Move(dir)] == '#')
                    dir = dir.RotateCW90();
//                map[pos] = Coord.trans2Arrow[dir];
                pos = pos.Move(dir);
            }

            part1 = path.Count(); 

            LogAndReset("*1", sw);
            
            var mapCounter = 0;
            var obstacles = new List<Coord>();
            var testable = path.Where(k => !k.Equals(start)).ToArray();
            foreach (var obstacle in testable)
            {
//                map = source.ToSparseBuffer('.');
                mapCounter++;
                map[obstacle] = 'O';
                // Log(()=>$"=== Attempt {mapCounter} ===");
                // Log(()=>map.ToString());
                //
                var pathWithDir = new HashSet<(Coord, Coord)>();
                dir = startDir;
                pos = start;
                while (map.InsideBounds(pos))
                {
                    if (pathWithDir.Contains((pos, dir)))
                    {
                        // cycle found
                        part2++;
                        // Log(()=>$"=== Solution {part2} ===");
                        // Log(()=>map.ToString());
                        obstacles.Add(obstacle);
                        break;
                    }

                    pathWithDir.Add((pos, dir));
                    
                    if (map[pos.Move(dir)] == '#' || map[pos.Move(dir)] == 'O')
                        dir = dir.RotateCW90();
//                    map[pos] = Coord.trans2Arrow[dir];
                    pos = pos.Move(dir);
                }

                map[obstacle] = '.';
            }

            var test = obstacles.ToHashSet();
            LogAndReset("*2", sw);

            return (part1, part2);
        }
    }
}