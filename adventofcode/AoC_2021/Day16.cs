using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2021
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day16 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(-1, null, "Day16_test.txt")]
        [TestCase(934, 912901337844, "Day16.txt")]
        [TestCase(6, null, "D2FE28")]
        [TestCase(-1, null, "38006F45291200")]
        [TestCase(-1, null, "EE00D40C823060")]
        [TestCase(16, null, "8A004A801A8002F478")]
        [TestCase(12, null, "620080001611562C8802118E34")]
        [TestCase(23, null, "C0015000016115A2E0802F182340")]
        [TestCase(31, null, "A0016C880162017C3686B18A3D4780")]

        [TestCase(null, 3, "C200B40A82")]
        [TestCase(null, 54, "04005AC33890")]
        [TestCase(null, 7, "880086C3E88112")]
        [TestCase(null, 9, "CE00C43D881120")]
        [TestCase(null, 1, "D8005AC2A8F0")]
        [TestCase(null, 0, "F600BC2D8F")]
        [TestCase(null, 0, "9C005AC2F8F0")]
        [TestCase(null, 1, "9C0141080250320F1802104A08")]

        /*
    C200B40A82 finds the sum of 1 and 2, resulting in the value 3.
04005AC33890 finds the product of 6 and 9, resulting in the value 54.
880086C3E88112 finds the minimum of 7, 8, and 9, resulting in the value 7.
CE00C43D881120 finds the maximum of 7, 8, and 9, resulting in the value 9.
D8005AC2A8F0 produces 1, because 5 is less than 15.
F600BC2D8F produces 0, because 5 is not greater than 15.
9C005AC2F8F0 produces 0, because 5 is not equal to 15.
9C0141080250320F1802104A08 produces 1, because 1 + 3 = 2 * 2.
         */

        [TestCase(-1, null, "Day16.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            //LogLevel = resourceName.Contains("test") ? 20 : -1;
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1 = 0;
            Part2Type part2 = 0;
            var sw = Stopwatch.StartNew();
            var bin = HexToBin(source.First());
            Log(source.First());
            Log(bin);
            var data = new PacketData(bin, 0);
            LogAndReset("Parse", sw);
            var packets = ParsePackets(data).ToList();
            part1 = packets.Select(p => p.VersionSum()).Sum();

            LogAndReset("*1", sw);
            
            part2 = packets.Select(p => p.ComputedValue()).Sum();
            var sb = new StringBuilder();
            packets.First().Debug(sb);
            Log(sb.ToString());

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private IEnumerable<Packet> ParsePackets(PacketData data)
        {
            while (data.CanProvideOneMorePacket)
            {
                yield return ParseSinglePacket(data);
            }

        }

        private Packet ParseSinglePacket(PacketData data)
        {
            var posAtStart = data.Pos;
            var charsLeft = data.Bin.Length - posAtStart;
            var versionS = data.Consume(3);
            var version = versionS.ParseBin();

            var pktTypeS = data.Consume(3);
            var pktType = pktTypeS.ParseBin();

            if (pktType == 4)
            {
                // value packet
                var x = 0;
                var temp =  data.Consume(5);
                var valueStr = temp.Substring(1, 4);

                while (temp[0] == '1')
                {
                    temp = data.Consume(5);
                    valueStr += temp.Substring(1, 4);
                }

                var value = valueStr.ParseBin();
                return new Packet(version, pktType, value, posAtStart, data.Pos);
            }

            // operator packet
            var opType = data.Consume(1);
            if (opType == "0")
            {
                var subPacketLength = data.Consume(15).ParseBin();
                var pktData = data.Consume(subPacketLength);
                var subPktData = new PacketData(pktData, 0);
                var subPackets = ParsePackets(subPktData).ToList();
                return new Packet(version, pktType, subPackets, posAtStart, data.Pos);
            }
            else
            {
                var pktCount = data.Consume(11).ParseBin();
                var subPackets = new List<Packet>();
                for (int i = 0; i < pktCount; i++)
                {
                    subPackets.Add(ParseSinglePacket(data));
                }

                return new Packet(version, pktType, subPackets, posAtStart, data.Pos);
            }
        }

        public string HexToBin(string hex)
        {
            var res = new StringBuilder();
            foreach (var c in hex)
            {
                var value = int.Parse(c.ToString(), System.Globalization.NumberStyles.HexNumber);
                res.Append(Convert.ToString(value, 2).PadLeft(4, '0'));
            }

            return res.ToString();
        }
    }

    internal class PacketData
    {
        public string Bin { get; }
        public long Pos { get; private set; }
        public bool CanProvideOneMorePacket => Bin.Length - Pos >= 11;

        public PacketData(string bin, long pos)
        {
            Bin = bin;
            Pos = pos;
        }

        public string Consume(long l)
        {
            var res = Bin.Substring((int)Pos, (int)l);
            Pos += l;
            return res;
        }
    }

    internal class Packet
    {
        public long Version { get; }
        public long PktType { get; }
        public List<Packet> SubPackets { get; }
        public long Value { get; }
        public long Start { get; }
        public long End { get; }

        public Packet(long version, long pktType, long value, long start, long end)
        {
            Version = version;
            PktType = pktType;
            Value = value;
            Start = start;
            End = end;
            SubPackets = new List<Packet>();
        }

        public Packet(long version, long pktType, List<Packet> subPackets, long start, long end)
        {
            Version = version;
            PktType = pktType;
            SubPackets = subPackets;
            Start = start;
            End = end;
        }

        public long VersionSum()
        {
            return Version + SubPackets.Select(p => p.VersionSum()).Sum();
        }

        public void Debug(StringBuilder sb, int level = 0)
        {
            var s = "".PadLeft(level * 2);
            s += $"Ver={Version}";
            s += $" Type={PktType} (";
            switch (PktType)
            {
                case 0: s += "sum"; break;
                case 1: s += "mul"; break; 
                case 2: s += "min"; break; 
                case 3: s += "max"; break; 
                case 4: s += "val"; break; 
                case 5: s += "gt"; break; 
                case 6: s += "lt"; break; 
                case 7: s += "eq"; break; 
            }
            s += $") Value={Value}";

            sb.AppendLine(s);
            foreach (var c in SubPackets)
                c.Debug(sb, level + 1);
        }

        public long ComputedValue()
        {
            /*
Packets with type ID 0 are sum packets - their value is the sum of the values of their sub-packets. 
            If they only have a single sub-packet, their value is the value of the sub-packet.
Packets with type ID 1 are product packets - their value is the result of multiplying together the values of their sub-packets. If they only have a single sub-packet, their value is the value of the sub-packet.
Packets with type ID 2 are minimum packets - their value is the minimum of the values of their sub-packets.
Packets with type ID 3 are maximum packets - their value is the maximum of the values of their sub-packets.
Packets with type ID 5 are greater than packets - their value is 1 if the value of the first sub-packet 
            is greater than the value of the second sub-packet; otherwise, their value is 0. 
            These packets always have exactly two sub-packets.
Packets with type ID 6 are less than packets - their value is 1 if the value of the first sub-packet is less than the value of the second sub-packet; otherwise, their value is 0. These packets always have exactly two sub-packets.
Packets with type ID 7 are equal to packets - their value is 1 if the value of the first sub-packet is equal to the value of the second sub-packet; otherwise, their value is 0. These packets always have exactly two sub-packets.
             */
            if (PktType == 4)
                return Value;
            var subValues = SubPackets.Select(p => p.ComputedValue()).ToList();
            switch (PktType)
            {
                case 0: return subValues.Sum();
                case 1: return subValues.Multiply();
                case 2: return subValues.Min();
                case 3: return subValues.Max();
                case 5: return subValues[0] > subValues[1] ? 1 : 0;
                case 6: return subValues[0] < subValues[1] ? 1 : 0;
                case 7: return subValues[0] == subValues[1] ? 1 : 0;
            }
            throw new Exception("unknown pkt type");
        }
    }
}