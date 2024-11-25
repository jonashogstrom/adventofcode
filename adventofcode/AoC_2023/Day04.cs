using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2023
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day04 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(13, 30, "Day04_test.txt")]
        [TestCase(27454, 6857330, "Day04.txt")]
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

            var sum = 0;
            var cards = new List<int>();
            foreach (var s in source)
            {
                // Card 1: 41 48 83 86 17 | 83 86  6 31 17  9 48 53
                var node = new SplitNode(s, ':', '|' , ' ' );

                
                var winningNumbers = node.Second.First.IntChildren.ToArray();
                var myNumbers = node.Second.Second.IntChildren.ToArray();

                var matches = 0;

                foreach (var v in myNumbers)
                {
                    if (winningNumbers.Contains(v))
                        matches += 1;
                }

                cards.Add(matches);
            }


            LogAndReset("Parse", sw);

            foreach (var s in cards)
                if (s > 0)
                    sum += 1 << (s - 1);
            part1 = sum;

            LogAndReset("*1", sw);


            var res = new DicWithDefault<int, int>(0);
            for (int i = 0; i < cards.Count; i++)
            {
                res[i] = 1;
            }

            for (int i = 0; i < cards.Count; i++)
                for (int j = 0; j < cards[i]; j++)
                    res[i + j + 1] += res[i];

            // var queue = new Queue<int>(Enumerable.Range(0, cards.Count));
            //
            // while (queue.Any())
            // {
            //     part2 += 1;
            //     var cardIndex = queue.Dequeue();
            //     if (cards[cardIndex] > 0)
            //     {
            //         for (var i = 0; i < cards[cardIndex]; i++)
            //         {
            //             var newCardIndex = cardIndex + i + 1;
            //             queue.Enqueue(newCardIndex);
            //         }
            //     }
            // }
            part2 = res.Values.Sum();

            LogAndReset("*2", sw);

            return (part1, part2);
        }


    }
}