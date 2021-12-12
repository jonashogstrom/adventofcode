using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
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
        [TestCase(4549, 120535, "Day12.txt")]
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
            var caverns = new Dictionary<string, Cavern>();
            foreach (var s in source)
            {
                var parts = s.Split('-');
                var c1 = GetCavern(caverns, parts[0]);
                var c2 = GetCavern(caverns, parts[1]);

                if (!c2.IsStart)
                    c1.Paths.Add(c2);
                if (!c1.IsStart)
                    c2.Paths.Add(c1);
                if (c1.Large && c2.Large)
                    throw new Exception("Loop warning");
            }

            LogAndReset("Parse", sw);
            part1 = FindPaths2(caverns, caverns["start"], false);
            LogAndReset("*1", sw);
            part2 = FindPaths2(caverns, caverns["start"], true);
            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private Cavern GetCavern(Dictionary<string, Cavern> caverns, string name)
        {
            if (!caverns.TryGetValue(name, out var c))
            {
                c = new Cavern() { Name = name };
                c.Large = c.Name[0] >= 'A' && c.Name[0] <= 'Z';
                c.IsStart = name == "start";
                c.IsEnd = name == "end";
                caverns[name] = c;
            }

            return c;
        }

        private long FindPaths2(Dictionary<string, Cavern> caverns, Cavern start, bool allowSecondVisit)
        {
            return FindRec2( start, new HashSet<Cavern>(), allowSecondVisit);
        }

        private long FindRec2(
            Cavern start,
            HashSet<Cavern> visitedSmall, bool allowSecondVisit)
        {
            if (start.IsEnd)
                return 1;
            var res = 0L;

            var added = false;
            if (!start.Large && !visitedSmall.Contains(start))
            {
                visitedSmall.Add(start); 
                added = true;
            }

            foreach (var c in start.Paths)
            {
                if (c.Large || !visitedSmall.Contains(c) || allowSecondVisit)
                {
                    res += FindRec2(c, visitedSmall, allowSecondVisit && !visitedSmall.Contains(c));
                }}

            if (added)
            {
                visitedSmall.Remove(start); 
            }

            return res;
        }
    }

    [DebuggerDisplay("{Name} ({Large})")]
    internal class Cavern
    {
        public string Name;
        public bool Large;
        public bool IsStart;
        public bool IsEnd;
        public List<Cavern> Paths = new List<Cavern>();
    }
}