using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventofCode.AoC_2018
{
    internal class Day16 : BaseDay
    {
        protected override void Setup()
        {
            UseTestData = false;
            //            UseTestData = false;
            LogLevel = UseTestData ? 5 : 0;

            Part1TestSolution = null;
            Part2TestSolution = null;
            Part1Solution = 642;
            Part2Solution = 481;
        }

        protected override void DoRun(string[] input)
        {
            var ok = true;
            int row = 0;
            int res = 0;
            var possibilities = new HashSet<Ops>[16];
            for (int x = 0; x < 16; x++)
                possibilities[x] = new HashSet<Ops>(Enum.GetValues(typeof(Ops)).Cast<Ops>());
            while (ok)
            {
                var count = TestOpCode(input[row], input[row + 1], input[row + 2], possibilities);
                if (count >= 3)
                    res++;
                row += 4;
                ok = row < input.Length && input[row].StartsWith("Before");
            }
            Part1 = res;


            var registers = new int[4];
            while (row < input.Length)
            {
                var instr = GetIntArr(input[row]);
                if (instr.Length == 4)
                    Execute(possibilities[instr[0]].Single(), instr[1], instr[2], instr[3], registers);
                row++;
            }

            Part2 = registers[0];

        }

        private int TestOpCode(string beforex, string instrx, string afterx, HashSet<Ops>[] possibilities)
        {
            var before = GetIntArr(beforex);
            var instr = GetIntArr(instrx);
            var after = GetIntArr(afterx);
            var matching = 0;
            foreach (Ops op in Enum.GetValues(typeof(Ops)))
            {
                var registers = new int[4];
                before.CopyTo(registers, 0);
                Execute(op, instr[1], instr[2], instr[3], registers);
                if (registers.SequenceEqual(after))
                    matching++;
                else
                {
                    RemoveOp(op, possibilities, instr[0]);
                }
            }

            return matching;

        }

        private void RemoveOp(Ops op, HashSet<Ops>[] possibilities, int opNum)
        {
            possibilities[opNum].Remove(op);
            var reduced = true;
            while (reduced)
            {
                reduced = false;
                for (int test = 0; test < possibilities.Length; test++)
                    if (possibilities[test].Count == 1)
                    {
                        var uniqueOp = possibilities[test].Single();
                        for (int i = 0; i < possibilities.Length; i++)
                            if (i != test && possibilities[i].Contains(uniqueOp))
                            {
                                possibilities[i].Remove(uniqueOp);
                                reduced = true;
                            }
                    }

            }
        }


        private void Execute(Ops op, int inp1, int inp2, int outp, int[] registers)
        {
            var opName = op.ToString();
            int value1;
            if (op == Ops.seti || op == Ops.gtir || op == Ops.eqir)
                value1 = inp1;
            else
                value1 = registers[inp1];

            int value2;
            if (opName.EndsWith("i"))
                value2 = inp2;
            else
                value2 = registers[inp2];


            int res;
            if (opName.StartsWith("add"))
                res = value1 + value2;
            else if (opName.StartsWith("mul"))
                res = value1 * value2;
            else if (opName.StartsWith("ban"))
                res = value1 & value2;
            else if (opName.StartsWith("bor"))
                res = value1 | value2;
            else if (opName.StartsWith("set"))
                res = value1;
            else if (opName.StartsWith("gt"))
                res = value1 > value2 ? 1 : 0;
            else if (opName.StartsWith("eq"))
                res = value1 == value2 ? 1 : 0;
            else
            {
                throw new Exception();
            }

            registers[outp] = res;
        }

        public enum Ops
        {
            addr, addi, mulr, muli, banr, bani,
            borr, bori,
            setr, seti,
            gtir, gtri, gtrr,
            eqir,
            eqri,
            eqrr
        }
    }
}