using System.Collections.Generic;
using System.Linq;

namespace AdventofCode.AoC_2018
{
    class Day4 : BaseDay
    {
        protected override void Setup()
        {
            Part1Solution = 38813;
            Part2Solution = 141071;
        }

        protected override void DoRun(string[] input)
        {
   
            var data = input.OrderBy(x=>x).ToList();
            var currentGuardId = 0;
            var asleep = 0;
            var guardList = new Dictionary<int, int[]>();
            foreach (var r in data)
            {
                var numbers = GetIntArr(r);
                var minutes = numbers[4];
                if (r.Contains("Guard"))
                {
                    currentGuardId = numbers.Last();
                    if (!guardList.ContainsKey(currentGuardId))
                        guardList[currentGuardId] = new int[60];
                }
                else if (r.Contains("falls asleep"))
                    asleep = minutes;
                else if (r.Contains("wakes up"))
                {
                    for (int i = asleep; i < minutes; i++)
                        guardList[currentGuardId][i]++;
                }
            }

            var sleepyGuard = guardList.Keys.OrderBy(k => guardList[k].Sum()).Last();
            var maxSleep = 0;
            var maxSleepMin = 0;
            for (int i=0; i<60; i++)
            {
                if (guardList[sleepyGuard][i] > maxSleep)
                {
                    maxSleep = guardList[sleepyGuard][i];
                    maxSleepMin = i;
                }
            }

           Log("Guard: " + sleepyGuard);
           Log("minute: " + maxSleepMin);
           Log("sleeptimes: " + maxSleep);

           Part1 = sleepyGuard * maxSleepMin;

            maxSleep = 0;
            maxSleepMin = -1;

            foreach (var id in guardList.Keys)
            {
                for (int i = 0; i < 60; i++)
                {
                    if (guardList[id][i] > maxSleep)
                    {
                        sleepyGuard = id;
                        maxSleepMin = i;
                        maxSleep = guardList[id][i];
                    }
                }
            }
           Log("Guard: "+sleepyGuard);
           Log("minute: "+maxSleepMin);
           Log("sleeptimes: "+maxSleep);
           Part2 = sleepyGuard*maxSleepMin;

        }
    }
}