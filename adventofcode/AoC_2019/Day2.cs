using System;

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
            Part2Solution = 9507;
        }

        protected override void DoRun(string[] input)
        {
            var comp = new IntCodeComputer();
            Part1 = comp.runprogram(input[0], 12, 02);
         
            for (var noun = 0; noun < 100; noun++)
            for (var verb = 0; verb < 100; verb++)
            {
                var res = comp.runprogram(input[0], noun, verb);
                if (res == 19690720)
                {
                    Part2 = 100 * noun + verb;
                    return;
                }
            }

        }


    }
}