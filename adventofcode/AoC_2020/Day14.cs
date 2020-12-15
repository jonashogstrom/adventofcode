using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using NUnit.Framework;

namespace AdventofCode.AoC_2020
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    // not 319487199375

    [TestFixture]
    class Day14 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }
        private bool _skipPart1;
        private bool _skipPart2;

        [Test]
        [TestCase(165, null, "Day14_test.txt")]
        [TestCase(null, 208, "Day14_test2.txt")]
        [TestCase(17765746710228, 4401465949086, "Day14.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            _skipPart1 = exp1.HasValue;
            _skipPart2 = exp2.HasValue;

            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            var part1 = 0L;
            var part2 = 0L;

            var sw = Stopwatch.StartNew();

            LogAndReset("Parse", sw);
            if (!_skipPart1)
            {
                var mem = ExecutePart1(source);

                part1 = mem.Values.Sum();
            }

            LogAndReset("*1", sw);
            if (!_skipPart2)
            {
                var mem = ExecutePart2(source);

                part2 = mem.Values.Sum();
            }

            LogAndReset("*2", sw);
            return (part1, part2);
        }

        private Dictionary<long, long> ExecutePart2(string[] source)
        {
            string mask = "";
            var mem = new Dictionary<long, long>();
            foreach (var s in source)
            {
                var parts = s.Split('=');
                var instr = parts[0].Trim();
                var arg = parts[1].Trim();
                if (instr == "mask")
                {
                    mask = arg;
                }
                else
                {
                    var addr = long.Parse(instr.Split('[', ']')[1]);
                    var value = long.Parse(arg);
                    var temp =  ApplyMask2(addr, mask);
                    foreach(var a in temp)
                        mem[a] = value;
                }
            }

            return mem;
        }

        private IEnumerable<long> ApplyMask2(long addr, string mask)
        {
            var res = new List<long> {addr};
            for (int i = 0; i < mask.Length; i++)
            {
                var bit = (1L << 35 - i);

                var m = mask[i];
                if (m == '1')
                {
                    for (int p = 0; p < res.Count; p++)
                        res[p] = res[p] | bit;
                }
                else if (m == 'X')
                {
                    var tempRes = new List<long>();
                    var count = res.Count;
                    for (int p = 0; p < count; p++)
                    {
                        var temp = res[p];
                        res[p] = temp | bit;
                        res.Add(res[p]-bit);
                    }
                }
            }
            return res;
        }

        private Dictionary<long, long> ExecutePart1(string[] source)
        {
            string mask = "";
            var mem = new Dictionary<long, long>();
            foreach (var s in source)
            {
                var parts = s.Split('=');
                var instr = parts[0].Trim();
                var arg = parts[1].Trim();
                if (instr == "mask")
                {
                    mask = arg;
                }
                else
                {
                    var addr = long.Parse(instr.Split('[', ']')[1]);
                    var value = long.Parse(arg);
                    value = ApplyMask(value, mask);
                    mem[addr] = value;
                }
            }

            return mem;
        }

        private long ApplyMask(long value, string mask)
        {
            var res = value;
            for (int i = 0; i < mask.Length; i++)
            {
                var bit = (1L << 35 - i);

                if (mask[i] == '1')
                {
                    res = res | bit;
                }
                else if (mask[i] == '0')
                {
                    if (((res & bit) == bit))
                    {
                        res -= bit;
                    }
                }
            }

            return res;
        }
    }
}
