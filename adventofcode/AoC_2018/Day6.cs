using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventofCode.AoC_2018
{
    class Day6 : BaseDay
    {
        protected override void Setup()
        {
            UseTestData = false;
            Part1TestSolution = 17;
            Part2TestSolution = 16;
            Part1Solution = 3604;
            Part2Solution = 46563;
        }

        protected override void DoRun(string[] input)
        {
            int regionSize = UseTestData ? 32 : 10000;

            int[][] coords = new int[input.Length][];
            for (int row = 0; row < input.Length; row++)
            {
                coords[row] = GetIntArr(input[row]);
            }

            var gridsize = coords.Max(c => Math.Max(c[0], c[1])) + 3;

            var grid = new int[gridsize * gridsize];
            var inRegion = 0;
            var dic = new Dictionary<int, int>();
            var edge = new HashSet<int>();

            for (int x = 0; x < gridsize; x++)
                for (int y = 0; y < gridsize; y++)
                {
                    int closest = -2;
                    var dist = int.MaxValue;
                    var totalDist = 0;
                    var allDists = new List<int>();
                    for (int row = 0; row < input.Length; row++)
                    {
                        var coord = coords[row];
                        var currentdist = Math.Abs(coord[0] - x) + Math.Abs(coord[1] - y);
                        allDists.Add(currentdist);
                        if (currentdist == dist)
                            closest = -1;
                        else if (currentdist < dist)
                        {
                            closest = row;
                            dist = currentdist;
                        }

                        totalDist += currentdist;
                    }

                    if (totalDist < regionSize)
                        inRegion++;

                    if ((x == 0 || x == gridsize - 1 || y == 0 || y == gridsize - 1) && !edge.Contains(closest))
                        edge.Add(closest);

                    if (closest >= 0)
                    {
                        if (!dic.ContainsKey(closest))
                            dic[closest] = 0;

                        dic[closest]++;
                    }

                    grid[x + y * gridsize] = closest;
                }

            if (UseTestData)
            {
                for (int x = 0; x < gridsize; x++)
                {
                    var s = "";
                    for (int y = 0; y < gridsize; y++)
                    {
                        var i = grid[x + y * gridsize];

                        s += i == -1 ? '.' : (char)(i + 65);
                    }

                    Log(s);
                }
            }

            var largest = dic.Keys.OrderByDescending(k => dic[k]).First(k => !edge.Contains(k));

            Log("Largest area belongs to coord #" + largest);
            Part1 = grid.Count(x => x == largest);
            Part2 = inRegion;
        }
    }

}
