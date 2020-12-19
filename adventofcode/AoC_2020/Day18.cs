using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace AdventofCode.AoC_2020
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day18 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(1, 1, "1")]
        [TestCase(4, 4, "1+3")]
        [TestCase(71, 231, "1 + 2 * 3 + 4 * 5 + 6 ")]
        [TestCase(51, 51, "1 + (2 * 3) + (4 * (5 + 6))")]
        [TestCase(12240, 669060, "5 * 9 * (7 * 3 * 3 + 9 * 3 + (8 + 6 * 4))")]
        [TestCase(13632, 23340, "((2 + 4 * 9) * (6 + 9 * 8 + 6) + 6) + 2 + 4 * 2")]
        [TestCase(11297104473091, 185348874183674, "Day18.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
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

            foreach (var s in source)
            {
                var rev = new string(s.Reverse().Select(c =>
                {
                    switch (c)
                    {
                        case '(': return ']';
                        case ')': return '[';
                        default: return c;
                    }
                }).ToArray());
                var temp = Evaluate1(rev.Replace(" ", ""), 0);
                part1 += temp.n.calc();
                var temp2 = Evaluate2(rev.Replace(" ", ""), 0);
                part2 += temp2.n.calc();
            }

            LogAndReset("*1", sw);

            LogAndReset("*2", sw);
            return (part1, part2);
        }

        private (node n, int newPos) Evaluate1(string s, int p)
        {
            node res = null;
            while (p < s.Length)
            {
                var c = s[p];
                if (c == '(' || c == '[')
                {
                    var temp = Evaluate1(s, p + 1);
                    res = temp.n;
                    p = temp.newPos;
                }
                else if (c == ')' || c == ']')
                {
                    return (res, p + 1);
                }
                else if (c >= '0' && c <= '9')
                {

                    res = new literal() { value = int.Parse(c.ToString()) };
                    p++;
                }
                else if (c == '*' || c == '+')
                {
                    var temp = Evaluate1(s, p + 1);
                    return (new op(c, res, temp.n), temp.newPos);
                }

            }

            return (res, p);
        }

        private (node n, int newPos) Evaluate2(string s, int p)
        {
            node res = null;
            while (p < s.Length)
            {
                var c = s[p];
                if (c == '(' || c == '[')
                {
                    var temp = Evaluate2(s, p + 1);
                    res = new paren { value = temp.n };
                    p = temp.newPos;
                }
                else if (c == ')' || c == ']')
                {
                    return (res, p + 1);
                }
                else if (char.IsDigit(c))
                {
                    res = new literal() { value = int.Parse(c.ToString()) };
                    p++;
                }
                else if (c == '+')
                {
                    var temp = Evaluate2(s, p + 1);
                    if (temp.n is op t && t.optype == '*')
                    {
                        var n1 = new op('+', res, t.left);
                        var n2 = new op('*', n1, t.right);
                        return (n2, temp.newPos);

                    }
                    return (new op(c, res, temp.n), temp.newPos);
                }
                else if (c == '*')
                {
                    var temp = Evaluate2(s, p + 1);
                    return (new op(c, res, temp.n), temp.newPos);
                }

            }

            return (res, p);
        }
    }
}


public abstract class node
{
    public abstract long calc();
}


public class op : node
{
    public op(char optype, node left, node right)
    {
        this.optype = optype;
        this.left = left;
        this.right = right;
    }

    public char optype;
    public node left;
    public node right;

    public override long calc()
    {
        if (optype == '+')
            return left.calc() + right.calc();
        if (optype == '*')
            return left.calc() * right.calc();
        throw new Exception("unknown op");
    }
}


public class literal : node
{
    public int value;

    public override long calc()
    {
        return value;
    }
}

public class paren : node
{
    public node value;

    public override long calc()
    {
        return value.calc();
    }
}

// not 9796547474617

// var comp = new IntCodeComputer(source[0]);
// comp.Execute();
// var part1 = (int)comp.LastOutput;
// return (part1, 0);
