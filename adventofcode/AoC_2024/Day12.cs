using System;
using System.Diagnostics;
using NUnit.Framework;
using System.Collections.Generic;
using AdventofCode.Utils;
using System.Linq;

namespace AdventofCode.AoC_2024
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day12 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(140, 80, "Day12_test.txt")]
        [TestCase(772, 436, "Day12_test2.txt")]
        [TestCase(1930, 1206, "Day12_test3.txt")]
        [TestCase(null, 236, "Day12_test4.txt")]
        [TestCase(null, 368, "Day12_test5.txt")]
        [TestCase(1431440, null, "Day12.txt")]
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

            var map = source.ToSparseBufferStr();


            LogAndReset("Parse", sw);
            var unhandled = map.Keys.ToHashSet();

            var regions = new List<Region>();
            while (unhandled.Count > 0)
            {
                var toExplore = new HashSet<CoordStr> { unhandled.First() };
                unhandled.Remove(unhandled.First());
                var r = new Region();
                regions.Add(r);
                while (toExplore.Count > 0)
                {
                    var exp = toExplore.First();
                    r.Plots.Add(exp);
                    toExplore.Remove(exp);
                    foreach (var x in exp.GenAdjacent4())
                    {
                        if (map[x] == map[exp])
                        {
                            if (!r.Plots.Contains(x))
                            {
                                toExplore.Add(x);
                                unhandled.Remove(x);
                            }
                        }
                        else
                        {
                            r.Fences++;
                            r.FenceByDir[exp.Subtract(x)].Add(exp);
                        }
                    }
                }
                Log($"Region {map[r.Plots.First()]}: Area: {r.Plots.Count}, fences: {r.Fences}");
            }

            foreach (var r in regions)
                part1 += r.Fences * r.Plots.Count;
            
            LogAndReset("*1", sw);

            foreach (var r in regions)
            {
                
                foreach (var d in CoordStr.Directions4)
                {
                    var toExplore = new HashSet<CoordStr> ( r.FenceByDir[d] );
                    //var clusters = new List<HashSet<CoordStr>>();
                    while (toExplore.Any())
                    {
                        var root = toExplore.First();
                        toExplore.Remove(root);
                        FindAndRemove(toExplore, root, d.RotateCW90());
                        FindAndRemove(toExplore, root, d.RotateCCW90());
                        r.Sides++;
                    }
                
                }
                
            }

            foreach (var r in regions)
                part2 += r.Sides * r.Plots.Count;
            
            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private void FindAndRemove(HashSet<CoordStr> toExplore, CoordStr root, CoordStr dir)
        {
            var next = root.Move(dir);
            while (toExplore.Contains(next))
            {
                toExplore.Remove(next);
                next = next.Move(dir);
            }
        }
    }

    internal class Region
    {
        public Region()
        {
            foreach(var d in CoordStr.Directions4)
                FenceByDir[d] = [];
        }

        public HashSet<CoordStr> Plots { get; } = [];
        public int Fences { get; set; }
        public Dictionary<CoordStr, List<CoordStr>> FenceByDir { get; } = new();
        public int Sides { get; set; }
    }
}