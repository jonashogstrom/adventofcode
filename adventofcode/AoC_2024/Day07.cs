using System;
using System.Diagnostics;
using NUnit.Framework;
using System.Collections.Generic;
using AdventofCode.Utils;
using System.Linq;


namespace AdventofCode.AoC_2024
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day07 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(3749, 11387, "Day07_test.txt")]
        [TestCase(0, 7290, "Day07_test2.txt")]
        [TestCase(1611660863222, 945341732469724, "Day07.txt")]
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

            var equations = new List<Equation>();
            foreach (var line in source)
            {
                var parts = line.Split([':', ' '], StringSplitOptions.RemoveEmptyEntries);
                var res = long.Parse(parts[0]);
                var values = parts.Skip(1).Select(int.Parse).ToArray();
                equations.Add(new Equation(res, values));
            }
            
            LogAndReset("Parse", sw);

            foreach(var eq in equations)
                if (CanSolve(eq, false))
                    part1 += eq.Res;

            LogAndReset("*1", sw);

            foreach(var eq in equations)
                if (CanSolve(eq, true))
                {
                    part2 += eq.Res;
                }

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private bool CanSolve(Equation eq, bool allowConcat)
        {
            return CanSolveRec(eq.Res, eq.Values[0], eq.Values.AsSpan()[1..], allowConcat);
        }

        private bool CanSolveRec(long exp, long sum, Span<int> values, bool allowConcat)
        {
            if (sum > exp) return false;
            if (values.IsEmpty) return exp == sum;
            var rest = values[1..];
            return CanSolveRec(exp, sum * values[0], rest, allowConcat) ||
                   CanSolveRec(exp, sum + values[0], rest, allowConcat) ||
                   (allowConcat && CanSolveRec(exp, long.Parse($"{sum}{values[0]}"), rest, true));
        }
    }

    internal class Equation(long res, int[] values)
    {
        public long Res { get; } = res;
        public int[] Values { get; } = values;
    }
}