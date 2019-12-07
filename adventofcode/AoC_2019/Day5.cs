using System.Collections.Generic;

namespace AdventofCode.AoC_2019
{
    class Day5 : BaseDay
    {
        protected override void Setup()
        {
            Source = InputSource.test;
            Source = InputSource.prod;

            LogLevel = UseTestData ? 5 : 0;

            Part1TestSolution = null;
            Part2TestSolution = null;
            Part1Solution = 5821753;
            Part2Solution = 11956381;
        }

        protected override void DoRun(string[] input)
        {
            var comp = new IntCodeComputer(new List<int> { 1 }, input[0], -1, -1);
            comp.Execute();
            Part1 = comp.LastOutput;

            comp = new IntCodeComputer(new List<int> { 5 }, input[0], -1, -1);
            comp.Execute();
            Part2 = comp.LastOutput;

        }
    }
}