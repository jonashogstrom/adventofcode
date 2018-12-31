using System;
using System.Threading;

namespace AoCStats
{
    class Program
    {
        static void Main(string[] args)
        {

            while (true)
            {
                new LeaderboardParser().GenerateReport(371692, 2018);
                new LeaderboardParser().GenerateReport(373164, 2018);
                new LeaderboardParser().GenerateReport(395782, new[] { 2018, 2017, 2016, 2015 });
                new LeaderboardParser().GenerateReport(34481, new[] { 2018, 2017, 2016, 2015 });
                Console.Write("Waiting...");
                for (int i = 0; i < 60; i++)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    Console.Write('.');
                    if (Console.KeyAvailable) return;
                }

                Console.WriteLine();
            }
        }
    }
}
