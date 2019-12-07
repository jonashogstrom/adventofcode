using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2019
{
    class Day7 : BaseDay
    {
        protected override void Setup()
        {
            Source = InputSource.test;
            Source = InputSource.prod;

            LogLevel = UseTestData ? 5 : 0;

            Part1TestSolution = null;
            Part2TestSolution = null;
            Part1Solution = 38834;
            Part2Solution = 69113332;
        }

        protected override void DoRun(string[] input)
        {
            //var test = Permutations(new[] { 1, 2 }).ToArray();
            var s1Phases = new[] { 0, 1, 2, 3, 4 };
            Validate(43210, new[] { 4, 3, 2, 1, 0 }, s1Phases, false, "3,15,3,16,1002,16,10,16,1,16,15,15,4,15,99,0,0");
            Validate(54321, new[] { 0, 1, 2, 3, 4 }, s1Phases, false, "3,23,3,24,1002,24,10,24,1002,23,-1,23,101, 5, 23, 23, 1, 24, 23, 23, 4, 23, 99, 0, 0");
            Validate(65210, new[] { 1, 0, 4, 3, 2 }, s1Phases, false, "3,31,3,32,1002,32,10,32,1001,31,-2,31,1007,31,0,33,1002, 33, 7, 33, 1, 33, 31, 31, 1, 32, 31, 31, 4, 31, 99, 0, 0, 0");

            var res = Compute2(input[0], s1Phases, false);
            Part1 = res.Item1;

            var s2Phases = new[] { 5, 6, 7, 8, 9 };

            Validate(139629729, new[] { 9, 8, 7, 6, 5 }, s2Phases, true, "3,26,1001,26,-4,26,3,27,1002,27,2,27,1,27,26,27, 4, 27, 1001, 28, -1, 28, 1005, 28, 6, 99, 0, 0, 5");
            Validate(18216, new[] { 9, 7, 8, 5, 6 }, s2Phases, true, "3,52,1001,52,-5,52,3,53,1,52,56,54,1007,54,5,55,1005,55,26,1001,54, -5, 54, 1105, 1, 12, 1, 53, 54, 53, 1008, 54, 0, 55, 1001, 55, 1, 55, 2, 53, 55, 53, 4, 53, 1001, 56, -1, 56, 1005, 56, 6, 99, 0, 0, 0, 0, 10");

            res = Compute2(input[0], s2Phases, true);

            Part2 = res.Item1;

        }

        private void Validate(int expected, IEnumerable<int> expPhaseSeq, IEnumerable<int> phases, bool feedback, string program)
        {
            var res = Compute2(program, phases, feedback);
            if (res.Item1 != expected)
                Log($"Result ERROR!!! Exp: {expected} Act: {res.Item1}");
            if (!expPhaseSeq.SequenceEqual(res.Item2))
                Log($"Sequence ERROR!!! Exp: {expPhaseSeq} Act: {res.Item2}");
        }


        private (int, IEnumerable<int>) Compute2(string program, IEnumerable<int> phases, bool feedback)
        {
            var maxThrusterSignal = int.MinValue;
            int[] bestPhaseSetting = null;

            var computerCount = 5;
            var p = Permutations(phases).Select(x => x.ToArray()).ToArray();
            foreach (var phaseSequence in p)
            {
                var computers = new IntCodeComputer[computerCount];
                for (int i = 0; i < computerCount; i++)
                {
                    computers[i] = new IntCodeComputer(new List<int> { phaseSequence[i] }, program);
                    computers[i].Name = "Amp" + (char)(i + 65);
                }
                computers.First().AddInput(0);
                for (int i = 0; i < computerCount - 1; i++)
                    computers[i].ConnectInpOutp(computers[i + 1]);

                if (feedback)
                    computers.Last().ConnectInpOutp(computers.First());

                while (!computers.Last().Terminated)
                {
                    foreach (var c in computers)
                        c.Execute();
                }

                if (computers.Last().LastOutput > maxThrusterSignal)
                {
                    maxThrusterSignal = computers.Last().LastOutput;
                    bestPhaseSetting = phaseSequence;
                }
            }

            return (maxThrusterSignal, bestPhaseSetting);
        }

        private IEnumerable<IEnumerable<int>> Permutations(IEnumerable<int> phases)
        {
            Assert.AreEqual(phases.Count(), phases.Distinct().Count());
            foreach (var i in phases)
            {
                var rest = Permutations(phases.Where(x => x != i).ToArray()).ToArray();
                if (rest.Length > 0)
                {
                    foreach (var part in rest)
                    {
                        yield return part.Prepend(i).ToArray();
                    }
                }
                else
                {
                    yield return new[] { i };
                }
            }
        }
    }
}