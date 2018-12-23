using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace adventofcode
{
    internal class Day23 : BaseDay
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
            
        }
    }
}