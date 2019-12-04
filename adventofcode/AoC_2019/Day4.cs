using System;
using System.Linq;

namespace AdventofCode.AoC_2019
{
    class Day4 : BaseDay
    {
        protected override void Setup()
        {
            Source = InputSource.test;
            Source = InputSource.prod;

            LogLevel = UseTestData ? 5 : 0;

            Part1TestSolution = null;
            Part2TestSolution = null;
            Part1Solution = 1686;
            Part2Solution = 1145;
        }

        protected override void DoRun(string[] input)
        {
            // not 1240
            var low = 168630;
            var high = 718098;
            // jespers input
            // low = 108457;
            // high = 562041;
            var matching1 = 0;
            var matching2 = 0;
            TestNum(111111, (p1: true, p2: false));
            TestNum(112233, (p1: true, p2: true));
            TestNum(123444, (p1: true, p2: false));
            TestNum(111122, (p1: true, p2: true));
            TestNum(168630, (p1: false, p2: false));
            TestNum(718098, (p1: false, p2: false));
            TestNum(123789, (p1: false, p2: false));
            TestNum(223450, (p1: false, p2: false));
            TestNum(555789, (p1: true, p2: false));

            for (var n = low; n <= high; n++)
            {
                var res = CheckNumber(n);
                if (res.p1)
                    matching1++;
                if (res.p2)
                    matching2++;
            }

            Part1 = matching1;
            Part2 = matching2;
        }

        private void TestNum(int i, (bool p1, bool p2) b)
        {
            var res = CheckNumber(i);
            Log(i + ": " + (res) + " " + (res.p1 == b.p1 ? "OK1" : "FAIL1") + " " + (res.p2 == b.p2 ? "OK2" : "FAIL2"));
            if (res != b)
                throw new Exception("regression error");
        }

        private (bool p1, bool p2) CheckNumber(int n)
        {
            var s = n.ToString();
            var x = s.groupSizes();
            var growing = true;
            for (var p = 1; p < s.Length; p++)
            {
                if (s[p] < s[p - 1])
                    growing = false;
            }

            if (!growing)
                return (false, false);

            var g = s.GroupBy(c => c);
            var identical = g.Any(l => l.Count() >= 2);
            var identical2 = g.Any(l => l.Count() == 2);

            return (p1: identical, p2: identical2);
        }

    }
}