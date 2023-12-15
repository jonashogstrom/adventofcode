using System;
using System.Diagnostics;
using NUnit.Framework;
using System.Collections.Generic;
using AdventofCode.Utils;
using System.Linq;
using Windows.Devices.PointOfService;
using ABI.Windows.UI.ApplicationSettings;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.ObjectModel;


namespace AdventofCode.AoC_2023
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day15 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(1320, 145, "Day15_test.txt")]
        [TestCase(52, null, "HASH")]
        [TestCase(495972, 245223, "Day15.txt")]
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

            var lines = source[0].Split(',');

            LogAndReset("Parse", sw);

            foreach (var p in lines)
            {
                part1 += Hash(p);
            }

            LogAndReset("*1", sw);

            var hashMap = new List<LinkedList<(string, int)>>();
            for (var i = 0; i < 256; i++)
                hashMap.Add(new LinkedList<(string, int)>());

            foreach (var p in lines)
            {
                var label = "";
                var op = Op.Unknown;
                var focalLength = 0;

                if (p.EndsWith('-'))
                {
                    label = p[..^1];
                    op = Op.Remove;
                }
                else if (p.Contains('='))
                {
                    op = Op.Add;
                    var temp = p.Split('=');
                    label = temp[0];
                    focalLength = int.Parse(temp[1]);
                }

                var boxNumber = Hash(label);

                var box = hashMap[boxNumber];
                var n = box.First;
                switch (op)
                {
                    case Op.Add:
                        while (n != null && n.Value.Item1 != label)
                            n = n.Next;
                        if (n != null)
                            n.Value = (label, focalLength);
                        else
                            box.AddLast((label, focalLength));
                        break;
                    case Op.Remove:
                        while (n != null && n.Value.Item1 != label)
                            n = n.Next;
                        if (n != null)
                            box.Remove(n);

                        break;
                }

                if (LogLevel > 0)
                {
                    Log(() => $"After \"{p}\":");
                    for (int i = 0; i < 256; i++)
                        if (hashMap[i].Count > 0)
                        {
                            Log(() => $"Box: {i}: " +
                                      string.Join(' ', hashMap[i].Select(x => $"[{x.Item1} {x.Item2}]")));
                        }
                }
            }

            for (var boxnum = 0; boxnum < 256; boxnum++)
            {
                var slot = 0;

                foreach (var entry in hashMap[boxnum])
                {
                    var power = (boxnum + 1) * (slot + 1) * entry.Item2;
                    Log(() => $"{entry.Item1}: {boxnum + 1} (box {boxnum}) * {slot + 1} (xxx slot) * {entry.Item2} (focal length) = {power}");
                    part2 += power;
                    slot++;
                }
            }

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private int Hash(string s)
        {
            return s.Aggregate(0, (current, c) => (current + c) * 17 % 256);
        }
    }

    internal enum Op
    {
        Add, Remove, Unknown
    }
}