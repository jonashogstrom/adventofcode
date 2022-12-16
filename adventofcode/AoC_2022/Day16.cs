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
            var tunnels = new Dictionary<string, Tunnel>();
            foreach (var s in source)
            {
                var parts = s.Split(new[] { ' ', '=', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                var tunnelName = parts[1];
                var rate = int.Parse(parts[5]);
                var x = new List<string>();
                for (var i = 10; i < parts.Length; i++)
                {
                    x.Add(parts[i].Trim());
                }

                var t = GetTunnel(tunnels, tunnelName);
                t.Rate = rate;
                foreach (var n in x)
                    t.Tunnels.Add(GetTunnel(tunnels, n));
            }

            var goodTunnels = tunnels.Values.Where(t => t.Rate > 0).ToList();
            foreach (var t in goodTunnels)
                foreach (var t2 in goodTunnels.Where(x => x != t))
                {
                    t.Distance[t2] = FindDist(t, t2);

                }
            LogAndReset("Parse", sw);


            var start = tunnels["AA"];
            var best = FindBestTunnelOrder(goodTunnels, start, 30);

            part1 = best;

            LogAndReset("*1", sw);

            var tunnelCount = goodTunnels.Count;
            var combos = 1 << tunnelCount;
            best = int.MinValue;
            for (var i = 0; i < combos; i++)
            {
                if (i % 100 == 0)
                    Log(i.ToString());
                var myTunnels = new List<Tunnel>();
                var elTunnels = new List<Tunnel>();
                for (int tunnelIndex = 0; tunnelIndex < goodTunnels.Count; tunnelIndex++)
                {
                    var bit = 1 << tunnelIndex;
                    if ((i & bit) == bit)
                    {
                        myTunnels.Add(goodTunnels[tunnelIndex]);
                    }
                    else
                    {
                        elTunnels.Add(goodTunnels[tunnelIndex]);
                    }
                }

                if (Math.Abs(myTunnels.Count - elTunnels.Count) < 2)
                {
                    var myBest = FindBestTunnelOrder(myTunnels, start, 26);
                    var elBest = FindBestTunnelOrder(elTunnels, start, 26);
                    var res = myBest + elBest;
                    if (res > best)
                    {
                        best = res;
                        Log($"Found res: {res}",-1);
                        Log($"My valves: " + string.Join(", ", myTunnels.Select(t => t.Name)), -1);
                        Log($"El valves: " + string.Join(", ", elTunnels.Select(t => t.Name)), -1);
                    }
                }
            }

            part2 = best;
            // solve part 2 here

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private int FindBestTunnelOrder(List<Tunnel> goodTunnels, Tunnel start, int timeAvailable)
        {
            var best = int.MinValue;
            foreach (var t in goodTunnels)
            {
                var initDist = FindDist(start, t);
                var path = new List<Tunnel> { start, t };
                var res = FindPath2(1 + initDist, t, new HashSet<Tunnel>(goodTunnels.Where(x => x != t)), 0, path,
                    new List<string>(), timeAvailable);
                if (res > best)
                    best = res;
            }

            return best;
        }

        private int FindPath2(int minute, Tunnel tunnel, HashSet<Tunnel> unvisitedTunnels, int accumulatedFlow, List<Tunnel> path, List<string> log, int timeAvailable)
        {
            if (tunnel.Name == "JJ")
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
            accumulatedFlow += tunnel.Rate * timeLeft;
            log.Add($"Minute {minute}, valve {tunnel.Name} adds {timeLeft} * {tunnel.Rate} = {tunnel.Rate * timeLeft}");
            var best = accumulatedFlow;
            foreach (var t in unvisitedTunnels)
            {
                var dist = tunnel.Distance[t];
                var newUnvisited = new HashSet<Tunnel>(unvisitedTunnels.Where(x => x != t));

                path.Add(t);
                var res = FindPath2(minute + dist, t, newUnvisited, accumulatedFlow, path, log, timeAvailable);
                if (res > best)
                {
                    best = res;
                }
                path.RemoveAt(path.Count - 1);
            }
            log.RemoveAt(log.Count - 1);

            return best;
        }

        private int FindDist(Tunnel start, Tunnel target)
        {
            var distances = new Dictionary<Tunnel, int>();
            distances[start] = 0;
            return FindDistRec(start, target, distances, 0);
        }

        private int FindDistRec(Tunnel start, Tunnel target, Dictionary<Tunnel, int> distances, int distance)
        {
            if (start == target)
                return distance;

            var best = int.MaxValue;
            foreach (var t in start.Tunnels)
            {
                var newDist = distance + 1;
                if (!distances.TryGetValue(t, out var d) || newDist < d)
                {
                    distances[t] = newDist;
                    var res = FindDistRec(t, target, distances, distance + 1);
                    best = Math.Min(res, best);
                }
            }
            return best;
        }


        private Tunnel GetTunnel(Dictionary<string, Tunnel> tunnels, string tunnelName)
        {
            if (tunnels.TryGetValue(tunnelName, out var t))
                return t;
            t = new Tunnel();
            t.Name = tunnelName;
            tunnels.Add(tunnelName, t);
            return t;
        }
    }

    [DebuggerDisplay("{Name}: {Rate} {Tunnels.Count}")]
    internal class Tunnel
    {
        public List<Tunnel> Tunnels { get; } = new List<Tunnel>();
        public string Name { get; set; }
        public int Rate { get; set; }
        public Dictionary<Tunnel, int> Distance { get; } = new Dictionary<Tunnel, int>();
    }
}