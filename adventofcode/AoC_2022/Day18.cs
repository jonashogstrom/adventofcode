using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Accord;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace AdventofCode.AoC_2022
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day18 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(64, 58, "Day18_test.txt")]
        [TestCase(4348, 2546, "Day18.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            LogLevel = resourceName.Contains("test") ? 20 : -1;
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1 = 0;
            Part2Type part2 = 0;
            var sw = Stopwatch.StartNew();
            var droplets = new HashSet<Coord3d>();
            foreach (var c in source.Select(Coord3d.Parse))
            {
                droplets.Add(c);
            }
            
            LogAndReset("Parse", sw);

            part1 = droplets.Sum(d => d.Neighbors6().Count(n => !droplets.Contains(n)));

            LogAndReset("*1", sw);

            var outsideCoords = new HashSet<Coord3d>();
            var q = new Queue<Coord3d>();
            // pick a coordinate outside the droplets
            var boundingBoxMax = droplets.Select(d => d.Max).Max() + 1;
            var boundingBoxMin = droplets.Select(d => d.Min).Min() - 1;
            var start = new Coord3d(boundingBoxMax, boundingBoxMax, boundingBoxMax);
            q.Enqueue(start);
            outsideCoords.Add(start);
            // find all coordinates outside
            while (q.Any())
            {
                var c = q.Dequeue();
                foreach (var n in c.Neighbors6())
                {
                    if (n.IsInsideCube(boundingBoxMin, boundingBoxMax) && !outsideCoords.Contains(n) && !droplets.Contains(n))
                    {
                        outsideCoords.Add(n);
                        q.Enqueue(n);
                    }
                }
            }
            part2 = droplets.Sum(d => d.Neighbors6().Count(neighbor => !droplets.Contains(neighbor) && outsideCoords.Contains(neighbor)));

            LogAndReset("*2", sw);

            return (part1, part2);
        }
    }
}