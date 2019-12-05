using System;

namespace AdventofCode.AoC_2019
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
            Part2Solution = 9507;
        }

        protected override void DoRun(string[] input)
        {
            var comp = new IntCodeComputer(0);
            comp.RunProgram(input[0], 12, 02);
            Part1 = comp.Memory[0];


            for (var noun = 0; noun < 100; noun++)
                for (var verb = 0; verb < 100; verb++)
                {
                    comp.RunProgram(input[0], noun, verb);
                    if (comp.Memory[0] == 19690720)
                    {
                        Part2 = 100 * noun + verb;
                        return;
                    }
                }
        }
    }
}