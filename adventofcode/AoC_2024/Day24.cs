using System;
using System.Diagnostics;
using NUnit.Framework;
using System.Collections.Generic;
using AdventofCode.Utils;
using System.Linq;
using AdventofCode.AoC_2021;


namespace AdventofCode.AoC_2024
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day24 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(4, null, "Day24_test.txt")]
        [TestCase(2024, null, "Day24_test2.txt")]
        [TestCase(9999999, null, "Day24.txt")]
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
            var board = new Dictionary<string, Component>();
            var g = source.AsGroups();
            foreach (var wireInfo in g.First())
            {
                var parts = wireInfo.Split(": ");
                var wireName = parts[0];
                var wirevalue = int.Parse(parts[1]);
                board[wireName] = new Wire(wirevalue);
            }

            foreach (var gateInfo in g.Last())
            {
                // x00 AND y00 -> z00
                var parts = gateInfo.Split(' ');
                var gateName = parts[4];
                var wire1 = parts[0];
                var wire2 = parts[2];
                var op = parts[1];
                board[gateName] = new Gate(gateName, wire1, wire2, op);
            }

            LogAndReset("Parse", sw);

            foreach (var zGate in board.Keys.Where(k => k.StartsWith("z")).OrderByDescending(k => k))
            {
                part1 = (part1 << 1) + board[zGate].GetValue(board);
            }
            // solve part 1 here

            LogAndReset("*1", sw);

            // solve part 2 here

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        internal abstract class Component
        {
            public abstract int GetValue(Dictionary<string, Component> board);
        }

        internal class Gate(string name, string wire1, string wire2, string op) : Component
        {
            private int? _value;
            public string Wire1 => wire1;
            public string Wire2 => wire2;
            public string Op => op;

            public override int GetValue(Dictionary<string, Component> board)
            {
                if (_value.HasValue)
                    return _value.Value;
                var v1 = board[Wire1].GetValue(board);
                var v2 = board[Wire2].GetValue(board);
                switch (Op)
                {
                    case "AND": _value = v1 & v2; break;
                    case "OR": _value = v1 | v2; break;
                    case "XOR": _value = v1 ^ v2; break;
                    default: throw new Exception();
                }
                Console.WriteLine($"Calculated {name} as: {Wire1} {Op} {Wire2} = {_value}");
                return _value.Value;
            }
        }

        internal class Wire : Component
        {
            public Wire(int value)
            {
                Value = value;
            }

            public int Value { get; }
            public override int GetValue(Dictionary<string, Component> board) => Value;
        }
    }
}