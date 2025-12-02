using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2025;

// to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
using Part1Type = Int64;
using Part2Type = Int64;

[TestFixture]
class Day01 : TestBaseClass<Part1Type, Part2Type>
{
    public bool Debug { get; set; }

    [Test]
    [TestCase(3, 6, "Day01_test.txt")]
    [TestCase(1055, 6386, "Day01.txt")]
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

        // parse input here

        LogAndReset("Parse", sw);

        var pos1 = 50;
        var pos2 = 50;
        foreach (var l in source)
        {
            // line parsing
            var dir = l[0] == 'R' ? +1 : -1;
            var dist = int.Parse(l[1..]);

            var oldpos = pos1;
            // solve part 1
            pos1 = pos1 + dist * dir;
            if (pos1 % 100 == 0)
                part1++;

            var oldpos2 = pos2;
            // eliminate entire "laps", they will pass 0 exactly once each.
            var laps = dist / 100;
            part2 += laps;
            
            dist -= laps * 100;
            pos2 += dist * dir;
            
            if (oldpos % 100 != 0) // if we start at 0 and make a non-whole lap, we won't end at 0
                if (pos2 % 100 == 0 || // pos2 arrives at a 0 
                    oldpos2 / 100 != pos2 / 100 || // old pos and pos2 are in different laps
                    oldpos2 * pos2 < 0) // oldpos and pos2 has different signs (on different sides of the actual 0
                part2++;

            // naive solution part 2, with loops
            // for (var i = 0; i < dist; i++)
            // {
            //     pos2 = (pos2 + 1 * dir) % 100;
            //     if (pos2 == 0)
            //         part2++;
            // }
            //Console.WriteLine($"{l}: {oldpos} => {pos1} | {part2}");
        }

        LogAndReset("*1", sw);


        LogAndReset("*2", sw);

        return (part1, part2);
    }
}