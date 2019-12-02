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
                // Tobii
                new LeaderboardParser().GenerateReport(371692, new[] { 2019, 2018 }, true);
                new LeaderboardParser().GenerateReport(371692, new[] { 2019, 2018 }, false);

                // Leica
                 new LeaderboardParser().GenerateReport(373164, new[] { 2019, 2018 }, false);

                // kodaporna
                new LeaderboardParser().GenerateReport(395782, new[] { 2019, 2018, 2017, 2016, 2015 }, false);

                // jeppes lista
                new LeaderboardParser().GenerateReport(34481, new[] { 2019, 2018, 2017, 2016, 2015 }, false);

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
