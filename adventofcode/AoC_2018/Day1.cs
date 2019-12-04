using System.Collections.Generic;

namespace AdventofCode.AoC_2018
{
    public class Day1 : BaseDay
    {
        protected override void Setup()
        {
            Part1Solution = 406;
            Part2Solution = 312;
        }

        protected override void DoRun(string[] input)
        {
            // part 1
            var sum = 0;
            foreach (var d in GetIntInput(input))
            {
                sum += d;
            }

            Part1 = sum;

            // part 2
            sum = 0;
            var values = new HashSet<int>();
            var done = false;
            while (!done)
            {
                foreach (var d in GetIntInput(input))
                {
                    sum += d;
                    if (values.Contains(sum))
                    {
                        Part2 = sum;
                        done = true;
                        break;
                    }

                    values.Add(sum);
                }
            }
        }
    }
}