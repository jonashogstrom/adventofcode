using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2020
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day4 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

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
            var passport = new Dictionary<string, string>();
            var input = source.Append("").ToArray();
            foreach (var x in input)
            {
                if (string.IsNullOrEmpty(x))
                {
                    if (ValidatePassport(passport))
                        part1++;
                    if (ValidatePassport2(passport))
                        part2++;
                    passport = new Dictionary<string, string>();
                }
                else
                {
                    foreach (var p in x.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var p2 = p.Split(':');
                        passport[p2[0]] = p2[1];
                    }
                }

            }
            return (part1, part2);
        }

        private bool ValidatePassport(Dictionary<string, string> passport)
        {
            var mandatory = new[] { "byr", "iyr", "eyr", "hgt", "hcl", "ecl", "pid" };
            foreach (var x in mandatory)
                if (!passport.ContainsKey(x))
                    return false;
            return true;
        }
        private bool ValidatePassport2(Dictionary<string, string> passport)
        {
            if (!ValidatePassport(passport))
                return false;
            if (!ValidateRange(passport, "byr", 1920, 2002))
                return false;
            if (!ValidateRange(passport, "iyr", 2010, 2020))
                return false;
            if (!ValidateRange(passport, "eyr", 2020, 2030))
                return false;

            if (passport["hgt"].EndsWith("cm"))
            {
                if (!ValidateRange(passport, "hgt", 150, 193, "cm"))
                    return false;
            }
            else if (passport["hgt"].EndsWith("in"))
            {
                if (!ValidateRange(passport, "hgt", 59, 76, "in"))
                    return false;
            }
            else
            {
                return false;
            }

            var haircolor = passport["hcl"];
            if (haircolor.Length != 7 || haircolor[0] != '#')
                return false;
            var hex = "0123456789abcdef";
            for (int i = 1; i < 7; i++)
                if (!hex.Contains(haircolor[i].ToString()))
                    return false;

            var validHairColors = new HashSet<string>()
            {
                "amb", "blu", "brn", "gry", "grn", "hzl", "oth"
            };
            if (!validHairColors.Contains(passport["ecl"]))
                return false;


            var pid = passport["pid"];
            if (pid.Length != 9)
                return false;
            if (!int.TryParse(pid, out var pidValue))
                return false;
            return true;


        }




        private bool ValidateRange(Dictionary<string, string> passport, string code, int min, int max, string suffix = null)
        {
            var strValue = passport[code];
            if (suffix != null)
                strValue = strValue.Replace(suffix, "");
            var value = int.Parse(strValue);
            if (value < min || value > max)
                return false;
            return true;
        }
    }
}