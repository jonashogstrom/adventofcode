using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Windows.Forms;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2021
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day24 : TestBaseClass<Part1Type, Part2Type>
    {
        private List<string> _validSolutions = new List<string>();
        public bool Debug { get; set; }

        [Test]
        [TestCase(29989297949519, 19518121316118, "Day24.txt")]
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
            _programBlocks = new List<List<Instr>>();
            var blockCounter = -1;
            foreach (var s in source)
            {
                var instr = ParseInstr(s);
                if (instr.Op == "inp")
                {
                    blockCounter++;
                    _programBlocks.Add(new List<Instr>());
                }
                _programBlocks[blockCounter].Add(instr);
                program.Add(instr);
            }

            LogAndReset("Parse", sw);

            var res = FindMonads(program);
            part1 = res.highest;
            part2 = res.lowest;

            LogAndReset("*1", sw);
            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private (long lowest, long highest)FindMonads(List<Instr> program)
        {
            var initStates = new Dictionary<long, StateData>();
            initStates[0] = new StateData { counter = 1, HighestMonad = 0, LowestMonad = 0};
            var res = FindMonad2(program, 0, initStates, true);
            if (res.TryGetValue(0, out var data2))
            {
                Log($"valid monads: {data2.counter}");
                Log($"highest: {data2.HighestMonad}");
                Log($"lowest: {data2.LowestMonad}");
                return (data2.LowestMonad, data2.HighestMonad);
            }

            return(-1,-1);
        }

        private long c = 0;

        private Dictionary<long, StateData> FindMonad2(List<Instr> program, int ptr, Dictionary<long, StateData> initStates, bool abortOnFind)
        {
            var states = initStates;
            for (int blockCounter = 0; blockCounter < 14; blockCounter++)
            {
                var newStates = new Dictionary<long, StateData>();
                foreach (var z in states.Keys)
                {
                    // var registers = new DicWithDefault<char, BigInteger>(0);
                    // registers['z'] = initZ;

                    for (byte i = 1; i <= 9; i++)
                    {
                        var newZ = ExecuteBlock(z, i, _blocks[blockCounter]);
                        // var registers = new DicWithDefault<char, BigInteger>();
                        // registers['z'] = z;
                        // var res = ExecuteProgram(_programBlocks[blockCounter], registers, 0, i);
                        //
                        // if (res.registers['z'] != newZ)
                        //     throw new Exception();

                        if (!newStates.TryGetValue(newZ, out var data))
                        {
                            data = new StateData();
                            data.HighestMonad = states[z].HighestMonad*10 + i;
                            data.LowestMonad = data.HighestMonad;
                            newStates[newZ] = data;
                        }
                        else
                        {
                            var x = states[z].HighestMonad*10 + i;
                            if (x > data.HighestMonad)
                                data.HighestMonad = x;

                            var y = states[z].LowestMonad* 10 + i;
                            if (y < data.LowestMonad)
                                data.LowestMonad = y;
                        }
                        data.counter += states[z].counter;
                    }
                }

                states = newStates;
                Log($"Block {blockCounter}: {states.Count} states");
            }

            return states;
        }

        private (ExecRes res, DicWithDefault<char, BigInteger> registers, int ptr) ExecuteProgram(
            List<Instr> program, DicWithDefault<char, BigInteger> registers, int ptr, byte nextInput)
        {
            var inputq = new Queue<byte>();
            inputq.Enqueue(nextInput);

            while (ptr < program.Count)
            {
                var res = program[ptr].Execute(registers, inputq);
                if (res != ExecRes.OK)
                    return (res, registers, ptr);
                ptr++;
            }
            // foreach (var r in registers.Keys)
            //     Log($"{r} = {registers[r]}");
            return (ExecRes.OK, registers, ptr);
        }

        /*
         sample code, block 4
inp w
mul x 0
add x z
mod x 26  => x=z%26
div z 1
add x 12
eql x w
eql x 0
mul y 0
add y 25
mul y x
add y 1
mul z y

mul y 0
add y w
add y 13
mul y x
add z y
          */

        // block 4: a = 12, b = 13 div=false
        private class BlockDef
        {
            public BlockDef(int a, int b, bool div = false)
            {
                this.a = a;
                this.b = b;
                this.div = div;
            }

            public int a;
            public int b;
            public bool div = false;
        }

        private List<BlockDef> _blocks = new List<BlockDef>
        {
            new BlockDef(14, 14),
            new BlockDef(14, 2),
            new BlockDef(14, 1),
            new BlockDef(12, 13),
            new BlockDef(15, 5),
            new BlockDef(-12, 5, true), // 6
            new BlockDef(-12, 5, true),
            new BlockDef(12, 9), // 8
            new BlockDef(-7, 3, true),
            new BlockDef(13, 13), // 10
            new BlockDef(-8, 2, true), // 11
            new BlockDef(-5, 1, true), // 12
            new BlockDef(-10, 11, true), // 13
            new BlockDef(-7, 8, true), // 14
        };

        private List<List<Instr>> _programBlocks;


        /* sample block 6 (first with div 26)
inp w //6
mul x 0
add x z
mod x 26
>> div z 26
add x -12
eql x w
eql x 0
mul y 0
add y 25
mul y x
add y 1
mul z y
mul y 0
add y w
add y 5
mul y x
add z y       
         */
        private long ExecuteBlock(long z, byte w, BlockDef blockDef)
        {
            if (z % 26 + blockDef.a == w)
            {
                if (blockDef.div)
                    z = z / 26;
            }
            else
            {
                if (blockDef.div)
                    z = z / 26;
                z = z * 26 + (w + blockDef.b);
            }

            return z;
        }


        private Instr ParseInstr(string s)
        {
            var parts = s.Split(' ');
            return new Instr(parts[0], parts[1], parts.Length == 3 ? parts[2] : null);

        }
    }

    internal class StateData
    {
        public long counter;
        public long HighestMonad;
        public long LowestMonad;
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

        public ExecRes Execute(DicWithDefault<char, BigInteger> registers, Queue<byte> inp)
        {
            BigInteger res;
            BigInteger n;
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
                    var v1 = registers[Arg1Reg];
                    var v2 = GetVal2(registers);
                    // var temp = v1.ToString();
                    // var temp2 = v2.ToString();
                    res = v1 * v2;
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

        private BigInteger GetVal2(DicWithDefault<char, BigInteger> registers)
        {
            return _arg2IsVal ? _arg2Val : registers[Arg2Reg];
        }
    }
}