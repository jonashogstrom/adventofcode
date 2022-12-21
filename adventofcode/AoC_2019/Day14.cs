using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2019
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day14 : TestBaseClass<Part1Type, Part2Type>
    {
        [Test]
        [TestCase(31, null, "Day14_test.txt")]
        [TestCase(165, null, "Day14_test2.txt")]
        [TestCase(13312, 82892753, "Day14_testbig1.txt")]
        [TestCase(180697, 5586022, "Day14_testbig2.txt")]
        [TestCase(2210736, 460664, "Day14_testbig3.txt")]
        [TestCase(720484, 1993284, "Day14.txt")]
        public void Test1(Part1Type exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = Compute(source);
            DoAsserts(res, exp1, exp2);

            // 721576 is too high
        }

        private class Reaction

        {
            public List<Tuple<int, string>> ingredients = new List<Tuple<int, string>>();
            public Tuple<int, string> produced;

            public override string ToString()
            {
                string s = "";
                foreach (var x in ingredients)
                    s += $"{x.Item1} {x.Item2}, ";

                return $"{s} => {produced.Item1} {produced.Item2}";
            }
        }

        private (Part1Type Part1, Part2Type Part2) Compute(string[] source)
        {
            var reactions = ParseInput(source);
            var backlog = new Dictionary<string, long>();
            backlog["FUEL"] = 1;
            var p1 = CalcFor2(reactions, backlog);

            var x = p1;

            var bit = 40;
            var value = 0L;
            var onetrillion = 1000000000000;
            while (bit > 0)
            {
                bit--;
                var test = value + (1L << bit);

                backlog = new Dictionary<string, long>();
                backlog["FUEL"] = test;
                x = CalcFor2(reactions, backlog);
                Log($"{x} ORE required for  {test} FUEL");
                if (x > onetrillion)
                {
                    Log($"bit {bit} must be zero");
                }
                else
                {
                    value = test;
                }
            }
            return (p1, value);
        }

        private long CalcFor2(Dictionary<string, Reaction> reactions, Dictionary<string, long> backlog)
        {
            while (backlog.Keys.Any(k => k != "ORE" && backlog[k] > 0))
            {
                string candidate = null;
                var candidates = backlog.Keys.Where(c => c != "ORE").ToArray();
                candidates = candidates.Where(c => backlog[c] > 0).ToArray();
                foreach (var n in candidates)
                {
                    var isCandidate = true;
                    foreach (var r in reactions.Values)
                        if (r.ingredients.Any(ingredient => ingredient.Item2 == n) &&
                            backlog.ContainsKey(r.produced.Item2) && backlog[r.produced.Item2] > 0)
                        {
                            isCandidate = false;
                            break;
                        }
                    if (isCandidate)
                    {
                        candidate = n;
                        break;
                    }
                }
                var p = candidate;

                var x = CalcFor(reactions, p, backlog[p], out var spare);
                if (spare != 0)
                {
                    //Log($"Created spares: {spare} {p}");
                    backlog[p] = -spare;
                }
                else
                {
                    backlog.Remove(p);
                }
                foreach (var item in x)
                {
                    if (!backlog.ContainsKey(item.Key))
                        backlog[item.Key] = item.Value;
                    else
                    {
                        backlog[item.Key] += item.Value;
                        if (backlog[item.Key] == 0)
                            backlog.Remove(item.Key);
                    }
                }
            }

            return backlog["ORE"];
        }


        private Dictionary<string, long> CalcFor(Dictionary<string, Reaction> reactions, string needed, long amount, out long spare)
        {

            var res = new Dictionary<string, long>();
            var reaction = reactions[needed];
            var multiplier = amount / reaction.produced.Item1;
            if (multiplier * reaction.produced.Item1 < amount)
                multiplier += 1;
            //            if (reaction.ingredients.Count == 1 && reaction.ingredients[0].Item2 == "ORE")
            //            {
            //                var ore = reaction.ingredients[0].Item1 * multiplier;
            //                Log($"Consume {ore} ORE to produce {amount} of {needed}");
            //                res["ORE"] = ore;
            //                spare = multiplier * reaction.produced.Item1 - amount;
            //                return res;
            //            }

//            Log("==================================");
//            Log("Reaction: " + reaction.ToString());
//            Log("Needed: " + amount);

            foreach (var n in reaction.ingredients)
            {

                if (!res.ContainsKey(n.Item2))
                    res[n.Item2] = 0;
                var x = n.Item1 * multiplier;

               // Log($"Consume {x} {n.Item2} to produce {amount} of {needed}");
                res[n.Item2] = x;

            }
            spare = multiplier * reaction.produced.Item1 - amount;

            return res;

        }

        private Dictionary<string, Reaction> ParseInput(string[] source)
        {
            var reactoins = new Dictionary<string, Reaction>();
            foreach (var l in source)
            {
                var r = new Reaction();
                var p1 = l.Split(new string[] { "=>" }, StringSplitOptions.None);
                foreach (var needed in p1[0].Split(','))
                {
                    r.ingredients.Add(ParseReaction(needed));
                }

                r.produced = ParseReaction(p1[1]);
                reactoins[r.produced.Item2] = r;
            }

            return reactoins;
        }

        private Tuple<int, string> ParseReaction(string p0)
        {
            var x = p0.Trim().Split(' ');
            return new Tuple<int, string>(int.Parse(x[0].Trim()), x[1].Trim());
        }
    }
}