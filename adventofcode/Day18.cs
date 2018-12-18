using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace adventofcode
{
    internal class Day18 : BaseDay
    {
        protected override void Setup()
        {
            Source = InputSource.test;
            Source = InputSource.prod;

            LogLevel = UseTestData ? 5 : 3;

            Part1TestSolution = null;
            Part2TestSolution = null;
            Part1Solution = null;
            Part2Solution = null;
        }

        protected override void DoRun(string[] input)
        {
            
        }
    }
}