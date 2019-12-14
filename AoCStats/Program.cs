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
                new LeaderboardParser().GenerateReport(371692, new[] { 2019, 2018 }, false, false);

                new LeaderboardParser().GenerateReport(139489, new[] { 2019, 2018 }, true, forceLoad);
                new LeaderboardParser().GenerateReport(139489, new[] { 2019, 2018 }, false, false);

                // Leica
                 new LeaderboardParser().GenerateReport(373164, new[] { 2019, 2018 }, true, forceLoad);
                 new LeaderboardParser().GenerateReport(373164, new[] { 2019, 2018 }, false, false);

                // kodaporna
                new LeaderboardParser().GenerateReport(395782, new[] { 2019, 2018, 2017, 2016, 2015 }, true, forceLoad);
                new LeaderboardParser().GenerateReport(395782, new[] { 2019, 2018, 2017, 2016, 2015 }, false, false);

                // jeppes lista
                new LeaderboardParser().GenerateReport(34481, new[] { 2019, 2018, 2017, 2016, 2015 }, true, forceLoad);
                new LeaderboardParser().GenerateReport(34481, new[] { 2019, 2018, 2017, 2016, 2015 }, false, false);

                forceLoad = false;

                Console.Write("Waiting... Press Space to forceLoad, any other key to quit");
                var ts = DateTime.Now;
                while ((DateTime.Now-ts).TotalSeconds < 60)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(0.1));
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
