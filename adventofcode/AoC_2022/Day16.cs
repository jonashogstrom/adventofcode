using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace AdventofCode.AoC_2022
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day16 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(1651, 1707, "Day16_test.txt")]
        [TestCase(2330, 2675, "Day16.txt")]
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

            // parse input here
            var valves = new Dictionary<string, Valve>();
            foreach (var s in source)
            {
                var parts = s.Split(new[] { ' ', '=', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                var valveName = parts[1];
                var rate = int.Parse(parts[5]);
                var x = new List<string>();
                for (var i = 10; i < parts.Length; i++)
                {
                    x.Add(parts[i].Trim());
                }

                var valve = GetValve(valves, valveName);
                valve.FlowRate = rate;
                foreach (var n in x)
                    valve.Tunnels.Add(GetValve(valves, n));
            }

            var goodValves = valves.Values.Where(v => v.FlowRate > 0).ToList();
            foreach (var valve in goodValves)
                foreach (var otherValve in goodValves.Where(x => x != valve))
                {
                    valve.Distance[otherValve] = FindDist(valve, otherValve);

                }
            LogAndReset("Parse", sw);


            var start = valves["AA"];
            var best = FindBestValveOrder(goodValves, start, 30);

            part1 = best;

            LogAndReset("*1", sw);

            var valveCount = goodValves.Count;
            var combos = 1 << valveCount;
            best = int.MinValue;
            for (var i = 0; i < combos; i++)
            {
                if (i % 100 == 0)
                    Log(i.ToString());
                var myValves = new List<Valve>();
                var elephantValves = new List<Valve>();
                for (int valveIndex = 0; valveIndex < goodValves.Count; valveIndex++)
                {
                    var bit = 1 << valveIndex;
                    if ((i & bit) == bit)
                    {
                        myValves.Add(goodValves[valveIndex]);
                    }
                    else
                    {
                        elephantValves.Add(goodValves[valveIndex]);
                    }
                }

                if (Math.Abs(myValves.Count - elephantValves.Count) < 2)
                {
                    var myBest = FindBestValveOrder(myValves, start, 26);
                    var elBest = FindBestValveOrder(elephantValves, start, 26);
                    var res = myBest + elBest;
                    if (res > best)
                    {
                        best = res;
                        Log($"Found res: {res}",-1);
                        Log($"My valves: " + string.Join(", ", myValves.Select(t => t.Name)), -1);
                        Log($"El valves: " + string.Join(", ", elephantValves.Select(t => t.Name)), -1);
                    }
                }
            }

            part2 = best;
            // solve part 2 here

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private int FindBestValveOrder(List<Valve> goodValves, Valve start, int timeAvailable)
        {
            var best = int.MinValue;
            foreach (var valve in goodValves)
            {
                var initDist = FindDist(start, valve);
                var path = new List<Valve> { start, valve };
                var res = FindPath2(1 + initDist, valve, new HashSet<Valve>(goodValves.Where(x => x != valve)), 0, path,
                    new List<string>(), timeAvailable);
                if (res > best)
                    best = res;
            }

            return best;
        }

        private int FindPath2(int minute, Valve valve, HashSet<Valve> unvisitedValves, int accumulatedFlow, List<Valve> path, List<string> log, int timeAvailable)
        {
            if (valve.Name == "JJ")
            {
                log.Add("xxx");
            }
            if (minute > timeAvailable)
            {
                // Log("----");
                // Log("Path: " + string.Join(",", path.Select(x => x.Name)));
                // Log("Res: " + accumulatedFlow);
                // foreach (var s in log)
                //     Log(s);
                return accumulatedFlow;
            }

            // open valve
            minute += 1;
            var timeLeft = (timeAvailable - minute) + 1;
            accumulatedFlow += valve.FlowRate * timeLeft;
            log.Add($"Minute {minute}, valve {valve.Name} adds {timeLeft} * {valve.FlowRate} = {valve.FlowRate * timeLeft}");
            var best = accumulatedFlow;
            foreach (var v in unvisitedValves)
            {
                var dist = valve.Distance[v];
                var newUnvisited = new HashSet<Valve>(unvisitedValves.Where(x => x != v));

                path.Add(v);
                var res = FindPath2(minute + dist, v, newUnvisited, accumulatedFlow, path, log, timeAvailable);
                if (res > best)
                {
                    best = res;
                }
                path.RemoveAt(path.Count - 1);
            }
            log.RemoveAt(log.Count - 1);

            return best;
        }

        private int FindDist(Valve start, Valve target)
        {
            var distances = new Dictionary<Valve, int>();
            distances[start] = 0;
            return FindDistRec(start, target, distances, 0);
        }

        private int FindDistRec(Valve start, Valve target, Dictionary<Valve, int> distances, int distance)
        {
            if (start == target)
                return distance;

            var best = int.MaxValue;
            foreach (var valve in start.Tunnels)
            {
                var newDist = distance + 1;
                if (!distances.TryGetValue(valve, out var d) || newDist < d)
                {
                    distances[valve] = newDist;
                    var res = FindDistRec(valve, target, distances, distance + 1);
                    best = Math.Min(res, best);
                }
            }
            return best;
        }


        private Valve GetValve(Dictionary<string, Valve> valves, string valveName)
        {
            if (valves.TryGetValue(valveName, out var t))
                return t;
            t = new Valve
            {
                Name = valveName
            };
            valves.Add(valveName, t);
            return t;
        }
    }

    [DebuggerDisplay("{Name}: {FlowRate} {Tunnels.Count}")]
    internal class Valve
    {
        public List<Valve> Tunnels { get; } = new List<Valve>();
        public string Name { get; set; }
        public int FlowRate { get; set; }
        public Dictionary<Valve, int> Distance { get; } = new Dictionary<Valve, int>();
    }
}