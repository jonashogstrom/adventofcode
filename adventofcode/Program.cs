using System;
using System.Collections.Generic;
using System.Linq;

namespace adventofcode
{
    class Program
    {

        [STAThread]
        static void Main()
        {
            var Aoc_2018_days = new List<BaseDay>
            {
                new AoC_2018.Day1(),
                new AoC_2018.Day2(),
                new AoC_2018.Day3(),
                new AoC_2018.Day4(),
                new AoC_2018.Day5(),
                new AoC_2018.Day6(),
                new AoC_2018.Day7(),
                new AoC_2018.Day8(),
                new AoC_2018.Day9(),
                new AoC_2018.Day10(),
//                new AoC_2018.Day11(),
                new AoC_2018.Day12(),
                new AoC_2018.Day13(),
                new AoC_2018.Day14(),
                new AoC_2018.Day15(),
                new AoC_2018.Day16(),
                new AoC_2018.Day17(),
                new AoC_2018.Day18(),
                new AoC_2018.Day19(),
                new AoC_2018.Day20(),
                new AoC_2018.Day21(),
                new AoC_2018.Day22(),
                new AoC_2018.Day23(),
                new AoC_2018.Day24(),
                new AoC_2018.Day25(),
            };

            var AoC_2016_days = new List<BaseDay>
            {
                new AoC_2016.Day1(),

            };
            var AoC_2015_days = new List<BaseDay>
            {
                new AoC_2015.Day1(),
            };

            var Aoc_2019_days = new List<BaseDay>
            {
                new AoC_2019.Day1(),
                new AoC_2019.Day2(),
            };


            Aoc_2019_days.Last().Run();

//            foreach(var d in days)
//                d.Run();

           
        }
    }
}
