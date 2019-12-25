using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2019
{
    using Part1Type = Int32;
    using Part2Type = Int32;

    [TestFixture]
    class Day25 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(-1, null, "Day25.txt")]
        public void Test1(Part1Type exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var instructions = GetResource("Day25_test.txt");
            var res = Compute(source, instructions);
            DoAsserts(res, exp1, exp2);
        }

        private (int Part1, int Part2) Compute(string[] source, string[] instructions)
        {
            var comp = new IntCodeComputer(source[0]);

            ReadInput(comp, "Day25_test.txt");
            var part1 = (int)comp.LastOutput;
            return (part1, 0);
        }

        private long WriteOutput(IntCodeComputer comp)
        {
            var x = "";
            while (comp.OutputQ.Any())
            {
                x += (char)comp.OutputQ.Dequeue();
            }

            var damage = comp.LastOutput;
            Log(x);
            return damage;
        }

        private void ReadInput(IntCodeComputer comp, string programName)
        {
            var program = GetResource(programName);
            foreach (var line in program)
            {
                AddLine(comp, line);
                comp.Execute();
                WriteOutput(comp);
            }
            AddLine(comp, "inv");
            comp.Execute();
            WriteOutput(comp);


        }

        private void AddLine(IntCodeComputer comp, string line)
        {
            var l = line.Trim().Split('/').First().Trim();
            var input = new List<long>();
            if (l != "")
            {
                Log("Move: " + line);
                input.AddRange(FormatInput(l.Trim()));
            }

            foreach (var i in input)
                comp.AddInput(i);
        }

        private IEnumerable<long> FormatInput(string seq)
        {
            for (var i = 0; i < seq.Length; i++)
            {
                yield return seq[i];
            }
            yield return 10;
        }
    }
}