using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace adventofcode
{
    internal class Day21 : BaseDay
    {
        protected override void Setup()
        {
            Source = InputSource.test;
            //   Source = InputSource.prod;

            LogLevel = UseTestData ? 5 : 0;

            Part1TestSolution = null;
            Part2TestSolution = null;
            Part1Solution = 10720163;
            Part2Solution = 5885821;
        }

        private void x()
        {
            long reg4 = 0;
            long reg1;
            long reg3;
            long reg5;

            var terminators = new HashSet<long>();

            long lastTerminator = -1;
            int round = 0;
            while (true)
            {
                // 6=>7
                reg1 = reg4 | 65536;
                reg4 = 16031208;

                while (true)
                {
                    // 8=>12
                    reg3 = reg1 & 255;
                    reg4 += reg3;
                    reg4 = reg4 & 16777215;
                    reg4 = reg4 * 65899;
                    reg4 = reg4 & 16777215;

                    // 13=>17
                    if (reg1 < 256)
                        break;

                    reg3 = 0;

                    // 18=>25
                    while (true)
                    {
                        reg5 = (reg3 + 1) * 256;
                        if (reg5 > reg1)
                            break;
                        reg3++;
                    }

                    reg1 = reg3;
                }

                if (!terminators.Contains(reg4))
                {
                    terminators.Add(reg4);
                //    Log("Terminator #"+terminators.Count+": " + reg4, 2);
                    lastTerminator = reg4;
                    if (Part1 == null)
                    {
                        Part1 = reg4;
                        Log("First terminator: " + lastTerminator, 2);
                    }
                }
                else
                {
                    Log("REPEATED TERMINATOR: "+ reg4, 2);
                    Log("Terminators: " + terminators.Count, 2);
                    Log("Last terminator: "+ lastTerminator, 2);
                    Part2 = lastTerminator;
                    return;
                }

//                round++;
//                if (round > 20000)
//                    return;
            }
        }

        protected override void DoRun(string[] input)
        {
            x();
            return;
            var i = 0;
            do
            {
                var registers = new long[6];
                registers[0] = 17;
                Log("***********Executing with i=" + i);
                var terminated = ExecuteProgram(input, registers);
                if (terminated)
                    Part1 = i;
                i++;
            } while (Part1 == null);
        }

        private bool ExecuteProgram(string[] input, long[] registers)
        {
            long ip = 0;
            var ipreg = GetIntArr(input[0])[0];
            string s = "";
            var gen = 0;
            var terminatingNumbers = new HashSet<long>();
            long lastTerminator = 0;
            while (ip >= 0 && ip < input.Length - 1)
            {
                var row = input[ip + 1];
                var oldres = registers[0];
                var args = GetIntArr(row);
                Enum.TryParse<Ops>(row.Split(' ')[0], out var op);
                registers[ipreg] = ip;
                if (LogLevel >= 4)
                {
                    s = "Gen=" + gen.ToString().PadLeft(8) + " ip=" + ip.ToString().PadLeft(2) + " " + PrintRegisters(registers);
                    s += $"{op} {args[0].ToString().PadLeft(3)} {args[1].ToString().PadLeft(3)} {args[2].ToString().PadLeft(3)} ";
                }

                if (ip == 28)
                {
                    var num = registers[4];
                    if (!terminatingNumbers.Contains(num))
                    {
                        terminatingNumbers.Add(registers[4]);
                        Log("Gen=" + gen + " terminates at " + registers[4].ToString(), 2);
                        lastTerminator = num;
                    }
                    else
                    {
                        Log("REPEAT: Gen=" + gen + " terminates at " + registers[4].ToString() + " Last terminator: " + lastTerminator, 2);
                    }

                    Log("Terminators found: " + terminatingNumbers.Count());
                }
                Execute(op, args[0], args[1], args[2], registers);
                ip = registers[ipreg] + 1;
                if (LogLevel >= 4)
                    s += PrintRegisters(registers);
                if (ip == 7)
                    Log(s, 4);
                gen++;
                //                if (gen > 20000)
                //                    return false;
            }
            Log(s, 2);
            return true;
        }

        private string PrintRegisters(long[] registers)
        {
            var s = "[";
            foreach (var r in registers)
                s += r.ToString().PadLeft(7) + ", ";
            return s.Substring(0, s.Length - 2) + "] ";
        }

        private void Execute(Ops op, int inp1, int inp2, int outp, long[] registers)
        {
            long value1;
            if (op == Ops.seti || op == Ops.gtir || op == Ops.eqir)
                value1 = inp1;
            else
                value1 = registers[inp1];

            long value2;
            switch (op)
            {
                case Ops.addi:
                case Ops.muli:
                case Ops.bani:
                case Ops.bori:
                case Ops.seti:
                case Ops.gtri:
                case Ops.eqri:
                    value2 = inp2;
                    break;
                case Ops.setr:
                    value2 = -1;
                    break;
                default:
                    value2 = registers[inp2];
                    break;
            }

            long res;
            switch (op)
            {
                case Ops.addi:
                case Ops.addr:
                    res = value1 + value2;
                    break;
                case Ops.muli:
                case Ops.mulr:
                    res = value1 * value2;
                    break;
                case Ops.bani:
                case Ops.banr:
                    res = value1 & value2;
                    break;
                case Ops.bori:
                case Ops.borr:
                    res = value1 | value2;
                    break;
                case Ops.seti:
                case Ops.setr:
                    res = value1;
                    break;
                case Ops.gtir:
                case Ops.gtrr:
                case Ops.gtri:
                    res = value1 > value2 ? 1 : 0;
                    break;
                case Ops.eqir:
                case Ops.eqri:
                case Ops.eqrr:
                    res = value1 == value2 ? 1 : 0;
                    break;
                default:
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