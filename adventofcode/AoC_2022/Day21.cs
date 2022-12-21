using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2022
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    // not -3721298272829

    [TestFixture]
    class Day21 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(152, 301, "Day21_test.txt")]
        [TestCase(110181395003396, null, "Day21.txt")]
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

            var monkeys = new Dictionary<string, Monkey2>();
            foreach (var s in source)
            {
                var parts = s.Split(':').Select(x => x.Trim()).ToArray();
                var m = new Monkey2(parts[0], parts[1]);
                monkeys[m.Name] = m;
            }

            LogAndReset("Parse", sw);
            var rootMonkey = monkeys["root"];
            ResolveTree(rootMonkey, monkeys, "humn");
            part1 = rootMonkey.Calculate();

            // solve part 1 here



            LogAndReset("*1", sw);
            rootMonkey.op = "=";

            var newRoot = TwistTree(rootMonkey);
            part2 = newRoot.GetSide(false).Calculate();
            // solve part 2 here

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private void ResolveTree(Monkey2 monkey, Dictionary<string, Monkey2> monkeys, string nameOfUnknown)
        {
            if (monkey.Name == nameOfUnknown)
                monkey.ContainsUnknown = true;

            if (monkey.op != "")
            {
                monkey.Left = monkeys[monkey.LeftName];
                monkey.Right = monkeys[monkey.RightName];
                ResolveTree(monkey.Left, monkeys, nameOfUnknown);
                ResolveTree(monkey.Right, monkeys, nameOfUnknown);
                if (monkey.Left.ContainsUnknown || monkey.Right.ContainsUnknown)
                    monkey.ContainsUnknown = true;
            }
        }

        private Monkey2 TwistTree(Monkey2 root)
        {
            if (root.IsLeaf)
                return root;

            var unknownSide = root.GetSide(true);
            var knownSide = root.GetSide(false);

            if (unknownSide.IsLeaf)
                return root;

            if (unknownSide.op == "-" && unknownSide.Right.ContainsUnknown)
            {
                root.Left = unknownSide.Left;
                root.op = "-";
                root.Right = knownSide;
                unknownSide.op = "=";
                unknownSide.Left = root;
                root.Update();
            }
            else if (unknownSide.op == "+")
            {
                var knownsideOfRoot = root.GetSide(false);
                root.Left = knownsideOfRoot;
                root.Right = unknownSide.GetSide(false);
                root.op = "-";
                unknownSide.op = "=";
                unknownSide.ReplaceKnownSide(root);
                root.Update();
            }
            else if (unknownSide.op == "*")
            {
                var knownsideOfRoot = root.GetSide(false);
                root.Left = knownsideOfRoot;
                root.Right = unknownSide.GetSide(false);
                root.op = "/";
                unknownSide.op = "=";
                unknownSide.ReplaceKnownSide(root);
                root.Update();
            }
            else if (unknownSide.op == "/" && unknownSide.Right.ContainsUnknown)
            {
                root.Left = unknownSide.Left;
                root.op = "/";
                root.Right = knownSide;
                unknownSide.op = "=";
                unknownSide.Left = root;
                root.Update();
            }
            else
            {
                root.op = Invert(unknownSide.op);
                var KnownSideOfUnknown = unknownSide.GetSide(false);
                root.ReplaceUnknownSide(KnownSideOfUnknown);
                unknownSide.ReplaceKnownSide(root);
                unknownSide.op = "=";
            }

            return TwistTree(unknownSide);
        }

        private string Invert(string op)
        {
            switch (op)
            {
                case "+": return "-";
                case "-": return "+";
                case "*": return "/";
                case "/": return "*";
                default: throw new Exception();
            }
        }
    }

    [DebuggerDisplay("{Name}")]
    internal class Monkey2
    {
        //public string Task { get; }
        public string Name { get; }
        public long? Value { get; set; } = null;

        public bool ContainsUnknown { get; set; }
        public string op { get; set; }
        public Monkey2 Left { get; set; }
        public Monkey2 Right { get; set; }

        public bool IsLeaf { get; set; }

        public Monkey2 GetSide(bool unknown)
        {
            if (Left.ContainsUnknown == unknown) return Left;
            return Right;
        }

        public Monkey2(string name, string task)
        {
            //Task = task;

            Name = name;
            var parts = task.Split(' ');
            if (parts.Length == 1)
            {
                op = "";
                Value = long.Parse(parts[0]);
                IsLeaf = true;
            }
            else
            {
                op = parts[1];
                LeftName = parts[0];
                RightName = parts[2];
                IsLeaf = false;
            }
        }

        public string LeftName { get; set; }

        public string RightName { get; set; }

        public long Calculate()
        {
            switch (op)
            {
                case "": return Value.Value;
                case "+": return Left.Calculate() + Right.Calculate();
                case "-": return Left.Calculate() - Right.Calculate();
                case "*": return Left.Calculate() * Right.Calculate();
                case "/": return Left.Calculate() / Right.Calculate();
                default:
                    throw new NotImplementedException();
            }


        }

        public void ReplaceKnownSide(Monkey2 monkey)
        {
            ReplaceSide(false, monkey);
        }

        public void ReplaceSide(bool unknown, Monkey2 monkey)
        {
            if (Left.ContainsUnknown == unknown)
                Left = monkey;
            else
                Right = monkey;
            Update();
        }
        public void ReplaceUnknownSide(Monkey2 monkey)
        {
            ReplaceSide(true, monkey);
        }

        public void Update()
        {
            ContainsUnknown = Left.ContainsUnknown || Right.ContainsUnknown;
            LeftName = Left.Name;
            RightName = Right.Name;
        }
    }
}