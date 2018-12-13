using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;


namespace adventofcode
{
    class Day9 : BaseDay
    {
        protected override void Setup()
        {
            UseTestData = false;
            Part1TestSolution = null;
            Part2TestSolution = null;
            Part1Solution = (long)410375;
            Part2Solution = (long)3314195047;
        }

        protected override void DoRun(string[] input)
        {
            foreach (var game in input)
            {
                var data = GetIntArr(game).ToList();
                var elfCount = data[0];
                var marbleCount = data[1];
                Part1 = CalculateGameScore(elfCount, marbleCount, data);
                Part2 = CalculateGameScore(elfCount, marbleCount*100, data.Take(2).ToList());
            }

        }

        private long CalculateGameScore(int elfCount, int marbleCount, List<int> data)
        {
            var elfScore = new long[elfCount];
            var marbleRing = new LinkedList<int>();
            marbleRing.AddFirst(new LinkedListNode<int>(0));
            var current = marbleRing.First;
            var currentElf = 0;
            for (int i = 1; i < marbleCount; i++)
            {
                if (i % 23 == 0)
                {
                    elfScore[currentElf] += i;
                    current = moveList(current, -7);
                    elfScore[currentElf] += current.Value;
                    current = moveList(current, +1);
                    if (current.Previous != null)
                        marbleRing.Remove(current.Previous);
                    else
                    {
                        marbleRing.RemoveLast();
                    }
                }
                else
                {
                    current = moveList(current, +1);
                    var newNode = new LinkedListNode<int>(i);
                    marbleRing.AddAfter(current, newNode);
                    current = newNode;
                }

                currentElf = (currentElf + 1) % elfCount;
            }

            var maxScore = elfScore.Max();
            if (data.Count == 3)
            {
                if (maxScore != data[2])
                    Log($"ERROR: Players={elfCount} Marbles={marbleCount} Exp: {data[2]} Act: {maxScore}");
                else
                {
                    Log($"OK: Players={elfCount} Marbles={marbleCount} result: {maxScore}");
                }

            }
            return maxScore;
        }

        private LinkedListNode<int> moveList(LinkedListNode<int> current, int p1)
        {
            if (p1 > 0)
            {
                for (int i = 0; i < p1; i++)
                {
                    if (current.Next == null)
                        current = current.List.First;
                    else
                    {
                        current = current.Next;

                    }
                }
            }
            else
            {
                for (int i = 0; i < Math.Abs(p1); i++)
                {
                    if (current.Previous== null)
                        current = current.List.Last;
                    else
                    {
                        current = current.Previous;
                    }
                }
            }

            return current;
        }
    }
}