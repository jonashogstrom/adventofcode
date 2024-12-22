using System;
using System.Diagnostics;
using NUnit.Framework;
using System.Collections.Generic;
using AdventofCode.Utils;
using System.Linq;
using AdventofCode.AoC_2019;
using AdventofCode.AoC_2020;
using AdventofCode.AoC_2021;


// 826703223 is too low
namespace AdventofCode.AoC_2024
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = string;
    using Part2Type = string;

    [TestFixture]
    class Day17 : TestBaseClass2<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase("4,6,3,5,6,3,5,2,1,0", null, "Day17_test.txt")]
        [TestCase("0,3,5,4,3,0", null, "Day17_test2.txt")]
        [TestCase("4,3,7,1,5,3,0,5,4", null, "Day17.txt")]
        public void Test1(Part1Type exp1, Part2Type exp2, string resourceName)
        {
            LogLevel = resourceName.Contains("test") ? 20 : -1;
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type part1, Part2Type part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1 = "";
            Part2Type part2 = "";
            var sw = Stopwatch.StartNew();

            var regA = int.Parse(source[0].Split(' ', StringSplitOptions.RemoveEmptyEntries).Last());
            var regB = int.Parse(source[1].Split(' ', StringSplitOptions.RemoveEmptyEntries).Last());
            var regC = int.Parse(source[2].Split(' ', StringSplitOptions.RemoveEmptyEntries).Last());
            var program = source[4].Trim().Split(' ').Last().Split(',').Select(int.Parse).ToArray();

            var comp = new Computer(regA, regB, regC, program);

            LogAndReset("Parse", sw);

            part1 = comp.ExecuteProgram();

            var comp2 = new SimpleComputer(program);
            // var part1_simple = comp2.ExecuteProgram(regA);
            // Assert.That(part1_simple, Is.EqualTo(part1));

            LogAndReset("*1", sw);

            var low = 0x1000_0000_0000L;
            var high = 0xFFFF_FFFF_FFFFL;
            for (var i = low; i < high; i++)
            {
                var x = i - low;
                var frac = (1.0 * x) / high;
                if (comp2.ExecuteProgram(i))
                {
                    part2 = i.ToString();
                    break;
                }
            }


            // var DigitResults = new Dictionary<int, int>();
            // for (int i = 0; i < 8; i++)
            // {
            //     comp.Reset();
            //     comp.Registers[0] = i;
            //     comp.ExecuteProgram();
            //     DigitResults[i] = comp.Output.First();
            // }
            //

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        internal class SimpleComputer
        {
            //private List<int> _output;
            private int[] _targetOutput;

            public SimpleComputer(int[] program)
            {
                _targetOutput = program;
            }

            public (long a, int output) ExecuteOne(long a)
            {
                var b = (int)(a & 7);
                b = b ^ 2;
                var c = a >> b; // (int)Math.Truncate(a / Math.Pow(2, b));
                var b2 = b ^ c;
                a = a / 8;
                b2 = b2 ^ 7;
                return (a, b & 7);
            }

            public bool ExecuteProgram(long a)
            {
                var i = 0;
                while (a != 0)
                {
                    var res = ExecuteOne(a);
                    if (res.output != _targetOutput[i])
                        return false;
                    i++;
                    a = res.a;
                }

                if (i != _targetOutput.Length)
                    throw new Exception();
                return true;
            }
        }

        internal class Computer
        {
            private List<int> _output = new List<int>();

            public Computer(int regA, int regB, int regC, int[] program)
            {
                Registers = [regA, regB, regC];
                Program = program;
                PC = 0;
                Instructions = new Dictionary<int, Action<int>>();
                // adv: a = a / 2^combo(op)
                Instructions[0] = op => RegA = (int)Math.Truncate(RegA / Math.Pow(2, Combo(op)));
                // bxl: b = b bitxor op
                Instructions[1] = op => RegB ^= op;
                // bst: b = b mod 8
                Instructions[2] = op => RegB = Combo(op) % 8;

                // jmp not Zero a
                Instructions[3] = op =>
                {
                    if (RegA == 0)
                        return;
                    PC = op - 2; // will be increased by 2 later
                };
                // bxc: b = b^c
                Instructions[4] = op => RegB = RegB ^ RegC;
                // out: print combo(op) mod 8
                Instructions[5] = op => _output.Add(Combo(op) % 8);
                // bdv: b = a / 2^combo(op)
                Instructions[6] = op => RegB = (int)Math.Truncate(RegA / Math.Pow(2, Combo(op)));
                // cdv: c = a / 2^combo(op)
                Instructions[7] = op => RegC = (int)Math.Truncate(RegA / Math.Pow(2, Combo(op)));
            }

            int RegA
            {
                get { return Registers[0]; }
                set { Registers[0] = value; }
            }

            int RegB
            {
                get { return Registers[1]; }
                set { Registers[1] = value; }
            }

            int RegC
            {
                get { return Registers[2]; }
                set { Registers[2] = value; }
            }

            private int Combo(int op)
            {
                if (op is >= 0 and <= 3)
                    return op;
                if (op is >= 4 and <= 6)
                    return Registers[op - 4];
                throw new Exception($"Undefined combo op: {op}");
            }

            public Dictionary<int, Action<int>> Instructions { get; }

            public IReadOnlyList<int> Output => _output;

            public void ExecuteOne()
            {
                var operation = Instructions[Program[PC]];
                var operand = Program[PC + 1];
                operation(operand);
                PC += 2;
            }

            public string ExecuteProgram()
            {
                PC = 0;
                while (PC < Program.Length)
                {
                    ExecuteOne();
                }

                return string.Join(",", Output);
            }

            internal class Instruction
            {
            }

            public int PC { get; set; }

            public int[] Program { get; }

            public int[] Registers { get; }

            public void Reset()
            {
                PC = 0;
                Registers[0] = 0;
                Registers[1] = 0;
                Registers[2] = 0;
                _output.Clear();
            }
        }
    }
}