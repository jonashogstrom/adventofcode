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
            var dir = l[0]=='R' ? +1 : -1;
            var dist = int.Parse(l[1..]);

            // solve part 1
            pos1 = (pos1 + dist * dir) % 100;
            if (pos1 == 0)
                part1++;

            // solve part 2
            for (var i = 0; i < dist; i++)
            {
                pos2 = (pos2 + 1 * dir) % 100;
                if (pos2 == 0)
                    part2++;
            }
        }

        LogAndReset("*1", sw);


        LogAndReset("*2", sw);

        return (part1, part2);
    }
}