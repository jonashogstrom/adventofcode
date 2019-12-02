using System;
using System.Linq;
using System.Net.Mail;
using System.Windows.Forms;

namespace adventofcode.AoC_2019
{
    class Day2 : BaseDay
    {
        protected override void Setup()
        {
            Source = InputSource.test;
            Source = InputSource.prod;

            LogLevel = UseTestData ? 5 : 0;

            Part1TestSolution = 30;
            Part2TestSolution = null;
            Part1Solution = 2692315;
            Part2Solution = null;
        }

        protected override void DoRun(string[] input)
        {

            Part1 = runprogram(input[0], 12, 02);
            

            for (var noun=0; noun<100; noun++)
                for (var verb= 0; verb< 100; verb++)
                    if (runprogram(input[0], noun, verb) == 19690720)
                    {
                        Part2 = 100 * noun + verb;
                        return;
                    }

        }

        private int runprogram(string program, int noun, int verb)
        {
            var mem = GetInts(program).ToArray();
            var pointer = 0;

            if (Source == InputSource.prod)
            {
                mem[1] = noun;
                mem[2] = verb;
            }

            while (pointer < mem.Length)
            {
                var op = mem[pointer];
                if (op == 99)
                {
                    return mem[0];
                }

                var arg1 = mem[pointer + 1];
                var arg2 = mem[pointer + 2];
                var res = mem[pointer + 3];

                switch (op)
                {
                    case 1:
                        mem[res] = mem[arg1] + mem[arg2];
                        break;
                    case 2:
                        mem[res] = mem[arg1] * mem[arg2];
                        break;
                }

                pointer += 4;
            }

            throw new Exception("program didn't terminate...");
        }
    }
}