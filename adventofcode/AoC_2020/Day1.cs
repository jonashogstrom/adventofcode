using System;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2020
{
    using Part1Type = Int32;
    using Part2Type = Int32;

    [TestFixture]
    class Day1 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(514579, 241861950, "Day1_test.txt")]
        [TestCase(806656, 230608320, "Day1.txt")]
        public void Test1(Part1Type exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            var ints = GetIntInput(source);

            var answ1 = GetAnswer1(ints);
            var answ2 = GetAnswer2(ints);

            return (answ1, answ2);

        }

        private int GetAnswer1(int[] ints)
        {
            for (int i = 0; i < ints.Length - 2; i++)
                for (int j = i + 1; j < ints.Length - 1; j++)
                    if (ints[i] + ints[j] == 2020)
                    {
                        return ints[i] * ints[j];
                    }

            return -1;
        }

        private int GetAnswer2(int[] ints)
        {
            ints = ints.OrderBy(x => x).ToArray();
            var counter = 0;
            for (int i = 0; i < ints.Length - 3; i++)
                for (int j = i + 1; j < ints.Length - 2; j++)
                {
                    var sum2 = ints[i] + ints[j];
                    if (sum2 >= 2020)
                        break;

                    for (int k = j + 1; k < ints.Length - 1; k++)
                    {
                        counter++;
                        var sum3 = ints[i] + ints[j] + ints[k];
                        if (sum3 > 2020)
                            break;
                        if (sum3 == 2020)
                        {
                            Log($"i={i}, j={j}, k={k}");
                            Log($"v1={ints[i]}, v2={ints[j]}, v3={ints[k]}");
                            Log($"comparisons: {counter}");
                            return ints[i] * ints[j] * ints[k];
                        }

                    }
                }

            return -1;
        }
    }
}