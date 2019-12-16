using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventofCode
{
    class Program
    {

        [STAThread]
        static void Main()
        {
            var AoC_2015_days = new List<BaseDay>
            {
                new AoC_2015.Day1(),
            };
            var AoC_2016_days = new List<BaseDay>
            {
                new AoC_2016.Day1(),

            };
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
            var Aoc_2019_days = new List<BaseDay>
            {
                new AoC_2019.Day1(), // Fuel calculations for take off, recursive
                new AoC_2019.Day2(), // IntCodeComputer, only Add and Mul (and terminate)
                new AoC_2019.Day3(), // Cables intersecting on a grid (manhattan)
                new AoC_2019.Day4(), // 6 digit passwords with rules, sequences of numbers.
                new AoC_2019.Day5(), // IntCodeComputer, parameter modes, input/output, jumps and comparisons
                new AoC_2019.Day6(), // Orbital maps, dist YOU => SAN
                new AoC_2019.Day7(), // more IntCode, serial and parallel execution with input loops
                new AoC_2019.Day8(), // Space image format, checksums and transparent pixels.
                //new AoC_2019.Day9(), // IntCodeComputer with relative mode parameters and bignum support
                //new AoC_2019.Day10(), // Asteroid fields, hidden asteroids and lasers (and some trigonometry)
                // Day 11 Painting the registration code on the hull using IntCode brains
                // Day 12 4 moon problems, Manhattan gravity and cycles detection
                // Day 13 Break out on an int computer
                // Day 14 ORE => Fuel, recepies and recursion

            };

            Aoc_2019_days.Last().Run();

//            foreach(var d in Aoc_2019_days)
//                d.Run();
        }
    }
}
