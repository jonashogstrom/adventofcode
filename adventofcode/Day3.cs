using System;
using System.Collections.Generic;
using System.Linq;

namespace adventofcode
{
    class Day3 : BaseDay
    {
        protected override void Setup()
        {
            Part1Solution = 110389;
            Part2Solution = 552;
        }

        protected override void DoRun(string[] input)
        {
            var allClaims = new List<Claimzx>();
            foreach (var s in input)
            {
                //var c = ParseClaim("#123 @ 3,2: 5x4");
                var c = ParseClaim(s);
                allClaims.Add(c);
            }

            var gridheight = allClaims.Max(c => c.top + c.height);
            var maxClaim = allClaims.Max(c => c.claimid);

            var grid = new Dictionary<int, List<Claimzx>>();
            bool[] overlapping = new bool[maxClaim + 1];

            foreach (var claim in allClaims)
            {

                for (int y = claim.top + 1; y <= claim.top + claim.height; y++)
                    for (int x = claim.left + 1; x <= claim.left + claim.width; x++)
                    {
                        var square = x + y * gridheight;
                        if (!grid.TryGetValue(square, out var list))
                            grid[square] = list = new List<Claimzx>();
                        list.Add(claim);
                        if (list.Count > 1)
                            foreach (var cl in list)
                                overlapping[cl.claimid] = true;
                    }
            }

            Part1 = grid.Values.Count(x => x.Count > 1);
            foreach (var cl in allClaims)
                if (overlapping[cl.claimid] == false)
                    Part2 = cl.claimid;
        }



        private Claimzx ParseClaim(string s)
        {
            //#1248 @ 268,895: 17x24
            //#1249 @ 591,960: 15x20
            var parts = GetIntArr(s);
            return new Claimzx()
            {
                claimid = parts[0],
                left = parts[1],
                top = parts[2],
                width = parts[3],
                height = parts[4]
            };
        }
    }

    internal struct Claimzx
    {
        public int claimid;
        public int top;
        public int left;
        public int width;
        public int height;
    }
}