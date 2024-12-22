using System;
using System.Diagnostics;
using NUnit.Framework;
using System.Collections.Generic;
using AdventofCode.Utils;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic;


namespace AdventofCode.AoC_2024
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    // 116742 is too high
    [TestFixture]
    class Day21 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        internal static readonly string[] keypadLayout = new string[]
        {
            "x^A",
            "<v>",
        };

        internal static readonly string[] numpadLayout = new string[]
        {
            "789",
            "456",
            "123",
            "x0A"
        };

        [Test]
        [TestCase(126384, null, "Day21_test.txt")]
        [TestCase(9999999, null, "Day21.txt")]
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


            LogAndReset("Parse", sw);

            var numpad = new Pad(numpadLayout.ToSparseBuffer(), ">^<v");
            var keypad = new Pad(keypadLayout.ToSparseBuffer(), "v><^");

            foreach (var line in source)
            {
                var p1 = Control(line, numpad);
                var p2 = Control(p1, keypad);
                var p3 = Control(p2, keypad);
                var len = p3.Length;
                Log("============");
                Log(p3);
                Log(p2);
                Log(p1);
                var numpart = int.Parse(line[..3]);
                var complexity = len * numpart;
                Log($"{line}: {len} * {numpart} = {complexity}");
                part1 += complexity;
            }

            LogAndReset("*1", sw);

            // solve part 2 here

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private string Control(string sequence, Pad pad)
        {
            var keypad = pad.Keypad;
            var init = keypad.Keys.Single(k => keypad[k] == 'A');
            var res = new StringBuilder();
            var pos = init;
            //var xLoc = keypad.Keys.Single(k => keypad[k] == 'x');
            foreach (var c in sequence)
            {
                var nextPos = keypad.Keys.Single(k => keypad[k] == c);
                var path = pad.FindSafePath(pos, nextPos).ToArray();

                foreach (var p in path)
                {
                    var dir = p.Subtract(pos);
                    res.Append(Coord.trans2Arrow[dir]);
                    pos = p;
                }

                res.Append('A');
            }

            return res.ToString();
        }

        public class Pad(SparseBuffer<char> pad, string order)
        {
            public SparseBuffer<char> Keypad => pad;
            public string dirOrder => order;

            public IEnumerable<Coord> FindSafePath(Coord start, Coord end)
            {
                var p = start;
                var comparisonDic = new Dictionary<char, Func<Coord, Coord, bool>>
                {
                    ['>'] = (x, y) => x.X < y.X,
                    ['<'] = (x, y) => x.X > y.X,
                    ['^'] = (x, y) => x.Y > y.Y,
                    ['v'] = (x, y) => x.Y < y.Y
                };

                foreach (var d in dirOrder)
                {
                    var dir = Coord.trans2Coord[d];
                    while (comparisonDic[d](p, end))
                    {
                        var next = p.Move(dir);
                        yield return next;
                        p = next;
                    }
                }
            }
        }
    }
}