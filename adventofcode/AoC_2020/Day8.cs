using System;
using System.Diagnostics;
using NUnit.Framework;

namespace AdventofCode.AoC_2020
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day8 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(5, 8, "Day8_test.txt")]
        [TestCase(1446, 1403, "Day8.txt")]
        [Repeat(5)]
        public void Test1(Part1Type exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            var sw = Stopwatch.StartNew();

            var c = new Comp(source);
            LogAndReset("Parse", sw);
            var part1 = c.Execute(true);

            LogAndReset("*1", sw);
            ExecutionState part2 = null;

            for (int i = 0; i < c.Program.Count; i++)
            {
                if (part1.Instructions[i] == 0)
                    continue;
                var op = c.Program[i];
                if (op.OpCode == OpCodex.nop)
                    c.Overrides[i] = op.Clone(OpCodex.jmp);
                else if (op.OpCode == OpCodex.jmp)
                    c.Overrides[i] = op.Clone(OpCodex.nop);
                else continue;

                var result = c.Execute(true);
                c.Overrides[i] = null;
                if (result.Terminated)
                {
                    part2 = result;
                    break;
                }
            }

            LogAndReset("*2", sw);
            return (part1.Accumulator, part2.Accumulator);
        }

    }


}