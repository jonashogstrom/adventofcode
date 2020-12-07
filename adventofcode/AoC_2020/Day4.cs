using System;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2020
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day4 : TestBaseClass<Part1Type, Part2Type>
    {
        [Test]
        [TestCase(4, 0, "Day4_test2_invalid.txt")]
        [TestCase(4, 4, "Day4_test2_valid.txt")]
        [TestCase(2, 2, "Day4_test.txt")]
        [TestCase(182, 109, "Day4.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            var part1 = 0;
            var part2 = 0;

            foreach (var passport in Passport.ParseInputToPassports(source))
            {
                if (passport.HasMandatoryFields)
                    part1++;
                if (passport.IsValid)
                    part2++;
                else
                {
                    foreach(var e in passport.ValidationErrors)
                        Log(()=>e, 5);

                }
            }
            return (part1, part2);
        }
    }
}