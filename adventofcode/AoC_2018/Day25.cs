using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AdventofCode.AoC_2018
{
    internal class Day25 : BaseDay
    {
        protected override void Setup()
        {
            // inte 405
            Source = InputSource.test;
                        Source = InputSource.prod;

            LogLevel = UseTestData ? 5 : 0;

            Part1TestSolution = null;
            Part2TestSolution = null;
            Part1Solution = null;
            Part2Solution = null;
        }

        protected override void DoRun(string[] input)
        {
            var coords = new List<Coord4d>();
            foreach (var x in input)
            {
                coords.Add(new Coord4d() { c = GetIntArr(x, true) });
            }

            var clusters = new List<List<Coord4d>>();
            foreach (var c in coords)
            {
                var matchingClusters = new List<List<Coord4d>>();
                foreach (var cluster in clusters)
                {
                    foreach (var star in cluster)
                    {
                        if (star.distance(c) <= 3)
                        {
                            matchingClusters.Add(cluster);
                            break;
                        }

                    }
                }

                if (matchingClusters.Count == 0)
                {
                    clusters.Add(new List<Coord4d>(){c});
                }
                else if (matchingClusters.Count == 1)
                {
                    matchingClusters.First().Add(c);
                }
                else
                {
                    var newCluster = new List<Coord4d>();
                    foreach (var oldCluster in matchingClusters)
                    {
                        clusters.Remove(oldCluster);
                        newCluster.AddRange(oldCluster);
                    }

                    newCluster.Add(c);
                    clusters.Add(newCluster);
                }
            }

            Part1 = clusters.Count;
        }


        [DebuggerDisplay("{s()}")]
        public class Coord4d
        {
            public int[] c;

            public int distance(Coord4d other)
            {
                var d = 0;
                for (int i = 0; i < c.Length; i++)
                    d += Math.Abs(c[i] - other.c[i]);
                return d;
            }

            public string s()
            {
                string str = "";

                foreach (var x in c)
                    str += x + ", ";
                return str;
            }
        }
    }
}