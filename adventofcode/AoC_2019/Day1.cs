using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace adventofcode.AoC_2019
{
    class Day1: BaseDay
    {
        protected override void Setup()
        {
            Source = InputSource.test;
            //Source = InputSource.prod;

            LogLevel = UseTestData ? 5 : 0;

            Part1TestSolution = 33583;
            Part2TestSolution = 50346;
            Part1Solution = 3327415;
            Part2Solution = 4988257;
        }

        protected override void DoRun(string[] input)
        {
            var sum2 = 0;
            var sum1 = 0;
            foreach (var i in GetIntInput(input))
            {
                sum1 += i / 3 - 2;
                sum2 += RecAdd(i);
            }

            Part1 = sum1;
            Part2 = sum2;
        }

        private int RecAdd(int i)
        {
            var p= i / 3 - 2;
            if (p > 0)
                return p + RecAdd(p);
            return 0;

        }
    }
}
