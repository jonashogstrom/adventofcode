using System;
using System.Diagnostics;
using NUnit.Framework;
using System.Collections.Generic;
using AdventofCode.Utils;
using System.Linq;
using System.Text;


namespace AdventofCode.AoC_2024
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    // 6234514521086 is too low
    // 6237075041489
    [TestFixture]
    class Day09 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(1928, 2858, "Day09_test.txt")]
        [TestCase(6216544403458, null, "Day09.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            LogLevel = resourceName.Contains("test") ? 20 : -1;
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1 = 0;
            Part2Type part2 = 0;
            var sw = Stopwatch.StartNew();

            var disk = new List<int?>();
            var state = 1;
            var file = 0;

            foreach (var d in source.First().Select(c => int.Parse(c.ToString())))
            {
                for (var i = 0; i < d; i++)
                    disk.Add(state == 1 ? file : null);
                if (state == 1)
                    file++;
                state = 1 - state;
            }

            var emptyPos = disk.IndexOf(null, 0);
            for (var i = disk.Count - 1; i > emptyPos; i--)
            {
                if (disk[i] != null)
                {
                    disk[emptyPos] = disk[i];
                    disk[i] = null;
                    emptyPos = disk.IndexOf(null, emptyPos + 1);
                }
            }

            for (var i = 0; i < disk.Count; i++)
                part1 += disk[i].HasValue ? disk[i].Value * i : 0;

            LogAndReset("Parse", sw);

            // solve part 1 here

            LogAndReset("*1", sw);


            /*
00...111...2...333.44.5555.6666.777.888899
0099.111...2...333.44.5555.6666.777.8888..
0099.1117772...333.44.5555.6666.....8888..
0099.111777244.333....5555.6666.....8888..
00992111777.44.333....5555.6666.....8888..

0099211177744333555566668888
             */
            // 2333133121414131402
            var str = source.First().Select(c => int.Parse(c.ToString())).ToList();
            var disk2 = new LinkedList<(int len, int val)>();
            for (int i = 0; i < str.Count; i++)
            {
                var valueTuple = (str[i], i % 2 == 0 ? i / 2 : -1);
                disk2.AddLast(valueTuple);
            }

            var nodeCount = disk2.Count;

            var f = disk2.Last;
            var step = 0;
            while (f.Previous != null)
            {
                if (step == 4518)
                {
                    Debugger.Break();
                }

                var hole = disk2.First.Next;
                while (hole != null && hole.Value.len < f.Value.len && hole.Previous != f && hole.Next != null)
                    hole = hole.Next.Next;

                if (hole != null && hole.Next == f)
                {
                    var holeLength = hole.Value.len;
                    hole.Value = hole.Value with { len = 0 };
                    if (f.Next != null)
                    {
                        f.Next.Value = f.Next.Value with { len = f.Next.Value.len + holeLength - f.Value.len };
                    }
                }
                else if (hole != null && hole.Previous != f && hole.Next != null)
                {
                    Log($"Found a hole for file {f.Value.val} ({f.Value.len} long and hole is {hole.Value.len})");
                    var tempf = f.Previous.Previous;
                    disk2.Remove(f);
                    var newHole = new LinkedListNode<(int, int)>((0, -1));
                    disk2.AddAfter(hole.Previous, newHole);
                    disk2.AddAfter(newHole, f);
                    hole.Value = hole.Value with { len = hole.Value.len - f.Value.len };
                    if (tempf.Next.Next != null)
                    {
                        tempf.Next.Value = tempf.Next.Value with
                        {
                            len = tempf.Next.Value.len + tempf.Next.Next.Value.len + f.Value.len
                        };
                        disk2.Remove(tempf.Next.Next);
                    }
                    else
                        disk2.Remove(tempf.Next);

                    f = tempf;
                }
                else
                    f = f.Previous.Previous;

                if (disk2.Count != nodeCount)
                    throw new Exception();
                if (disk2.Last().val == -1)
                    throw new Exception();
                step++;
            }

            state = 1;
            var pos = 0;
            f = disk2.First;
            var str2 = new StringBuilder();
            var nodeIndex = 0;
            while (f != null)
            {
                if (state == 1 && f.Value.val == -1)
                    throw new Exception();
                if (state == 0 && f.Value.val != -1)
                    throw new Exception();

                for (int i = 0; i < f.Value.len; i++)
                {
                    if (state == 1)
                    {
                        part2 += f.Value.val * pos;
                        str2.Append(f.Value.val.ToString());
                    }
                    else
                    {
                        str2.Append('.');
                    }

                    pos++;
                }

                state = 1 - state;
                f = f.Next;
                nodeIndex++;
            }

            Log(str2.ToString());
            LogAndReset("*2", sw);

            return (part1, part2);
        }
    }
}