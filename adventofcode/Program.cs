using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace adventofcode
{
    class Program
    {

        [STAThread]
        static void Main()
        {
            var days = new List<BaseDay>
            {
                new Day1(),
                new Day2(),
                new Day3(),
                new Day4(),
                new Day5(),
                new Day6(),
                new Day7(),
                new Day8(),
                new Day9(),
                new Day10(),
//                new Day11(),
                new Day12(),
                new Day13(),
            };
            days.Last().Run();

            if (days.Last().Part2 != null)
            {
                new LeaderboardParser().GenerateReport(371692, 2018);
                new LeaderboardParser().GenerateReport(373164, 2018);
             //   new LeaderboardParser().GenerateReport(34481, new[] { 2018, 2017, 2016, 2015 });
            }
        }
    }
}
