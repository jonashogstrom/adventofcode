using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace adventofcode
{
    internal class Day19 : BaseDay
    {
        protected override void Setup()
        {
            Source = InputSource.test;
            Source = InputSource.prod;

            LogLevel = UseTestData ? 5 : 1;

            Part1TestSolution = 6;
            Part2TestSolution = null;
            Part1Solution = 1836;
            Part2Solution = 18992556;

        }

        protected override void DoRun(string[] input)
        {

            var registers = new int[6];
            Part1 = ExecuteProgram(input, registers);

            registers = new int[6];
            registers[0] = 1;
            Part2 = ExecuteProgram(input, registers);
        }

        private int ExecuteProgram(string[] input, int[] registers)
        {
            var ip = 0;
            var ipreg = GetIntArr(input[0])[0];
            string s = "";
            var gen = 0;
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
                    s += $"{op} {args[0]} {args[1]} {args[2]} ";
                }

                Execute(op, args[0], args[1], args[2], registers);
                ip = registers[ipreg] + 1;
                if (LogLevel >= 4)
                    s += PrintRegisters(registers);
                if (registers[0] != oldres)
                {
                    Log($"Gen={gen}: {op} {args[0]} {args[1]} {args[2]} => {PrintRegisters(registers)}");
                    var factors = FindPrimeFactors(registers[1]).ToList();
                    if (factors.Count == 3)
                    {
                        // shortcut the evaluation, take the prime factors of the value in reg1, multiply them in all combinations.
                        // only written for 3 factors...
                        var res = 1 + registers[1] +
                                  factors[0] + factors[1] + factors[2] +
                                  factors[0] * factors[1] +
                                  factors[1] * factors[2] +
                                  factors[0] * factors[2];
                        return res;
                    }
                }
                Log(s, 4);
                gen++;
            }
            Log(s, 2);
            return registers[0];
        }

        private IEnumerable<int> FindPrimeFactors(int value)
        {
            for (int i=2; i<Math.Sqrt(value); i++)
                if (value % i == 0)
                {
                    yield return i;
                    foreach (var x in FindPrimeFactors(value / i))
                        yield return x;
                    yield break;
                }
            yield return value;
        }

        private string PrintRegisters(int[] registers)
        {
            var s = "[";
            foreach (var r in registers)
                s += r.ToString().PadLeft(5) + ", ";
            return s.Substring(0, s.Length - 2) + "] ";
        }

        private void Execute(Ops op, int inp1, int inp2, int outp, int[] registers)
        {
            int value1;
            if (op == Ops.seti || op == Ops.gtir || op == Ops.eqir)
                value1 = inp1;
            else
                value1 = registers[inp1];

            int value2;
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

            int res;
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