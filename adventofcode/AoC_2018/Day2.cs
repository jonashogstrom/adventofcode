using System.Linq;

namespace adventofcode.AoC_2018
{
    public class Day2 : BaseDay
    {
        protected override void Setup()
        {
            Part1Solution = 7936;
            Part2Solution = "lnfqdscwjyteorambzuchrgpx";
        }

        protected override void DoRun(string[] input)
        {
            var groupsWith3Counter = 0;
            var groupsWith2Counter = 0;
            foreach (var id in input)
            {
                var chars = id.ToCharArray().GroupBy(x => x);
                var has3 = chars.Any(group => group.Count() == 3);
                var has2 = chars.Any(group => group.Count() == 2);
                if (has3) groupsWith3Counter++;
                if (has2) groupsWith2Counter++;
            }
            Log("3: " + groupsWith3Counter);
            Log("2: " + groupsWith2Counter);
            Part1 = groupsWith2Counter * groupsWith3Counter;
            foreach (var id1 in input)
                foreach (var id2 in input)
                    Compareids(id1, id2);
        }

        private void Compareids(string id1, string id2)
        {
            var errorCount = 0;
            var errorpos = -1;
            for (int i = 0; i < id1.Length; i++)
                if (id1[i] != id2[i])
                {
                    errorCount++;
                    errorpos = i;
                }

            if (errorCount == 1)
            {
                Part2 = id1.Remove(errorpos, 1);
                Log(id1 + "  " + id2 + "=>" + Part2);
            }
        }
    }
}