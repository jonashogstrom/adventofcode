using System;
using System.Threading;

namespace AoCStats
{
    class Program
    {
        static void Main(string[] args)
        {

            var forceLoad = false;
            while (true)
            {
                     
                // Tobii
                new LeaderboardParser().GenerateReport(371692, new[] { 2019, 2018 }, true, forceLoad);
                new LeaderboardParser().GenerateReport(371692, new[] { 2019, 2018 }, false, forceLoad);

                // Leica
                 new LeaderboardParser().GenerateReport(373164, new[] { 2019, 2018 }, true, forceLoad);
                 new LeaderboardParser().GenerateReport(373164, new[] { 2019, 2018 }, false, forceLoad);

                // kodaporna
                new LeaderboardParser().GenerateReport(395782, new[] { 2019, 2018, 2017, 2016, 2015 }, true, forceLoad);
                new LeaderboardParser().GenerateReport(395782, new[] { 2019, 2018, 2017, 2016, 2015 }, false, forceLoad);

                // jeppes lista
                new LeaderboardParser().GenerateReport(34481, new[] { 2019, 2018, 2017, 2016, 2015 }, true, forceLoad);
                new LeaderboardParser().GenerateReport(34481, new[] { 2019, 2018, 2017, 2016, 2015 }, false, forceLoad);

                forceLoad = false;

                Console.Write("Waiting...");
                for (int i = 0; i < 60; i++)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    Console.Write('.');
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey();
                        if (key.KeyChar == ' ')
                        {
                            forceLoad = true;
                            break;
                        }

                        return;
                    }
                }

                Console.WriteLine();
            }
        }
    }
}
