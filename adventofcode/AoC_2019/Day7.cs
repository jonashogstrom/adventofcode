using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

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
            Part1Solution = null;
            Part2Solution = null;
        }

        protected override void DoRun(string[] input)
        {
            Validate(43210, new List<int> {4, 3, 2, 1, 0}, "3,15,3,16,1002,16,10,16,1,16,15,15,4,15,99,0,0");
            Validate(54321, new List<int> { 0, 1, 2, 3, 4 }, "3,23,3,24,1002,24,10,24,1002,23,-1,23,101, 5, 23, 23, 1, 24, 23, 23, 4, 23, 99, 0, 0");
            Validate(65210, new List<int> { 1, 0, 4, 3, 2 }, "3,31,3,32,1002,32,10,32,1001,31,-2,31,1007,31,0,33,1002, 33, 7, 33, 1, 33, 31, 31, 1, 32, 31, 31, 4, 31, 99, 0, 0, 0");
            //var res = Compute2(input[0]);
            //Part1 = res.Item1;

            ValidateAsync(139629729, new List<int> { 9, 8, 7, 6, 5 }, "3,26,1001,26,-4,26,3,27,1002,27,2,27,1,27,26,27, 4, 27, 1001, 28, -1, 28, 1005, 28, 6, 99, 0, 0, 5");
            ValidateAsync(18216, new List<int> { 9, 7, 8, 5, 6 }, "3,52,1001,52,-5,52,3,53,1,52,56,54,1007,54,5,55,1005,55,26,1001,54, -5, 54, 1105, 1, 12, 1, 53, 54, 53, 1008, 54, 0, 55, 1001, 55, 1, 55, 2, 53, 55, 53, 4, 53, 1001, 56, -1, 56, 1005, 56, 6, 99, 0, 0, 0, 0, 10");

            var res = Compute2Async(input[0]);

            Part2 = res.Item1;

        }

        private void Validate(int expected, List<int> expPhaseSeq, string program)
        {
            Log("===");
            var act = Compute(expPhaseSeq, 0, program);
            if (act != expected)
                throw new Exception("broken...");
            Log("===");
            var res = Compute2(program);
            if (res.Item1 != expected)
                throw new Exception("unexpected result");
            if (!expPhaseSeq.SequenceEqual(res.Item2))
                throw new Exception("unexpected phase sequence");
            Log("===");
        }

        private void ValidateAsync(int expected, List<int> expPhaseSeq, string program)
        {
            Log("===");
//            var act = ComputeAsync(expPhaseSeq, 0, program);
//            if (act != expected)
//                throw new Exception("broken...");
//            Log("===");
            var res = Compute2Async(program);
            if (res.Item1 != expected)
                throw new Exception("unexpected result");
            if (!expPhaseSeq.SequenceEqual(res.Item2))
                throw new Exception("unexpected phase sequence");
            Log("===");
        }

        private (int, List<int>) Compute2(string program)
        {

            var maxThrusterSignal = int.MinValue;
            List<int> bestPhaseSetting = null;
            for (int i1 = 0; i1 <= 4; i1++)
                for (int i2 = 0; i2 <= 4; i2++)
                    for (int i3 = 0; i3 <= 4; i3++)
                        for (int i4 = 0; i4 <= 4; i4++)
                            for (int i5 = 0; i5 <= 4; i5++)
                            {

                                var phaseSettingSeq = new List<int>() { i1, i2, i3, i4, i5 };
                                var x = new HashSet<int>(phaseSettingSeq);
                                if (x.Count != 5)
                                    continue;


                                var ampA = new IntCodeComputer(new List<int>() { i1,0 }, program, -1, -1);
                                var ampB = new IntCodeComputer(new List<int>() { i2 }, program, -1, -1);
                                var ampC = new IntCodeComputer(new List<int>() { i3 }, program, -1, -1);
                                var ampD = new IntCodeComputer(new List<int>() { i4 }, program, -1, -1);
                                var ampE = new IntCodeComputer(new List<int>() { i5 }, program, -1, -1);


                                ampA.ConnectInpOutp(ampB);
                                ampB.ConnectInpOutp(ampC);
                                ampC.ConnectInpOutp(ampD);
                                ampD.ConnectInpOutp(ampE);
                                while (!ampE.Terminated)
                                {
                                    ampA.Execute();
                                    ampB.Execute();
                                    ampC.Execute();
                                    ampD.Execute();
                                    ampE.Execute();
                                }

                                if (ampE.LastOutput > maxThrusterSignal)
                                {
                                    maxThrusterSignal = ampE.LastOutput;
                                    bestPhaseSetting = phaseSettingSeq;
                                }

//
//
//                                var res = Compute(phaseSettingSeq, 0, program);
//
//                                if (res > maxThrusterSignal)
//                                {
//                                    maxThrusterSignal = res;
//                                    bestPhaseSetting = phaseSettingSeq;
//                                }

                            }

            return (maxThrusterSignal, bestPhaseSetting);
        }



        private int Compute(List<int> phaseSettingSeq, int value, string program)
        {
            if (phaseSettingSeq.Count == 0)
            {
                Log(value.ToString);
                return value;
            }
            var comp = new IntCodeComputer(new List<int>()
            {
                phaseSettingSeq.First(),
                value
            }, program, -1, -1);
            comp.RunProgram();
            var output = comp.LastOutput;
            var newPhaseSettingSeq = phaseSettingSeq.Skip(1).ToList();
            return Compute(newPhaseSettingSeq, output, program);
        }


