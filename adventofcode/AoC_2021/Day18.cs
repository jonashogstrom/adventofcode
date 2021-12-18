using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2021
{
    using Part1Type = Int64;
    using Part2Type = Int64;
    // 4471 part2 is too low
    [TestFixture]
    class Day18 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(4140, 3993, "Day18_test.txt")]
        [TestCase(1384, 1384, "Day18_test2.txt")]
        [TestCase(3488, null, "Day18_test3.txt")]
        [TestCase(4347, 4721, "Day18.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            LogLevel = resourceName.Contains("test") ? 15 : -1;
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1 = 0;
            Part2Type part2 = 0;
            var sw = Stopwatch.StartNew();
            var numbers = new List<SFNumber>();
            foreach (var s in source)
            {
                numbers.Add(ParseNumber(s, 0, 0).num);
            }
            LogAndReset("Parse", sw);
            var num = numbers.First();
            Log($"Initial: " + num);
            foreach (var x in numbers.Skip(1))
            {
                var newNum = new SFPair() { Left = num, Right = x };
                newNum.Left.Parent = newNum;
                newNum.Right.Parent = newNum;
                Reduce(newNum);
                num = newNum;
                Log("Result: " + num);
            }
            part1 = num.Magnitude();
            LogAndReset("*1", sw);

            numbers = new List<SFNumber>();
            foreach (var s in source)
            {
                numbers.Add(ParseNumber(s, 0, 0).num);
            }
            for (int i = 0; i < source.Length; i++)
                for (int j = 0; j < source.Length; j++)
                    if (j != i)
                    {
                        var num1 = ParseNumber(source[i], 0, 0);
                        var num2 = ParseNumber(source[j], 0, 0);
                        var newNum = new SFPair() { Left = num1.num, Right = num2.num };
                        newNum.Left.Parent = newNum;
                        newNum.Right.Parent = newNum;
                        Reduce(newNum);
                        var mag = newNum.Magnitude();
                        if (mag > part2)
                        {
                            part2 = mag;
                            Log($"{mag} : {source[i]} + {source[j]} = {newNum}");
                        }
                        part2 = Math.Max(part2, mag);
                    }

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        [Test]
        [TestCase("[[[[[9,8],1],2],3],4]", "[[[[0,9],2],3],4]")]
        [TestCase("[7,[6,[5,[4,[3,2]]]]]", "[7,[6,[5,[7,0]]]]")]
        [TestCase("[[3,[2,[8,0]]],[9,[5,[4,[3,2]]]]]", "[[3,[2,[8,0]]],[9,[5,[7,0]]]]")]
        //[TestCase("[[3,[2,[1,[7,3]]]],[6,[5,[4,[3,2]]]]]", "[[3,[2,[8,0]]],[9,[5,[4,[3,2]]]]]")] test case is only for a single explode, not a reduce
        [TestCase("[[6,[5,[4,[3,2]]]],1]", "[[6,[5,[7,0]]],3]")]
        [TestCase("[[[[[4,3],4],4],[7,[[8,4],9]]],[1,1]]", "[[[[0,7],4],[[7,8],[6,0]]],[8,1]]")]
        [TestCase("[[[[0,7],4],[[7,8],[0,[6,7]]]],[1,1]]", "[[[[0,7],4],[[7,8],[6,0]]],[8,1]]")]
        [TestCase("[[[[0,7],4],[[7,8],[0,13]]],[1,1]]", "[[[[0,7],4],[[7,8],[6,0]]],[8,1]]")]
        [TestCase("[[[[0,[4,5]],[0,0]],[[[4,5],[2,6]],[9,5]]],[7,[[[3,7],[4,3]],[[6,3],[8,8]]]]]", "[[[[4,0],[5,4]],[[7,7],[6,0]]],[[8,[7,7]],[[7,9],[5,0]]]]")]


        [TestCase("[[6,[5,[4,[3,2]]]],1]", "[[6,[5,[7,0]]],3]")]
        public void TextReduce(string s, string exp)
        {
            LogLevel = 10;
            var x = ParseNumber(s, 0, 0);
            Reduce(x.num as SFPair);
            Log(x.num.ToString());
            Assert.That(x.num.ToString(), Is.EqualTo(exp));
        }

        [Test]
        [TestCase("[[9,1],[1,9]]", 129)]
        [TestCase("[[[[8,7],[7,7]],[[8,6],[7,7]]],[[[0,7],[6,6]],[8,7]]]", 3488)]
        [TestCase("[[[[7,8],[6,6]],[[6,0],[7,7]]],[[[7,8],[8,8]],[[7,9],[0,6]]]]", 3993)]
        public void TestMagnitude(string s, long exp)
        {
            var x = ParseNumber(s, 0, 0);
            Log(x.num.ToString());
            Assert.That(x.num.Magnitude(), Is.EqualTo(exp));
        }

        [Test]
        [TestCase("[[[[4,3],4],4],[7,[[8,4],9]]]|[1,1]:", "[[[[0,7],4],[[7,8],[6,0]]],[8,1]]")]
        [TestCase("[1,1]|[2,2]|[3,3]|[4,4]|[5,5]|[6,6]", "[[[[5,0],[7,4]],[5,5]],[6,6]]")]
        [TestCase("[[[[1,1],[2,2]],[3,3]],[4,4]]|[5,5]", "[[[[3,0],[5,3]],[4,4]],[5,5]]")]
        [TestCase("[[2,[[7,7],7]],[[5,8],[[9,3],[0,2]]]]|[[[0,[5,8]],[[1,7],[9,6]]],[[4,[1,2]],[[1,4],2]]]", "[[[[7,8],[6,6]],[[6,0],[7,7]]],[[[7,8],[8,8]],[[7,9],[0,6]]]]")]
        public void TestSum(string numbersStr, string exp)
        {
            LogLevel = 10;
            var numbers = numbersStr.Split('|');

            var num = ParseNumber(numbers[0], 0, 0).num;
            foreach (var x in numbers.Skip(1))
            {
                var newNum = new SFPair() { Left = num, Right = ParseNumber(x, 0, 0).num };
                newNum.Left.Parent = newNum;
                newNum.Right.Parent = newNum;
                Reduce(newNum);
                num = newNum;
                Log("Result: " + num);
            }
            Assert.That(num.ToString(), Is.EqualTo(exp));
        }

        private void Reduce(SFPair root)
        {
            var stable = false;
            while (!stable)
            {
                stable = true;
                var expl = FindExplodeCandidate(root);
                if (expl.Item1 != null)
                {
                    expl.Item1.Explode(expl.left, expl.right);
                    Log($"Explode {expl.Item1} ({expl.left}, {expl.right}", 20);
                    stable = false;
                    continue;
                }

                var split = FindSplitCandidate(root);
                if (split != null)
                {
                    Log($"Split {split}", 20);
                    split.Split();
                    stable = false;
                    continue;
                }
            }
        }

        private SFRegular FindSplitCandidate(SFNumber root)
        {
            if (root is SFRegular r)
            {
                if (r.Value > 9)
                    return r;
                return null;
            }

            var p = root as SFPair;
            var c = FindSplitCandidate(p.Left);
            if (c == null)
                c = FindSplitCandidate(p.Right);
            return c;
        }

        private (SFPair, SFRegular left, SFRegular right) FindExplodeCandidate(SFPair root)
        {
            return FindExplodeCandidateRec(root, null, null, null, 0);
        }

        private (SFPair, SFRegular left, SFRegular right) FindExplodeCandidateRec(SFNumber root, SFPair candidate, SFRegular left, SFRegular right, int depth)
        {
            if (root is SFRegular n)
            {
                if (candidate == null)
                    return (candidate, n, null);
                return (candidate, left, n);
            }

            var p = root as SFPair;
            if (depth == 4 && candidate == null)
            {
                return (p, left, null);
            }

            if (right != null)
                return (candidate, left, right);

            var x = FindExplodeCandidateRec(p.Left, candidate, left, right, depth + 1);
            if (x.right != null)
                return x;

            if (x.Item1 != null)
            {
                var y = FindExplodeCandidateRec(p.Right, x.Item1, x.left, x.right, depth + 1);
                if (y.right != null)
                    return (x.Item1, x.left, y.right);
            }

            var z = FindExplodeCandidateRec(p.Right, x.Item1, x.left, x.right, depth + 1);
            return z;
        }

        private (SFNumber num, int newPos) ParseNumber(string s, int pos, int depth)
        {
            if (s[pos] == '[')
            {
                var l = ParseNumber(s, pos + 1, depth + 1);
                pos = l.newPos;
                var r = ParseNumber(s, pos + 1, depth + 1);
                pos = r.newPos + 1;
                var res = new SFPair() { Left = l.num, Right = r.num };
                l.num.Parent = res;
                r.num.Parent = res;
                return (res, pos);
            }

            var str = "";
            while (s[pos] >= '0' && s[pos] <= '9')
            {
                str += s[pos];
                pos++;
            }
            var value = int.Parse(str);
            return (new SFRegular() { Value = value }, pos);
        }
    }

    internal abstract class SFNumber
    {
        //public int Depth;
        public SFPair Parent;

        public abstract long Magnitude();
    }
    [DebuggerDisplay("{Value}")]
    internal class SFRegular : SFNumber
    {
        public long Value;
        public override long Magnitude()
        {
            return Value;
        }
        public override string ToString()
        {
            return Value.ToString();
        }

        public void Split()
        {
            var newPair = new SFPair()
            {
                Left = new SFRegular() { Value = Value / 2 },
                Right = new SFRegular() { Value = (Value + 1) / 2 }
            };
            newPair.Left.Parent = newPair;
            newPair.Right.Parent = newPair;
            newPair.Parent = Parent;
            if (Parent.Left == this)
                Parent.Left = newPair;
            else
            {
                Parent.Right = newPair;
            }
        }
    }

    [DebuggerDisplay("[{Left},{Right}]")]
    internal class SFPair : SFNumber
    {
        public SFNumber Left;
        public SFNumber Right;
        public override long Magnitude()
        {
            return 3 * Left.Magnitude() + 2 * Right.Magnitude();
        }

        public override string ToString()
        {
            return $"[{Left},{Right}]";
        }

        public void Explode(SFRegular closestLeft, SFRegular closestRight)
        {
            if (closestLeft != null)
                closestLeft.Value += (Left as SFRegular).Value;
            if (closestRight != null)
                closestRight.Value += (Right as SFRegular).Value;

            // replace the node in parent
            if (Parent.Left == this)
                Parent.Left = new SFRegular() { Value = 0, Parent = Parent };
            else
                Parent.Right = new SFRegular() { Value = 0, Parent = Parent };
        }
    }
}