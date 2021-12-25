using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Windows.Forms;
using AdventofCode.AoC_2020;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2021
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day24 : TestBaseClass<Part1Type, Part2Type>
    {
        private byte[] _best;
        public bool Debug { get; set; }

        [Test]
        [TestCase(-1, null, "Day24_test.txt")]
        [TestCase(-1, null, "Day24.txt")]
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
            var program = new List<Instr>();
            foreach (var s in source)
                program.Add(ParseInstr(s));

            LogAndReset("Parse", sw);

            FindMonad(program);
            LogAndReset("*1", sw);
            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private void FindMonad(List<Instr> program)
        {
            FindMonad2(program, new List<byte>());
            if (_best != null)
                Log(string.Join("", _best));
        }

        private long c = 0;
        private void FindMonad2(List<Instr> program, List<byte> input)
        {
            c++;
            if (c % 10000 == 0)
                Log(string.Join("", input));
            for (int i = 9; i >= 1; i--)
            {
                var res = ExecuteProgram(program, input.Append((byte)i));
                if (input.Count == 3)
                {
                    var s = string.Join("", input)+i;
                    Log($"{s} => {res.registers['z']}");
                    continue;
                }
                switch (res.res)
                {

                    case ExecRes.OK:
                        if (res.registers['z'] == 0)
                        {
                            _best = input.Append((byte)i).ToArray();
                            return;
                        }
                        else
                        {
                            //                            Log($"{res.registers['x']:0000000} {res.registers['y']:0000000} {res.registers['z']:000000000000}");
                        }
                        break;
                    case ExecRes.EndOfInput:
                        FindMonad2(program, input.Append((byte)i).ToList());
                        if (_best != null)
                            return;
                        break;
                    case ExecRes.DivByZero:
                        break;
                }
            }
        }

        private (ExecRes res, DicWithDefault<char, long> registers) ExecuteProgram(List<Instr> program, IEnumerable<byte> input)
        {
            var registers = new DicWithDefault<char, long>();
            var inputq = new Queue<byte>(input);
            foreach (var i in program)
            {
                var res = i.Execute(registers, inputq);
                if (res != ExecRes.OK)
                    return (res, registers);
            }
            // foreach (var r in registers.Keys)
            //     Log($"{r} = {registers[r]}");
            return (ExecRes.OK, registers);
        }


        private Instr ParseInstr(string s)
        {
            var parts = s.Split(' ');
            return new Instr(parts[0], parts[1], parts.Length == 3 ? parts[2] : null);

        }
    }

    internal enum ExecRes
    {
        OK,
        DivByZero,
        EndOfInput
    }

    internal class Instr
    {
        private readonly bool _arg2IsVal;
        private long _arg2Val;
        public string Op { get; }
        public char Arg1Reg { get; }
        public char Arg2Reg { get; }

        public Instr(string op, string arg1, string arg2)
        {
            Op = op;
            Arg1Reg = arg1[0];
            _arg2IsVal = long.TryParse(arg2, out _arg2Val);
            if (!_arg2IsVal && arg2 != null)
                Arg2Reg = arg2[0];
        }

        public ExecRes Execute(DicWithDefault<char, long> registers, Queue<byte> inp)
        {
            long res;
            long n;
            switch (Op)
            {
                case "inp":
                    if (!inp.Any())
                        return ExecRes.EndOfInput;
                    registers[Arg1Reg] = inp.Dequeue();
                    break;
                case "add":
                    res = registers[Arg1Reg] + GetVal2(registers);
                    registers[Arg1Reg] = res;
                    break;
                case "mul":
                    res = registers[Arg1Reg] * GetVal2(registers);
                    registers[Arg1Reg] = res;
                    break;
                case "div":
                    n = GetVal2(registers);
                    if (n == 0)
                        return ExecRes.DivByZero;
                    res = registers[Arg1Reg] / n;
                    registers[Arg1Reg] = res;
                    break;
                case "mod":
                    n = GetVal2(registers);
                    if (n == 0)
                        return ExecRes.DivByZero;
                    res = registers[Arg1Reg] % n;
                    registers[Arg1Reg] = res;
                    break;
                case "eql":
                    registers[Arg1Reg] = registers[Arg1Reg] == GetVal2(registers) ? 1 : 0;
                    break;
                default:
                    throw new Exception("unknown instr");
            }

            return ExecRes.OK;
        }

        private long GetVal2(DicWithDefault<char, long> registers)
        {
            return _arg2IsVal ? _arg2Val : registers[Arg2Reg];
        }
    }
}