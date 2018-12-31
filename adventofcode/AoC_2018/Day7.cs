using System;
using System.Collections.Generic;
using System.Linq;

namespace adventofcode.AoC_2018
{
    class Day7 : BaseDay
    {
        protected override void Setup()
        {
            UseTestData = false;
            Part1TestSolution = "CABDFE";
            Part2TestSolution = 15;
            Part1Solution = "OKBNLPHCSVWAIRDGUZEFMXYTJQ";
            Part2Solution = 982;
        }

        protected override void DoRun(string[] input)
        {
            var workerCount = UseTestData ? 2 : 5;
            var stepMinTime = UseTestData ? 0 : 60;

            var graph = new List<Tuple<char, char>>();
            var letters = new HashSet<char>();
            foreach (var row in input)
            {
                var parts = row.Split(' ');
                var before = parts[1][0];
                var after = parts[7][0];
                letters.Add(before);
                letters.Add(after);
                graph.Add(new Tuple<char, char>(before, after));
            }

            var s = "";

            while (letters.Any())
            {
                var candidates = new HashSet<char>(letters);
                foreach (var cond in graph)
                {
                    candidates.Remove(cond.Item2);
                }

                var pick = candidates.OrderBy(c => c).First();
                s += pick;
                letters.Remove(pick);
                graph.RemoveAll(t => t.Item1 == pick);
            }

            Part1 = s;


            //=============
            foreach (var row in input)
            {
                var parts = row.Split(' ');
                var before = parts[1][0];
                var after = parts[7][0];
                letters.Add(before);
                letters.Add(after);
                graph.Add(new Tuple<char, char>(before, after));
            }

            var workerQueue = new int[workerCount];

            var totalTime = 0;

            s = "";
            var workinprogress = new Dictionary<char, int>();

            while (letters.Any())
            {
                var candidates = new HashSet<char>(letters);

                foreach (var cond in graph)
                {
                    candidates.Remove(cond.Item2);
                }

                while (!candidates.Any())
                {
                    totalTime++;
                    foreach (var c in workinprogress.Keys)
                    {
                        if (workinprogress[c] <= totalTime)
                        {
                            graph.RemoveAll(t => t.Item1 == c);
                            workinprogress.Remove('c');
                        }
                        for (int i = 0; i < workerCount; i++)
                        {
                            if (workerQueue[i] > 0)
                                workerQueue[i] -= 1;
                        }
                    }

                    candidates = new HashSet<char>(letters);
                    foreach (var cond in graph)
                    {
                        candidates.Remove(cond.Item2);
                    }

                }

                var waitTime = workerQueue.Min();
                totalTime += waitTime;
                var nextWorker = -1;
                for (int i = 0; i < workerCount; i++)
                {
                    workerQueue[i] -= waitTime;
                    if (workerQueue[i] == 0)
                        nextWorker = i;
                }
                var pick = candidates.OrderBy(c => c).First();
                s += pick;
                Log(pick + " was started at " + totalTime);

                var taskTime = (pick - 65) + stepMinTime+1;
                workerQueue[nextWorker] = taskTime;

                letters.Remove(pick);
                workinprogress[pick] = totalTime + taskTime;
            }

            totalTime += workerQueue.Max();
            Part2 = totalTime;
        }
    }
}