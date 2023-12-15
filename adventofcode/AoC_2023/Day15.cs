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
        [TestCase(495972, null, "Day15.txt")]
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

//            LogLevel = 20;

            var hashMap = new List<List<(string, int)>>();
            for (int i = 0; i < 256; i++)
                hashMap.Add(new List<(string, int)>());
            foreach (var p in lines)
            {
                var label = "";
                var op = Op.Unknown;
                var focalLength = 0;

                if (p.EndsWith('-'))
                {
                    label = p.Substring(0, p.Length - 1);
                    op = Op.Remove;
                }
                else if (p.Contains('='))
                {
                    op = Op.Add;
                    var temp = p.Split('=');
                    label = temp[0];
                    focalLength = int.Parse(temp[1]);
                }

                var boxnumber = Hash(label);

                var box = hashMap[boxnumber];
                switch (op)
                {
                    case Op.Add:
                        var found = false;
                        var ix = 0;
                        foreach (var v in box)
                        {
                            if (v.Item1 == label)
                            {
                                box[ix] = (label, focalLength);
                                found = true;
                                break;
                            }
                            ix++;
                        }

                        if (!found)
                            box.Add((label, focalLength));
                        break;
                    case Op.Remove:
                        var ix2 = 0;
                        foreach (var v in box)
                        {
                            if (v.Item1 == label)
                            {
                                box.RemoveAt(ix2);
                                break;
                            }

                            ix2++;
                        }

                        break;
                    default: throw new Exception();
                }
                Log($"After \"{p}\":");
                for (int i = 0; i < 256; i++)
                    if (hashMap[i].Count > 0)
                    {
                        Log($"Box: {i}: " + string.Join(' ', hashMap[i].Select(x => $"[{x.Item1} {x.Item2}]")));
                    }
            }

            for (var boxnum = 0; boxnum < 256; boxnum++)
            for (var slot = 0; slot < hashMap[boxnum].Count; slot++)
            {
                var entry = hashMap[boxnum][slot];
                var power = (boxnum + 1) * (slot + 1) * entry.Item2;
                Log($"{entry.Item1}: {boxnum+1} (box {boxnum}) * {slot+1} (xxx slot) * {entry.Item2} (focal length) = {power}", -1);
                part2 += power;
            }


            // not 296701
            // not 327061
            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private int Hash(string s)
        {
            var hash = 0;
            foreach (var c in s)
            {
                hash += (int)c;
                hash *= 17;
                hash = hash % 256;
            }

            return hash;

        }
    }

    internal enum Op
    {
        Add, Remove, Unknown
    }
}