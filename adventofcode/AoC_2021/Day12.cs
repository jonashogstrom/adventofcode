using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2021
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day12 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(10, 36, "Day12_test.txt")]
        [TestCase(19, 103, "Day12_test2.txt")]
        [TestCase(226, 3509, "Day12_test3.txt")]
        [TestCase(4549, null, "Day12.txt")]
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
            // end-zg
            var paths = new Dictionary<string, string>();

            var caverns = new Dictionary<string, Cavern>();
            foreach (var s in source)
            {
                var parts = s.Split('-');
                if (!caverns.TryGetValue(parts[0], out var c1))
                {
                    var name = parts[0];
                    c1 = new Cavern() { Name = name };
                    c1.Large = c1.Name[0] >= 'A' && c1.Name[0] <= 'Z';
                    caverns[name] = c1;
                }
                if (!caverns.TryGetValue(parts[1], out var c2))
                {
                    var name = parts[1];
                    c2 = new Cavern() { Name = name };
                    c2.Large = c2.Name[0] >= 'A' && c2.Name[0] <= 'Z';
                    caverns[name] = c2;
                }
                c1.Paths.Add(c2);
                c2.Paths.Add(c1);
                if (c1.Large && c2.Large)
                    throw new Exception("Loop warning");
            }
            LogAndReset("Parse", sw);
            part1 = FindPaths(caverns, caverns["start"]);
            LogAndReset("*1", sw);
            part2 = FindPaths2(caverns, caverns["start"]);
            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private long FindPaths(Dictionary<string, Cavern> caverns, Cavern start)
        {
            return FindRec(caverns, start, new HashSet<Cavern>(), new List<Cavern>());
        }
        private long FindPaths2(Dictionary<string, Cavern> caverns, Cavern start)
        {
            return FindRec2(caverns, start, new HashSet<Cavern>(), new List<Cavern>(), false);
        }

        private long FindRec(Dictionary<string, Cavern> caverns, 
            Cavern start, 
            HashSet<Cavern> visitedSmall, List<Cavern> path)
        {
            if (start.Name == "end")
                return 1;
            var res = 0L;
            Log($"Exploring {start.Name}");

            var newVisitedSmall = visitedSmall;
            var newPath = new List<Cavern>(path) { start };
            if (!start.Large)
            {
                newVisitedSmall = new HashSet<Cavern>(visitedSmall);
                newVisitedSmall.Add(start);
            }

            foreach (var c in start.Paths)
                if (c.Large || !newVisitedSmall.Contains(c))
                {

                    res += FindRec(caverns, c, newVisitedSmall, newPath);
                }

            return res;
        }

        private long FindRec2(Dictionary<string, Cavern> caverns,
            Cavern start,
            HashSet<Cavern> visitedSmall, List<Cavern> path, bool visit2)
        {
            if (start.Name == "end")
                return 1;
            var res = 0L;
            Log($"Exploring {start.Name}");

            var newVisitedSmall = visitedSmall;
            var newPath = new List<Cavern>(path) { start };
            if (!start.Large)
            {
                newVisitedSmall = new HashSet<Cavern>(visitedSmall);
                newVisitedSmall.Add(start);
            }

            foreach (var c in start.Paths)
                if ((c.Large || !newVisitedSmall.Contains(c) || !visit2)&& 
                    c.Name != "start")
                {
                    
                    res += FindRec2(caverns, c, newVisitedSmall, newPath, visit2||newVisitedSmall.Contains(c));
                }

            return res;
        }

    }

    [DebuggerDisplay("{Name} ({Large})")]
    internal class Cavern
    {
        public string Name;
        public bool Large;
        public List<Cavern> Paths = new List<Cavern>();
    }
}
