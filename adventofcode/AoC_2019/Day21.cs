using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using NUnit.Framework;

namespace AdventofCode.AoC_2019
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day21 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        
        [TestCase(19355436, 1142618405, "Day21.txt")]
        public void Test1(Part1Type exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = Compute(source);
            DoAsserts(res, exp1, exp2);
        }

        private (Part1Type Part1, Part2Type Part2) Compute(string[] source)
        {
            var comp1 = new IntCodeComputer(source[0]);
            ReadInput(comp1, "Day21program.txt");
            comp1.Execute();
            var damage1 = WriteOutput(comp1);


            var comp = new IntCodeComputer(source[0]);
            ReadInput(comp, "Day21program2.txt");
            comp.Execute();
            var damage2 = WriteOutput(comp);

            return (damage1, damage2);

            // var part1 = (int)comp.LastOutput;
            // return (part1, 0);
        }

        private long WriteOutput(IntCodeComputer comp)
        {
            var x = "";
            while (comp.OutputQ.Any())
            {
                x += (char) comp.OutputQ.Dequeue();
            }

            var damage = comp.LastOutput;
            Log(x);
            return damage;
        }

        private void ReadInput(IntCodeComputer comp, string programName)
        {
            var input = new List<long>();
            var program = GetResource(programName);
            foreach (var line in program)
            {
                var l = line.Trim().Split('/').First().Trim().ToUpper();
                if (l != "")
                {
                    Log(l);
                    input.AddRange(FormatInput(l));
                }
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