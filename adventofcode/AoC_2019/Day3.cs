﻿namespace adventofcode.AoC_2019
{
    class Day3 : BaseDay
    {
        protected override void Setup()
        {
            Source = InputSource.test;
            //Source = InputSource.prod;

            LogLevel = UseTestData ? 5 : 0;

            Part1TestSolution = null;
            Part2TestSolution = null;
            Part1Solution = null;
            Part2Solution = null;
        }

        protected override void DoRun(string[] input)
        {
            /*
            var board = EmptyArr<int>(3, 3);
            foreach (var i in GetIntInput(input))
            {
            }
            */

            var comp = new IntCodeComputer();
            Part1 = comp.RunProgram(input[0], 12, 02);

        }
    }
}