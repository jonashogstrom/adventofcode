using System.Collections.Generic;
using System.Linq;

namespace adventofcode
{
    internal class Day14 : BaseDay
    {
        protected override void Setup()
        {
            UseTestData = false;

            Part1TestSolution = null;
            Part2TestSolution = null;
            Part1Solution = "1474315445";
            Part2Solution = 20278122;
        }

        protected override void DoRun(string[] input)
        {
            if (UseTestData)
            {
                GenerateRecepies(9, "5158916779", null);
                GenerateRecepies(51589, null, 9);
                GenerateRecepies(01245, null, null);
                GenerateRecepies(92510, null, 18);

                GenerateRecepies(59414, null, 2018); // the one that is validated
            }
            else
            {
                GenerateRecepies(540391, "1474315445", 20278122);
            }
        }

        private void GenerateRecepies(int recCount, string expPart1, int? expPart2)
        {
            var recepies = new List<byte>(20000000) { 3, 7 };
            var elf1 = 0;
            var elf2 = 1;
            var s2 = recCount.ToString().ToCharArray().Select(c => byte.Parse(c.ToString())).ToArray();
            Part2 = null;
            Part1 = null;
            while (Part1 == null || Part2 == null)
            {
                var newrec = (byte)(recepies[elf1] + recepies[elf2]);
                if (newrec < 10)
                    recepies.Add(newrec);
                else
                {
                    recepies.Add(1);
                    CheckForMatch(recCount, recepies, s2, expPart2);
                    recepies.Add((byte)(newrec % 10));
                }
                CheckForMatch(recCount, recepies, s2, expPart2);


                elf1 = (elf1 + recepies[elf1] + 1) % recepies.Count;
                elf2 = (elf2 + recepies[elf2] + 1) % recepies.Count;

                if (Part1 == null && recepies.Count >= recCount + 10)
                {
                    var res = "";
                    for (int i = 0; i < 10; i++)
                        res = res + recepies[recCount + i];
                    Part1 = res;
                    var valid = expPart1 != null ? (expPart1.Equals(res) ? "OK" : "FAIL!! Expected: " + expPart1) : "";
                    Log($"Part1 ({recCount}): {res} {valid}");
                }
            }
        }

        private void CheckForMatch(int recCount, List<byte> recepies, byte[] s2, int? expPart2)
        {
            if (Part2 != null)
                return;
            if (recepies.Count > s2.Length)
            {
                var pos = recepies.Count - s2.Length;
                for (int i = 0; i < s2.Length; i++)
                {
                    if (recepies[pos + i] != s2[i])
                        return;
                }

                var valid = expPart2 != null ? (expPart2.Value.Equals(pos) ? "OK" : "FAIL!! Expected: " + expPart2) : "";
                Log($"Part2 ({recCount}): {pos} {valid}");
//                foreach (var x in recepies.GroupBy(x => x).OrderBy(x => x.Key))
//                    Log($"{x.Key}: {x.Count()} ({x.Count() * 100.0 / recepies.Count:F2}%)");
                Part2 = pos;
            }
        }
    }
}