using System;
using System.Diagnostics;
using System.Security;
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
            DoAsserts(res, exp1, exp2);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            var sw = Stopwatch.StartNew();

            var c = new Comp(source);
            LogAndReset("Parse", sw);
            var part1 = c.Execute(true);
            Log("*1 - lastJump: " + part1.LastJump);

            LogAndReset("*1", sw);
            ExecutionState part2 = null;

            for (int i = 0; i < source.Length; i++)
            {
                if (c.Program[i].OpCode == OpCodex.nop)
                    c.Overrides[i] = c.Program[i].Clone(OpCodex.jmp);
                else if (c.Program[i].OpCode == OpCodex.jmp)
                    c.Overrides[i] = c.Program[i].Clone(OpCodex.nop);
                else continue;

                var result = c.Execute(true);
                c.Overrides.Clear();
                if (!result.ReExecuted)
                {
                    part2 = result;
                    Log("*2 - Modified op: " + i);
                    break;
                }
            }

            LogAndReset("*2", sw);
            return (part1.Accumulator, part2.Accumulator);
        }

    }


}