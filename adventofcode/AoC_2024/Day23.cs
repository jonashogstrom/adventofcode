using System;
using System.Diagnostics;
using NUnit.Framework;
using System.Collections.Generic;
using AdventofCode.Utils;
using System.Linq;
using System.Net.Sockets;


namespace AdventofCode.AoC_2024
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = string;

    [TestFixture]
    class Day23 : TestBaseClass2<Part1Type, Part2Type>
    {
        private HashSet<string> _maxClique = new HashSet<string>();
        private List<string[]> _cliques;
        public bool Debug { get; set; }

        [Test]
        [TestCase(7, "co,de,ka,ta", "Day23_test.txt")]
        [TestCase(1062, "xxx", "Day23.txt")]
        public void Test1(Part1Type exp1, Part2Type exp2, string resourceName)
        {
            LogLevel = resourceName.Contains("test") ? 20 : -1;
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type part1, Part2Type part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1 = 0;
            Part2Type part2 = "";
            var sw = Stopwatch.StartNew();

            var connections = new Dictionary<string, HashSet<string>>();
            foreach (var s in source)
            {
                var parts = s.Split('-');
                AddConnection(connections, parts[0], parts[1]);
                AddConnection(connections, parts[1], parts[0]);
            }

            LogAndReset("Parse", sw);

            var found = new List<HashSet<string>>();
            foreach (var c in connections.Keys.Where(x=>x.StartsWith('t')))
            {
                var connected = connections[c];
                foreach (var p in connected.AsCombinations())
                {
                    if (connections[p.Item1].Contains(p.Item2))
                    {
                        var x = new HashSet<string>(){p.Item1, p.Item2, c};
                        if (found.All(l => l.Intersect(x).Count() != 3))
                        {
                            Log($"FullyConnected: {c}, {p.Item1}, {p.Item2}");
                            found.Add(x);
                        }
                    }
                }
            }

            part1 = found.Count();

            LogAndReset("*1", sw);
            var upperBoundCliqueSize = connections.Values.Max(s=>s.Count+1);

            _cliques = new List<string[]>();
            BronKerbosch2([], connections.Keys.ToArray(), [], connections);
            var maxClique = _cliques.OrderByDescending(x => x.Length).First();
            part2 = string.Join(',', maxClique.OrderBy(x => x));

            

            // // find the largest, fully connected clique 
            // foreach(var root in connections.Keys)
            //     FindMaxClique([root], connections[root], connections, new HashSet<string>());

            LogAndReset("*2", sw);

            part2 = string.Join(',', _maxClique.OrderBy(x => x));
            return (part1, part2);
        }

        // https://en.wikipedia.org/wiki/Bron%E2%80%93Kerbosch_algorithm
        // implementaiton is very similar to day23 2018, but it doesn't work, only finds cliques of size 12
        
        private void BronKerbosch2(string[] r, string[] p, string[] x,
            Dictionary<string, HashSet<string>> connections)
        {
            if (p.Length == 0 && x.Length == 0)
            {
                if (r.Length >_maxClique.Count)
                    _maxClique = r.ToHashSet();
                _cliques.Add(r);
                return;
            }
            var pivot = p.Union(x).OrderByDescending(comp => connections[comp].Count).First();
            var neighbours = connections[pivot];

            foreach (var v in p.Except(neighbours))
            {
                var newR = r.Append(v).ToArray();
                var newP = p.Intersect(connections[v]).ToArray();
                var newX = x.Intersect(connections[v]).ToArray();
                BronKerbosch2(newR, newP, newX, connections);
                p = p.Where(a=>a != v).ToArray();
                x = x.Append(v).ToArray();
            }
        }


        private void FindMaxClique(HashSet<string> clique,
            HashSet<string> candidates,
            Dictionary<string, HashSet<string>> connections, 
            HashSet<string> preSkipCandidates)
        {
            var foundCandidate = false;
            var skipCandidates = new HashSet<string>();
            var myCandidates = candidates.Except(skipCandidates).ToList();
            foreach (var c in myCandidates)
            {
                var candidateConnections = connections[c];
                if (candidateConnections.Count() < clique.Count)
                {   
                    skipCandidates.Add(c);
                    continue;
                }
                if (clique.All(x=>candidateConnections.Contains(x)))
                {
                    var newClique = new HashSet<string>(clique);
                    newClique.Add(c);
                    var newCandidates = myCandidates.Where(x=>connections[x].Count >= clique.Count && x!= c).ToHashSet(); 
                    FindMaxClique(newClique, newCandidates, connections, skipCandidates);
                    foundCandidate = true;
                }
                else
                {
                    skipCandidates.Add(c);  
                }
            }
            if (!foundCandidate)
                if (clique.Count > _maxClique.Count)
                {
                    Log("FoundCliqueOfSize "+clique.Count);
                    _maxClique = clique;
                    Log(string.Join(',', _maxClique.OrderBy(x => x)));

                }
        }

        private void AddConnection(Dictionary<string,HashSet<string>> connections, string c1, string c2)
        {
            if (!connections.ContainsKey(c1))
                connections.Add(c1, []);
            connections[c1].Add(c2);
        }
    }
}