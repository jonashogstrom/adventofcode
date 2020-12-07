using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2020
{
    using Part1Type = Int32;
    using Part2Type = Int32;

    [TestFixture]
    class Day2 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(2, 1, "Day2_test.txt")]
        [TestCase(636, 588, "Day2.txt")]
        public void Test1(Part1Type exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            var passwords = source.Select(s => new Pwd(s)).ToArray();
            var part1 = passwords.Count(p => p.Valid1);
            var part2 = passwords.Count(p => p.Valid2);
            return (part1, part2);
        }
    }

    internal class Pwd
    {
        public Pwd(string s)
        {
            var parts = s.Split(new[] { '-', ' ', ':' }, StringSplitOptions.RemoveEmptyEntries);
            Low = int.Parse(parts[0]);
            High = int.Parse(parts[1]);
            Ch = parts[2][0];
            Password = parts[3];
        }

        public int Low { get; }

        public int High { get; }

        public char Ch { get; }

        public string Password { get; }

        public bool Valid1
        {
            get
            {
                var count = 0;
                foreach (var c in Password)
                    if (c == Ch)
                        count++;
                return count >= Low && count <= High;
            }
        }

        public bool Valid2 => Password[Low - 1] == Ch ^ Password[High - 1] == Ch;
    }
}

