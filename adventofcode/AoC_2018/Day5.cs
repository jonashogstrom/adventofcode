using System.Collections.Generic;
using System.Linq;

namespace AdventofCode.AoC_2018
{
    class Day5 : BaseDay
    {
        const string alpha = "abcdefghijklmnopqrstuvwxyz";

        protected override void Setup()
        {
            UseTestData = false;

            Part1Solution = 9370;
            Part2Solution = 6390;

            Part1TestSolution = 10;
            Part2TestSolution = 4;
        }

        protected override void DoRun(string[] input)
        {
            var rawdata = input.First();
            var list = new LinkedList<char>(rawdata);

            Reduce(list);
            Part1 = list.Count;

            var best = int.MaxValue;
            for (int ix = 0; ix < alpha.Length; ix++)
            {
                var c = alpha[ix];
                var C = char.ToUpper(alpha[ix]);

                var stripped = new LinkedList<char>(list.Where(x => x != c && x != C));
                Reduce(stripped);

                if (stripped.Count < best)
                    best = stripped.Count;
            }

            Part2 = best;
        }


        private void Reduce(LinkedList<char> list)
        {
            var head = list.First;
            while (head.Next != null)
            {
                var c1 = char.ToUpper(head.Value);
                var c2 = char.ToUpper(head.Next.Value);
                if (c1 == c2 && head.Value != head.Next.Value)
                {
                    // found match, perform reduction
                    if (head.Previous == null)
                    {
                        // match was in first char, remove first two
                        head = head.Next;
                        list.Remove(head.Previous);
                        head = head.Next;
                        list.Remove(head.Previous);
                    }
                    else
                    {
                        // Move one back and remove the two ahead
                        head = head.Previous;
                        list.Remove(head.Next);
                        list.Remove(head.Next);
                    }

                }
                else if (head.Next != null)
                {
                    // skip ahead
                    head = head.Next;
                }
            }
        }
    }
}