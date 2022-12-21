using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2022
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day13 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(13, 140, "Day13_test.txt")]
        [TestCase(6070, null, "Day13.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            LogLevel = resourceName.Contains("test") ? 20 : -1;
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        [Test]
        [TestCase("[[4,4],4,4]", "[[4,4],4,4,4]", true)]
        public void Test2(string p1, string p2, bool exp)
        {
            var packet1 = ParsePacketList(p1, 0);
            var packet2 = ParsePacketList(p2, 0);
            Assert.AreEqual(ComparePackets2(packet1, packet2), exp);
        }
        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1 = 0;
            Part2Type part2 = 0;
            var sw = Stopwatch.StartNew();

            // parse input here
            var packetPairs = source.AsGroups().ToList();
            LogAndReset("Parse", sw);

            var sum = 0;
            var correctPackages = new List<int>();
            for (int i = 0; i < packetPairs.Count(); i++)
            {
                Log($"== Pair {i + 1} ==");
                var packetPair = packetPairs[i];
                if (ComparePackets(packetPair))
                {
                    sum += i + 1;
                    correctPackages.Add(i + 1);
                }
            }

            Log("Correctly ordered package pairs: " + string.Join(", ", correctPackages), -1);

            part1 = sum;
            LogAndReset("*1", sw);
            // Solution for part2
            var allPackets = new List<Packet>();
            foreach (var s in source)
                if (!string.IsNullOrEmpty(s))
                    allPackets.Add(ParsePacketList(s, 0));
            allPackets.Add(ParsePacketList("[[2]]", 0, true));
            allPackets.Add(ParsePacketList("[[6]]", 0, true));
            allPackets.Sort((packet1, packet2) => ComparePackets2(packet1, packet2).Value ? -1 : 1);
            var res = 1;
            for (int i = 0; i < allPackets.Count; i++)
                if (allPackets[i].IsDividerPackage)
                    res *= i + 1;
            part2 = res;
            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private bool ComparePackets(IList<string> packetPair)
        {
            Log($"Compare {packetPair[0]} vs {packetPair[1]}");

            var packet1 = ParsePacketList(packetPair[0], 0);
            var packet2 = ParsePacketList(packetPair[1], 0);
            var res = ComparePackets2(packet1, packet2);
            if (!res.HasValue)
                throw new Exception("packets identical???");
            Log("Result: " + (res.Value ? "Ordered" : "NOT ORDERED"));
            return res.Value;
        }

        // NOT 5309

        private bool? ComparePackets2(Packet packet1, Packet packet2)
        {
            var p = 0;
            while (p < packet1.Values.Count && p < packet2.Values.Count)
            {
                var v1 = packet1.Values[p];
                var v2 = packet2.Values[p];
                if (v1 is IntValue int1 && v2 is IntValue int2)
                {
                    if (int1.i < int2.i)
                        return true;
                    if (int1.i > int2.i)
                        return false;
                }
                else
                {
                    var list1 = EnsureList(v1);
                    var list2 = EnsureList(v2);
                    var res = ComparePackets2(list1, list2);
                    if (res.HasValue)
                        return res.Value;
                }

                p++;

            }

            if (packet1.Values.Count == packet2.Values.Count)
                return null;

            return packet1.Values.Count < packet2.Values.Count;
        }

        private Packet EnsureList(PacketValue v2)
        {
            if (v2 is Packet p2)
                return p2;
            var res = new Packet();
            res.AddValue(v2 as IntValue);
            return res;

        }

        private Packet ParsePacketList(string packetString, int pos, bool isDividerPackage = false)
        {
            Assert.AreEqual(packetString[pos], '[');
            pos++;
            var res = new Packet(isDividerPackage);
            while (true)
            {
                if (packetString[pos] == '[')
                {
                    var parsedPackage = ParsePacketList(packetString, pos);
                    pos = parsedPackage.End;
                    if (packetString[pos] == ',')
                        pos++;
                    res.AddValue(parsedPackage);
                }
                else if (packetString[pos] == ']')
                {
                    pos++;
                    break;
                }
                else // int
                {

                    var s = "";
                    while (char.IsDigit(packetString[pos]))
                    {
                        s += packetString[pos];
                        pos++;
                    }

                    var intValue = int.Parse(s);
                    res.AddValue(new IntValue(intValue));
                    if (packetString[pos] == ',')
                        pos++;
                }
            }

            res.End = pos;
            return res;

        }
    }

    [DebuggerDisplay("{i}")]
    internal class IntValue : PacketValue
    {
        public int i { get; }

        public IntValue(int intValue)
        {
            i = intValue;
        }

        public override string ToString()
        {
            return $"{i}";
        }
    }

    internal class PacketValue
    {

    }

    internal class Packet : PacketValue
    {
        public bool IsDividerPackage { get; }

        public Packet(bool isDividerPackage = false)
        {
            IsDividerPackage = isDividerPackage;
        }

        public void AddValue(PacketValue value)
        {
            Values.Add(value);
        }

        public List<PacketValue> Values { get; } = new List<PacketValue>();
        public int End { get; set; }

        public override string ToString()
        {
            return "[" + string.Join(",", Values) + "]";
        }
    }
}