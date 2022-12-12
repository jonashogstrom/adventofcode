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
    class Day11 : TestBaseClass2<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(10605, 2713310158, "Day11_test.txt")]
        [TestCase(78960, 14561971968, "Day11.txt")]
        public void Test1(Part1Type exp1, Part2Type exp2, string resourceName)
        {
            LogLevel = resourceName.Contains("test") ? 20 : -1;
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type part1, Part2Type part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1 = 0;
            Part2Type part2 = 0;
            var sw = Stopwatch.StartNew();
            var monkeys = ParseInput(source);
            LogAndReset("Parse", sw);

            ExecuteRounds(monkeys, 20, true);
            part1 = CalculateActivity(monkeys);
            LogAndReset("*1", sw);

            monkeys = ParseInput(source);
            ExecuteRounds(monkeys, 10000, false);
            part2 = CalculateActivity(monkeys);
            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private static long CalculateActivity(List<Monkey> monkeys)
        {
            long part1;
            var activity = monkeys.Select(m => m.Inspections).OrderByDescending(v => v).ToArray();

            part1 = activity[0] * activity[1];
            return part1;
        }

        private void ExecuteRounds(List<Monkey> monkeys, int rounds, bool reduceStress)
        {
            for (int i = 0; i < rounds; i++)
            {
                Log($"Round {i + 1}");
                foreach (var m in monkeys)
                {
                    m.ExecuteTurn(monkeys, reduceStress);
                }

                foreach (var m in monkeys)
                {
                    Log(()=>$"{string.Join(", ", m.Items)}");
                }
            }
        }

        private static List<Monkey> ParseInput(string[] source)
        {
            var monkeys = new List<Monkey>();
            var factors = new HashSet<int>();
            foreach (var desc in source.AsGroups())
            {
                var m = new Monkey();
                monkeys.Add(m);
                var items = desc[1].Split(':', ',').Skip(1).Select(x => long.Parse(x.Trim())).ToArray();
                foreach (var x in items)
                    m.Items.Enqueue(x);
                var opParts = desc[2].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var op = opParts[5].Trim();
                if (op == "old")
                {
                    if (opParts[4] == "*")
                        m.Operation = x => x * x;
                    if (opParts[4] == "+")
                        m.Operation = x => x + x;
                }
                else
                {
                    var value = int.Parse(opParts[5].Trim());
                    if (opParts[4] == "*")
                        m.Operation = x => x * value;
                    if (opParts[4] == "+")
                        m.Operation = x => x + value;
                }

                m.OperationStr = desc[2];

                m.TestDiv = int.Parse(desc[3].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[3]);
                m.TrueMonkey = int.Parse(desc[4].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[5]);
                m.FalseMonkey = int.Parse(desc[5].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[5]);
                factors.Add(m.TestDiv);
            }

            var magicReducer = 1;
            foreach (var m in factors)
                magicReducer *= m;
            foreach (var m in monkeys)
                m.MagicReducer = magicReducer;

            return monkeys;
        }
    }

    internal class Monkey
    {
        public int TrueMonkey { get; set; }
        public Queue<long> Items { get; } = new Queue<long>();
        public Func<long, long> Operation { get; set; }
        public int TestDiv { get; set; }
        public int FalseMonkey { get; set; }

        public void ExecuteTurn(List<Monkey> monkeys, bool reduceStress)
        {
            while (Items.Any())
            {
                Inspections++;
                var item = Items.Dequeue();

                item = Operation(item);
                if (reduceStress)
                    item = item / 3;

                item = item % MagicReducer;

                if (item % TestDiv == 0)
                    monkeys[TrueMonkey].Items.Enqueue(item);
                else
                    monkeys[FalseMonkey].Items.Enqueue(item);
            }
        }

        public long Inspections { get; set; }
        public string OperationStr { get; set; }
        public int MagicReducer { get; set; }
    }
}