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
            var comp = new IntCodeComputer(1);
            comp.RunProgram(input[0], -1, -1);
            Part1 = comp.LastOutput;

            comp = new IntCodeComputer(5);
            comp.RunProgram(input[0], -1, -1);
            Part2 = comp.LastOutput;

        }
    }
}