//        private int ComputeAsync(List<int> phaseSettingSeq, int value, string program)
//        {
//            if (phaseSettingSeq.Count == 0)
//            {
//                Log(value.ToString);
//                return value;
//            }
//            var comp = new IntCodeComputer(new List<int>()
//            {
//                phaseSettingSeq.First(),
//                value
//            }, program, -1, -1);
//            comp.RunProgram();
//            var output = comp.LastOutput;
//            var newPhaseSettingSeq = phaseSettingSeq.Skip(1).ToList();
//            return Compute(newPhaseSettingSeq, output, program);
//        }

        private (int, List<int>) Compute2Async(string program)
        {

            var maxThrusterSignal = int.MinValue;
            List<int> bestPhaseSetting = null;
            for (int i1 = 5; i1 <= 9; i1++)
                for (int i2 = 5; i2 <= 9; i2++)
                    for (int i3 = 5; i3 <= 9; i3++)
                        for (int i4 = 5; i4 <= 9; i4++)
                            for (int i5 = 5; i5 <= 9; i5++)
                            {

                                var phaseSettingSeq = new List<int>() { i1, i2, i3, i4, i5 };
                                var x = new HashSet<int>(phaseSettingSeq);
                                if (x.Count != 5)
                                    continue;
                                var ampA = new IntCodeComputer(new List<int>() { i1,0 }, program, -1, -1);
                                var ampB = new IntCodeComputer(new List<int>() { i2 }, program, -1, -1);
                                var ampC = new IntCodeComputer(new List<int>() { i3 }, program, -1, -1);
                                var ampD = new IntCodeComputer(new List<int>() { i4 }, program, -1, -1);
                                var ampE = new IntCodeComputer(new List<int>() { i5 }, program, -1, -1);


                                ampA.ConnectInpOutp(ampB);
                                ampB.ConnectInpOutp(ampC);
                                ampC.ConnectInpOutp(ampD);
                                ampD.ConnectInpOutp(ampE);
                                ampE.ConnectInpOutp(ampA);
                                while (!ampE.Terminated)
                                {
                                    ampA.Execute();
                                    ampB.Execute();
                                    ampC.Execute();
                                    ampD.Execute();
                                    ampE.Execute();
                                }

                                if (ampE.LastOutput > maxThrusterSignal)
                                {
                                    maxThrusterSignal = ampE.LastOutput;
                                    bestPhaseSetting = phaseSettingSeq;
                                }

                            }

            return (maxThrusterSignal, bestPhaseSetting);
        }

    }


}