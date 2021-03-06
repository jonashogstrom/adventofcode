﻿using System;
using System.Collections.Generic;

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
            var comp = new IntCodeComputer(new List<long>(0) { 0 }, input[0], 12, 02);
            comp.Execute();
            Part1 = comp.ReadMemory(0, ReadOp.data);


            for (var noun = 0; noun < 100; noun++)
                for (var verb = 0; verb < 100; verb++)
                {
                    comp = new IntCodeComputer(new List<long>(0) { 0 }, input[0], noun, verb);
                    comp.Execute();
                    if (comp.ReadMemory(0, ReadOp.data) == 19690720)
                    {
                        Part2 = 100 * noun + verb;
                        return;
                    }
                }
        }
    }
}