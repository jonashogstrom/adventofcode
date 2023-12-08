using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Devices.PointOfService;
using Accord;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2023
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day07 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(6440, 5905, "Day07_test.txt")]
        [TestCase(248836197, null, "Day07.txt")]
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

            var hands = ParseHands(source, false);

            LogAndReset("Parse", sw);

            // solve part 1 here
            var orderedHands = hands.OrderBy(x => x.HandType).
                ThenBy(x => x.Labels[0]).
                ThenBy(x => x.Labels[1]).
                ThenBy(x => x.Labels[2]).
                ThenBy(x => x.Labels[3]).
                ThenBy(x => x.Labels[4]).
                ToList();


            for (int i = 0; i < orderedHands.Count; i++)
                part1 += orderedHands[i].Bid * (i + 1);
            LogAndReset("*1", sw);

            hands = ParseHands(source, true);

            // solve part 1 here
            orderedHands = hands.OrderBy(x => x.HandType).
                ThenBy(x => x.Labels[0]).
                ThenBy(x => x.Labels[1]).
                ThenBy(x => x.Labels[2]).
                ThenBy(x => x.Labels[3]).
                ThenBy(x => x.Labels[4]).
                ToList();


            for (int i = 0; i < orderedHands.Count; i++)
                part2 += orderedHands[i].Bid * (i + 1);
            // solve part 2 here

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private static List<Hand> ParseHands(string[] source, bool useJokers)
        {
            var hands = new List<Hand>();
            foreach (var s in source)
            {
                var parts = s.Split(' ');
                var h = new Hand(parts[0], int.Parse(parts[1]), useJokers);
                hands.Add(h);
            }

            return hands;
        }
    }

    [DebuggerDisplay("{Cards} {Bid} ({HandType})")]
    internal class Hand
    {
        public HandType HandType { get; }
        public string Cards { get; }
        public int Bid { get; }

        public Hand(string cards, int bid, bool useJokers = false)
        {
            Cards = cards;
            Bid = bid;
            // var bestCardType = HandType.Highcard;
            // if (useJokers && Cards.Contains('J'))
            // {
            //     
            //     var otherCards = new[] { 'A', 'K', 'Q', 'T', '9', '8', '7', '6', '5', '4', '3', '2' };
            //     for (int i = 0; i < 4; i++)
            //     {
            //         if (Cards[i] == 'J')
            //         {
            //             foreach(var c in otherCards)
            //
            //         }
            //     }
            // }

            HandType = GetHandType(Cards, useJokers);
            Labels = Cards.Select(c => GetLabel(c, useJokers)).ToArray();
        }

        public int[] Labels { get; }

        private int GetLabel(char argCard, bool useJokers = false)
        {
            switch (argCard)
            {
                case 'A': return 14;
                case 'K': return 13;
                case 'Q': return 12;
                case 'J': return useJokers ? -1 : 11;
                case 'T': return 10;
            }

            return int.Parse(argCard.ToString());
        }

        private HandType GetHandType(string cards, bool useJokers)
        {
            var cardCount = new DicWithDefault<char, int>();
            foreach (var card in cards)
            {
                cardCount[card]++;
            }

            var jokers = 0;
            if (useJokers && cardCount['J'] > 0)
            {
                jokers = cardCount['J'];
                cardCount.RemoveKey('J');
                if (jokers == 5) return HandType.Fiveofakind;
            }

            var values = cardCount.Values.OrderByDescending(x => x).ToArray();
            values[0] += jokers;
            if (cardCount.Keys.Count() == 1)
                return HandType.Fiveofakind;
            if (cardCount.Keys.Count() == 2)
            {
                if (values[0] == 4)
                    return HandType.Fourofakind;
                if (values[0] == 3)
                    return HandType.Fullhouse;
            }
            if (cardCount.Keys.Count() == 3)
            {
                if (values[0] == 3)
                    return HandType.Threeofakind;
                if (values[0] == 2)
                    return HandType.Twopair;
            }

            if (cardCount.Keys.Count() == 4)
            {
                return HandType.Onepair;
            }

            return HandType.Highcard;
        }
    }

    internal enum HandType
    {
        Highcard,
        Onepair,
        Twopair,
        Threeofakind,
        Fullhouse,
        Fourofakind,
        Fiveofakind,
    }
}