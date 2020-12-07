using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventofCode.AoC_2020
{
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
        private readonly List<string> _validationErrors = new List<string>();
        public IEnumerable<string> ValidationErrors => _validationErrors;
        public bool IsValid => !ValidationErrors.Any();
        public bool HasMandatoryFields { get; private set; }

        public Passport(Dictionary<string, string> dictionary)
        {
            foreach (var k in dictionary.Keys)
            {
                _data[k] = dictionary[k];
            }
            _validators["byr"] = value => ValidateRange2(value, 1920, 2002, i => BirthYear = i);
            _validators["iyr"] = value => ValidateRange2(value, 2010, 2020, i => IssueYear = i);
            _validators["eyr"] = value => ValidateRange2(value, 2020, 2030, i => ExpirationYear = i);
            _validators["hgt"] = ValidateHeight;
            _validators["hcl"] = ValidateHairColor;
            _validators["pid"] = ValidatePassportId;
            _validators["ecl"] = ValidateEyeColor;
            ValidatePassport();

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
                HeightUnit = HeightUnit.inch;
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

        #region properties
        public int BirthYear { get; private set; }
        public int IssueYear { get; private set; }
        public int ExpirationYear { get; private set; }
        public HeightUnit HeightUnit { get; private set; }
        public int Height { get; private set; }
        public int HairColor { get; private set; }
        public EyeColor EyeCol { get; private set; }
        public int PassportId { get; private set; }
        #endregion

        private void ValidatePassport()
        {
            HasMandatoryFields = true;
            foreach (var key in _validators.Keys)
            {
                if (!_data.ContainsKey(key))
                {
                    _validationErrors.Add($"Passport missing mandatory field {key}");
                    HasMandatoryFields = false;

                }
                else if (!_validators[key](_data[key]))
                {
                    _validationErrors.Add($"Passport validation failed for {key}: {_data[key]}");
                }
            }
        }

        public static IEnumerable<Passport> ParseInputToPassports(string[] source)
        {
            foreach (var g in source.AsGroups())
            {
                var fields = new Dictionary<string, string>();
                foreach (var line in g)
                    foreach (var p in line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var keyValuePair = p.Split(':');
                        fields[keyValuePair[0]] = keyValuePair[1];
                    }
                yield return new Passport(fields);
            }
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