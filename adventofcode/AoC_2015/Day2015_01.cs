namespace adventofcode.AoC_2015
{
    internal class Day1 : BaseDay
    {
        protected override void Setup()
        {
            //      Source = InputSource.test;
            Source = InputSource.prod;

            LogLevel = UseTestData ? 5 : 0;

            Part1TestSolution = null;
            Part2TestSolution = null;
            Part1Solution = null;
            Part2Solution = null;
        }

        protected override void DoRun(string[] input)
        {
            var level = 0;
            for (var i = 0; i < input[0].Length; i++)
            {
                var x = input[0][i];
                if (x == '(')
                    level++;
                else if (x == ')')
                    level--;
                if (level == -1 && Part2 == null)
                    Part2 = i + 1;

            }

            Part1 = level;
            Part1 = level;
        }
    }
}