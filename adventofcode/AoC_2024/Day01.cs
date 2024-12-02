using System;
using System.Diagnostics;
using NUnit.Framework;
using System.Collections.Generic;
using AdventofCode.Utils;
using System.Linq;


namespace AdventofCode.AoC_2024;

// to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
using Part1Type = Int64;
using Part2Type = Int64;

[TestFixture]
class Day01 : TestBaseClass<Part1Type, Part2Type>
{
    public bool Debug { get; set; }

    [Test]
    [TestCase(11, 31, "Day01_test.txt")]
    [TestCase(1197984, 23387399, "Day01.txt")]
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

        var l1 = new List<long>();
        var l2 = new List<long>();
        foreach (var s in source)
        {
            var parts = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            l1.Add(long.Parse(parts[0]));
            l2.Add(long.Parse(parts[1]));
        }


        LogAndReset("Parse", sw);
        var sorted1 = l1.OrderBy(x => x).ToList();
        var sorted2 = l2.OrderBy(x => x).ToList();

        var sum = 0L;
        for (var i = 0; i < sorted1.Count; i++)
        {
            var diff = Math.Abs(sorted1[i]-sorted2[i]);
            sum += diff;
        }

        part1 = sum;
        
        LogAndReset("*1", sw);

        var g2 = l2.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());

        part2 = 0;
        foreach (var v1 in l1)
        {
            if (g2.TryGetValue(v1, out int count))
                part2 += v1 * count;
        }
        
        LogAndReset("*2", sw);

        return (part1, part2);
    }
}