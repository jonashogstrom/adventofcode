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
                new LeaderboardParser().GenerateReport(371692, new[] { 2020, 2019, 2018 }, true, forceLoad);

                // jonnes lista
                new LeaderboardParser().GenerateReport(139489, new[] { 2020, 2019, 2018 }, false, forceLoad);

                // kodaporna
                new LeaderboardParser().GenerateReport(395782, new[] { 2020, 2019, 2018, 2017, 2016, 2015 }, false, forceLoad);

                // jeppes lista
                new LeaderboardParser().GenerateReport(34481, new[] { 2020 }, false, forceLoad);

                // jeppes andra lista
                new LeaderboardParser().GenerateReport(967416, new[] { 2020 }, false, forceLoad);

                // Leica
                new LeaderboardParser().GenerateReport(373164, new[] { 2020, 2019, 2018 }, false, forceLoad);

                // FredrikMarcus
                new LeaderboardParser().GenerateReport(126234, new[] { 2020, 2019, 2018 }, false, forceLoad);

                // repl.it
                new LeaderboardParser().GenerateReport(374141, new[] { 2020 }, false, forceLoad);

                // catawiki
                new LeaderboardParser().GenerateReport(190418, new[] { 2020 }, false, forceLoad);

                // smartystreet
                new LeaderboardParser().GenerateReport(193188, new[] { 2020 }, false, forceLoad);

                // David Lozzi
                new LeaderboardParser().GenerateReport(1030369, new[] { 2020 }, false, forceLoad);

                // Linus
                new LeaderboardParser().GenerateReport(382101, new[] { 2020 }, false, forceLoad);


                // tobii with all participants
                //new LeaderboardParser().GenerateReport(371692, new[] { 2020, 2019, 2018 }, false, false);

                forceLoad = false;

                Console.WriteLine("Waiting... Press Space to forceLoad, any other key to quit");
                var ts = DateTime.Now;
                int i = 0;
                while ((DateTime.Now - ts).TotalSeconds < 60)
                {
                    if (i++ % 20 == 0)
                        Console.Write('.');
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
