using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace AdventofCode.AoC_2019
{
    class Day6 : BaseDay
    {
        protected override void Setup()
        {
            Source = InputSource.test;
            //Source = InputSource.prod;

            LogLevel = UseTestData ? 5 : 0;

            Part1TestSolution = 42;
            Part2TestSolution = 4;
            Part1Solution = null;
            Part2Solution = null;
        }

        private class orbit
        {

            private string source;
        }

        protected override void DoRun(string[] input)
        {
            var objects = new HashSet<string>();


            var orbits = new Dictionary<string, List<string>>();
            orbits["COM"] = new List<string>();
            var parents = new Dictionary<string, string>();
            foreach (var r in input)
            {
                var p = r.Split(')');
                if (!orbits.ContainsKey(p[1]))
                    orbits[p[1]] = new List<string>();
                //    orbits[p[0]].Add(p[1]);
                parents[p[1]] = p[0];
            }

            //            Part1 = CalcRec("COM", orbits, 0);

            var sum = 0;

            foreach (var obj in parents.Keys)
                sum += FindDepth(obj, parents, 0);
            Part1 = sum;

            // not 432, 428

            var x = CollectPath("YOU", parents);
            var y = CollectPath("SAN", parents);


            var temp = 0;
            while (x[temp] == y[temp])
                temp++;
            Part2 = (x.Count - 1 - (temp)) + (y.Count - 1 - (temp));

        }

        private List<string> CollectPath(string start, Dictionary<string, string> parents)
        {
            if (parents.ContainsKey(start))
            {
                var x = CollectPath(parents[start], parents);
                x.Add(start);
                return x;
            }
            return new List<string>() { start };

        }

        private int FindDepth(string s, Dictionary<string, string> parents, int depth)
        {
            if (parents.ContainsKey(s))
                return FindDepth(parents[s], parents, depth + 1);
            return depth;
        }

        private int CalcRec(string obj, Dictionary<string, List<string>> orbits, int depth)
        {
            var sum = depth;
            foreach (var x in orbits[obj])
                sum += depth + CalcRec(x, orbits, depth + 1);
            return sum;
        }
    }
}