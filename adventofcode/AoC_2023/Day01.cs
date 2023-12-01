using System;
using System.Diagnostics;
using NUnit.Framework;

namespace AdventofCode.AoC_2023;

using Part1Type = Int64;
using Part2Type = Int64;

[TestFixture]
class Day01 : TestBaseClass<Part1Type, Part2Type>
{
    public bool Debug { get; set; }

    [Test]
    [TestCase(142, null, "Day01_test.txt")]
    [TestCase(null, 281, "Day01_test2.txt")]
    [TestCase(55090, 54845, "Day01.txt")]
    public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
    {
        LogLevel = resourceName.Contains("test") ? 20 : -1;
        var source = GetResource(resourceName);
        var res = ComputeWithTimer(source);
        DoAsserts(res, exp1, exp2, resourceName);
    }

    protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
    {
        var sw = Stopwatch.StartNew();

        LogAndReset("Parse", sw);

        Part1Type part1 = Compute(source, false);

        LogAndReset("*1", sw);

        Part2Type part2 = Compute(source, true);

        LogAndReset("*2", sw);

        return (part1, part2);
    }

    private int Compute(string[] source, bool supportStringNumbers)
    {
        var sum = 0;
        foreach (var s in source)
        {
            var first = FindDigit(s, 0, 1, supportStringNumbers);
            var last = FindDigit(s, s.Length - 1, -1, supportStringNumbers);
            if (first == -1 || last == -1)
                continue;

            int s3 = first * 10 + last;
            Log(s + "=>" + s3);
            sum += s3;
        }

        return sum;
    }

    private int FindDigit(string s, int startPos, int dir, bool supportStringNumbers = false)
    {
        var numbers = new string[]{
            "one", "two", "three", "four", "five", "six", "seven", "eight", "nine"
        };

        var pos = startPos;
        while (pos >= 0 && pos < s.Length)
        {
            if (s[pos] >= '0' && s[pos] <= '9')
            {
                return int.Parse(s[pos].ToString());
            }
            if (supportStringNumbers)
                for (int i = 0; i < numbers.Length; i++)
                {
                    if (s.Length >= pos + numbers[i].Length && s.Substring(pos, numbers[i].Length) == numbers[i])
                    {
                        return i + 1;
                    }
                }

            pos += dir;
        }

        return -1;
    }
}