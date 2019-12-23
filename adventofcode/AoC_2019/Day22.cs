using System;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using NUnit.Framework.Internal;

namespace AdventofCode.AoC_2019
{
    using Part1Type = Int32;
    using Part2Type = Int32;

    [TestFixture]
    class Day22 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(0, null, "Day22_test.txt", "0 3 6 9 2 5 8 1 4 7", 0, 10)]
        [TestCase(7, null, "Day22_test2.txt", "9 2 5 8 1 4 7 0 3 6", 0, 10)]
        [TestCase(2, null, "Day22_test3.txt", "6 3 0 7 4 1 8 5 2 9", 0, 10)]
        [TestCase(1, null, "Day22_test4.txt", "3 0 7 4 1 8 5 2 9 6", 0, 10)]

        [TestCase(4, null, "cut -4", "6 7 8 9 0 1 2 3 4 5", 0, 10)]
        [TestCase(7, null, "cut 3", "3 4 5 6 7 8 9 0 1 2", 0, 10)]

        [TestCase(4096, null, "Day22.txt", "", 2019, 10007)]


        [TestCase(null, null, "Day22_testnew.txt","", 0, 101)]
        [TestCase(null, null, "Day22_test4.txt","", 0, 101)]

        public void Test1(Part1Type exp1, Part2Type? exp2, string resourceName, string order, int finalCard, int deckSize)
        {
            Debug = deckSize < 100;
            var source = GetResource(resourceName);
            var res = Compute(source, order, finalCard, deckSize);

            DoAsserts(res, exp1, exp2);
        }
        // not 9410, 9209

        private (int Part1, int Part2) Compute(string[] source, string order, int finalCard, int deckSize)
        {
            var deck = new Deck(deckSize, true);
            if (Debug)
            {
                Log("Original order (factory):");
                Log(deck.PrintDeck);
            }
            Log("before:    " + deck.PrintDeck());

            var x = source.Select(instr => instr.Split(' ')).ToArray();

            for (int i = 0; i < deck.Length*3; i++)
            {
                foreach (var instr in x)
                {
                    deck = ExecuteInstruction(instr, deck);
                }
                if (deck.cards[0] == 0) 
                    Log("after : "+(i+1)+ ": "+ deck.PrintDeck());
            }

            if (order != "")
            {
                CheckExpectedOrder(order, deck);
            }


            int pos = deck.FindCard(finalCard);
            return (pos, 0);
        }

        [Test]
        public void TestCutAllButOne()
        {
            var deck = new Deck(101, true);
            for (int i = 0; i < deck.Length - 1; i++)
                deck = Cut(deck, 5);

            Log(deck.PrintDeck);
            var deck2 = new Deck(101, true);
            deck2 = Cut(deck2, 5);

            Log(deck2.PrintDeck);

        }

        [Test]
        public void TestDealIncr()
        {
            var deck = new Deck(101, true);
            for (int i = 0; i < deck.Length; i++)
                deck = DealWithIncr(deck, 5);

            Log(deck.PrintDeck);

            var deck2 = new Deck(101, true);
            deck2 = DealWithIncr(deck, 5);

            Log(deck2.PrintDeck);

        }

        private void CheckExpectedOrder(string order, Deck deck)
        {
            Log("Expected:");

            Log(order);
            var orderarr = order.Split(' ').Select(int.Parse).ToArray();
            for (var x = 0; x < orderarr.Length; x++)
            {
                Assert.That(deck.cards[x], Is.EqualTo(orderarr[x]), "error at pos " + x);
            }
        }

        private Deck ExecuteInstruction(string[] instr, Deck deck)
        {
            if (instr[0] == "cut")
                deck = Cut(deck, int.Parse(instr[1]));
            else if (instr[1] == "into")
                deck = DealIntoNew(deck);
            else if (instr[1] == "with")
                deck = DealWithIncr(deck, int.Parse(instr[3]));
            else
            {
                Assert.Fail("unknown instruction");
            }
            //
            // if (Debug)
            // {
            //     Log(instr);
            //     Log(deck.PrintDeck);
            // }

            return deck;
        }

        private Deck DealWithIncr(Deck deck, int incr)
        {
            var res = new Deck(deck.Length);
            var pos = 0;
            for (int i = 0; i < deck.Length; i++)
            {
                res.cards[pos] = deck.cards[i];
                pos = (pos + incr) % deck.Length;
            }

            return res;
        }

        private Deck DealIntoNew(Deck deck)
        {
            var res = new Deck(deck.Length);
            for (int i = 0; i < deck.Length; i++)
                res.cards[deck.Length - 1 - i] = deck.cards[i];
            return res;
        }

        private Deck Cut(Deck deck, int pos)
        {
            var res = new Deck(deck.Length);
            var move = deck.Length - pos;
            for (int i = 0; i < deck.Length; i++)
                res.cards[(i + move) % deck.Length] = deck.cards[i];
            return res;
        }

    }

    public class Deck
    {
        public int[] cards;

        public Deck(int length, bool factoryOrder = false)
        {
            Length = length;
            cards = new int[length];
            for (int i = 0; i < Length; i++)
                cards[i] = factoryOrder ? i : -1;

        }

        public string PrintDeck()
        {
            var sb = new StringBuilder();
            foreach (var x in cards.Take(100))
                sb.Append($"{x:D5} ");
            return sb.ToString();
        }

        public int Length { get; }

        public int FindCard(int card)
        {
            for (int i = 0; i < cards.Length; i++)
                if (cards[i] == card)
                    return i;
            return -1;

        }
    }
}