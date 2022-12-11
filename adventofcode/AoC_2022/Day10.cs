using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;

namespace AdventofCode.AoC_2022
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day10 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(13140, null, "Day10_test.txt")]
        [TestCase(0, null, "Day10_test_small.txt")]
        [TestCase(14760, null, "Day10.txt")]
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
            var registerX = 1;
            var registerXHistory = new List<int>();
            registerXHistory.Add(registerX);
            foreach (var instr in source)
            {
                var parts = instr.Split(' ');
                var op = parts[0];
                switch (op)
                {
                    case "addx":
                    {
                        var arg = int.Parse(parts[1]);
                        registerXHistory.Add(registerX);
                        registerX += arg;
                        registerXHistory.Add(registerX);
                        //              Log($"{registerXHistory.Count}: {instr} => regx = {registerX}");
                        break;
                    }
                    case "noop":
                    {
                        registerXHistory.Add(registerX);
                        break;
                    }
                    default: throw new Exception();
                }
            }

            LogAndReset("Parse", sw);

            var cycle = 20;
            var sum = 0;
            while (cycle <= registerXHistory.Count)
            {
                var regValue = registerXHistory[cycle-1];
                var signalStrength = regValue*cycle;
                sum += signalStrength;
                Log($"{cycle}: regX = {regValue} SignalStrength={signalStrength} Sum={sum}");
                cycle += 40;
            }

            part1 = sum;
            LogAndReset("*1", sw);
            var line = "";
            for (int i = 0; i < 240; i++)
            {
                var crtPos = i % 40;
                var regx = registerXHistory[i];
                if (regx >= crtPos - 1 && regx <= crtPos + 1)
                    line += "#";
                else
                {
                    line += ".";
                }

                if ((i + 1) % 40 == 0)
                {
                    Log(line, -1);
                    line = "";
                }
            }

            LogAndReset("*2", sw);

            return (part1, part2);
        }
    }
}