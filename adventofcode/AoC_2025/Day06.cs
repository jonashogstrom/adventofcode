using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2025
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    public class Day06 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(4277556, 3263827, "<day>_test.txt")]
        [TestCase(5381996914800, 9627174150897, "<day>.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            LogLevel = resourceName.Contains("test") ? 20 : -1;
            var source = GetResource2(ref resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1 = 0;
            Part2Type part2 = 0;
            var sw = Stopwatch.StartNew();

            var values = new List<int[]>();
            foreach (var s in source.Take(source.Length-1))
                values.Add(s.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray());

            var signs = source.Last().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            LogAndReset("Parse", sw);

            for (int i = 0; i < values.First().Length; i++)
            {
                var op = signs[i];
                long init = op == "+" ? 0 : 1;
                foreach(var v in values)
                    if (op == "+")
                        init = init + v[i];
                    else
                        init = init * v[i];

                part1 += init;
            }

            LogAndReset("*1", sw);
            
            var operations = source.Last();
            var operationPos = new List<int>();
            for (int col0 = 0; col0 < operations.Length; col0++)
            {
                if (operations[col0] != ' ')
                    operationPos.Add(col0);
            }

            operationPos.Add(operations.Length+1);

            for (int i = 0; i < operationPos.Count - 1 ; i++)
            {
                var operationCol = operationPos[i];
                var lastCol = operationPos[i + 1]-2;
                var operation = operations[operationCol];
                long sum = operation == '+' ? 0 : 1;

                for (var col = lastCol; col >= operationCol; col--)
                {
                    var valueStr = "";
                    foreach (var s in source.SkipLast(1))
                        valueStr += s[col];
                    var value =  long.Parse(valueStr.Trim());
                    Log(()=>$"{valueStr} => {value}");
                    if (operation == '+')
                        sum += value;
                    else
                        sum *= value;
                    
                }
                Log(()=>$"Sum {sum}");

                part2 += sum;
                Log(()=>"");
            }

            LogAndReset("*2", sw);

            return (part1, part2);
        }
    }
}