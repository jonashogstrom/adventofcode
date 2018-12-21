using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoCStats
{
    class Program
    {
        static void Main(string[] args)
        {
            new LeaderboardParser().GenerateReport(371692, 2018);
            new LeaderboardParser().GenerateReport(373164, 2018);
            new LeaderboardParser().GenerateReport(395782, 2018);
            new LeaderboardParser().GenerateReport(34481, new[] { 2018, 2017, 2016, 2015 });
        }
    }
}
