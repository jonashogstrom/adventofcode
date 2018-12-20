using System.Collections.Generic;
using System.Linq;

namespace adventofcode
{
    class Day8 : BaseDay
    {
        protected override void Setup()
        {
            UseTestData = false;
            Part1TestSolution = 138;
            Part2TestSolution = 66;
            Part1Solution = 48155;
            Part2Solution = 40292;
        }

        protected override void DoRun(string[] input)
        {
            var ints = GetIntArr(input.First());
            var pos = 0;
            var nodenum = 0;
            var key = ParseNode(ints, ref pos, ref nodenum);
            Part1 = key.SumMeta;
            Part2 = key.Sum2;
        }

        private Node ParseNode(int[] ints, ref int pos, ref int nodenum)
        {
            var children = ints[pos++];
            var metacount = ints[pos++];
            var res = new Node(nodenum++);

            for (int i=0; i<children; i++)
                res.AddChild(ParseNode(ints, ref pos, ref nodenum ));
            for (int i=0; i<metacount; i++)
                res.Addmeta(ints[pos++]);
            return res;
        }

        internal class Node
        {
            public List<int> Meta { get; } = new List<int>();
            public List<Node> Nodes { get; } = new List<Node>();

            public int SumMeta
            {
                get
                {
                    return Meta.Sum() + Nodes.Select(n => n.SumMeta).Sum();
                }
            }

            public int Sum2
            {
                get
                {
                    if (Nodes.Count == 0)
                        return Meta.Sum();
                    var sum = 0;
                    for (int i = 0; i < Meta.Count; i++)
                    {
                        var pos = Meta[i] - 1;
                        if (pos < Nodes.Count)
                            sum += Nodes[pos].Sum2;
                    }

                    return sum;

                }
            }

            public Node(int v)
            {
                Name = ((char)v + 65).ToString();
            }

            public string Name { get; }

            public void Addmeta(int i)
            {
                Meta.Add(i);
            }

            public void AddChild(Node n)
            {
                Nodes.Add(n);
            }
        }
    }


}