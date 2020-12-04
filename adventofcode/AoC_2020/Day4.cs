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
        //        public bool Debug { get; set; }

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
            var passports = Passport.ParseInputToPassports(source);

            foreach (var p in passports)
            {
                if (p.ValidateMandatoryFields())
                    part1++;
                var errors = p.ValidateLevel2().ToArray();
                if (errors.Any())
                {
                    foreach(var e in errors)
                        Log(()=>e, 5);

                }
                    else part2++;

            }
            return (part1, part2);
        }
    }

    internal class Passport
    {
        /*
        byr (Birth Year)
        iyr (Issue Year)
        eyr (Expiration Year)
        hgt (Height)
        hcl (Hair Color)
        ecl (Eye Color)
        pid (Passport ID)
        cid (Country ID)        
         */
        private readonly Dictionary<string, string> _data = new Dictionary<string, string>();
        private readonly Dictionary<string, Func<string, bool>> _validators = new Dictionary<string, Func<string, bool>>();
        public Passport()
        {
            _validators["byr"] = value => ValidateRange2(value, 1920, 2002, i => BirthYear = i);
            _validators["iyr"] = value => ValidateRange2(value, 2010, 2020, i => IssueYear = i);
            _validators["eyr"] = value => ValidateRange2(value, 2020, 2030, i => ExpirationYear = i);
            _validators["hgt"] = ValidateHeight;
            _validators["hcl"] = ValidateHairColor;
            _validators["pid"] = ValidatePassportId;
            _validators["ecl"] = ValidateEyeColor;
        }
        #region validators
        private bool ValidateEyeColor(string value)
        {
            if (Enum.TryParse(value, out EyeColor temp))
            {
                EyeCol = temp;
                return true;
            }

            return false;
        }

        private bool ValidatePassportId(string pid)
        {
            if (pid.Length != 9)
                return false;
            if (!int.TryParse(pid, out var pidValue))
                return false;
            PassportId = pidValue;
            return true;
        }

        private bool ValidateHairColor(string haircolor)
        {
            if (haircolor.Length != 7 || haircolor[0] != '#')
                return false;
            var hex = "0123456789abcdef";
            for (int i = 1; i < 7; i++)
                if (!hex.Contains(haircolor[i].ToString()))
                    return false;
            HairColor = Convert.ToInt32(haircolor.Substring(1), 16);
            return true;
        }

        private bool ValidateHeight(string value)
        {
            if (value.EndsWith("cm"))
            {
                HeightUnit = HeightUnit.cm;
                return ValidateRange2(value.Substring(0, value.Length - 2), 150, 193, i => Height = i);
            }

            if (value.EndsWith("in"))
            {
                HeightUnit = HeightUnit.cm;
                return ValidateRange2(value.Substring(0, value.Length - 2), 59, 76, i => Height = i);

            }
            return false;
        }

        private bool ValidateRange2(string value, int min, int max, Action<int> setter)
        {
            if (int.TryParse(value, out var i) && i >= min && i <= max)
            {
                setter(i);
                return true;
            }

            return false;
        }
#endregion


        private static readonly string[] mandatory =
        {
            "byr", "iyr", "eyr", "hgt", "hcl", "ecl", "pid"
        };


        public bool HasData => _data.Any();

        public int BirthYear { get; private set; }
        public int IssueYear { get; private set; }
        public int ExpirationYear { get; private set; }
        public HeightUnit HeightUnit { get; private set; }
        public int Height { get; private set; }
        public int HairColor { get; private set; }
        public EyeColor EyeCol { get; private set; }
        public int PassportId { get; private set; }


        public bool ValidateMandatoryFields()
        {
            foreach (var x in mandatory)
                if (!_data.ContainsKey(x))
                    return false;
            return true;
        }

        public void AddData(string key, string value)
        {
            _data[key] = value;
        }

        public IEnumerable<string> ValidateLevel2()
        {
            var valid = true;
            if (!ValidateMandatoryFields())
            {
                yield return "Mandatory field missing";
                yield break;
            }
            foreach (var key in _validators.Keys)
            {
                if (!_data.ContainsKey(key))
                {
                    yield return $"Passport missing mandatory field {key}";
                } 
                else if (!_validators[key](_data[key]))
                {
                    yield return $"Passport validation failed for {key}: {_data[key]}";
                }
            }
        }

        public static List<Passport> ParseInputToPassports(string[] source)
        {
            var passports = new List<Passport>();
            var passport = new Passport();
            foreach (var line in source)
            {
                if (string.IsNullOrEmpty(line) && passport.HasData)
                {
                    passports.Add(passport);
                    passport = new Passport();
                }
                else
                {
                    foreach (var p in line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var p2 = p.Split(':');
                        passport.AddData(p2[0], p2[1]);
                    }
                }
            }

            if (passport.HasData)
            {
                passports.Add(passport);
            }

            return passports;
        }
    }

    internal enum HeightUnit
    {
        cm, inch
    }

    internal enum EyeColor
    {
        amb, blu, brn, gry, grn, hzl, oth

    }
}