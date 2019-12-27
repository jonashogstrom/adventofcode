using System;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Numerics;
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


        [TestCase(null, null, "Day22_testnew.txt", "", 0, 101)]
        [TestCase(null, null, "Day22_test4.txt", "", 0, 101)]

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

            for (int i = 0; i < deck.Length * 3; i++)
            {
                foreach (var instr in x)
                {
                    deck = ExecuteInstruction(instr, deck);
                }
                if (deck.cards[0] == 0)
                    Log("after : " + (i + 1) + ": " + deck.PrintDeck());
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


        /*
        def egcd(a, b):
            if a == 0:
        return (b, 0, 1)
        else:
        g, y, x = egcd(b % a, a)
        return (g, x - (b // a) * y, y)

        def modinv(a, m):
            g, x, y = egcd(a, m)
            if g != 1:
        raise Exception('modular inverse does not exist')
            else:
            return x % m

            */


        long modInverse(long a, long n)
        {
            long i = n, v = 0, d = 1;
            while (a > 0)
            {
                long t = i / a, x = a;
                a = i % x;
                i = x;
                x = d;
                d = v - t * x;
                v = x;
            }
            v %= n;
            if (v < 0) v = (v + n) % n;
            return v;
        }


        BigInteger modInverse(BigInteger a, BigInteger n)
        {
            BigInteger i = n, v = 0, d = 1;
            while (a > 0)
            {
                BigInteger t = i / a, x = a;
                a = i % x;
                i = x;
                x = d;
                d = v - t * x;
                v = x;
            }
            v %= n;
            if (v < 0) v = (v + n) % n;
            return v;
        }

        private long ReverseDeal(long pos, long deckSize)
        {
            return deckSize - 1 - pos;
        }
        private long ReverseCut(long pos, long cutPos, long deckSize)
        {
            return (pos + cutPos + deckSize) % deckSize;
        }

        private long ReverseDealWithIncrement(long pos, long increment, long deckSize)
        {
            var pos2 = new BigInteger(pos);
            var increment2 = new BigInteger(increment);
            var deckSize2 = new BigInteger(deckSize);
            var res = modInverse(increment, deckSize) * pos % deckSize;
            var res2 = modInverse(increment2, deckSize2) * pos2 % deckSize2;
            var res2_long = long.Parse(res2.ToString());
//            Assert.That(res2_long, Is.EqualTo(res));
            return res2_long;
        }

        [TestCase("Day22_test.txt", 10, 1, 6)]
        [TestCase("Day22_test2.txt", 10, 1, 6)]
        [TestCase("Day22_test3.txt", 10, 1, 6)]
        [TestCase("Day22_test4.txt", 10, 1, 6)]
        [TestCase("deal with increment 3", 10, 1, 6)]
        [TestCase("cut -4", 10, 1, 6)]

        public void Test2(string resourceName, long deckSize, long repeats, long finalCardPos)
        {
            var source = GetResource(resourceName);
            var final = new StringBuilder();
            var org = new StringBuilder();
            foreach (var s in source)
                Log(s);
            for (long i = 0; i < deckSize; i++)
            {
                org.Append(i + " ");
                var r = Reverse(source, deckSize, i);
                final.Append(r + " ");
            }
            Log("Org (rev):" + org);
            Log("Final....:" + final);

            // Log(org.ToString);
            // var reversed2 = Reverse(source, deckSize, reversed1);
            // Log(finalCardPos.ToString);
            // Log(reversed1.ToString);
            // Log(reversed2.ToString);
        }

        [TestCase("Day22.txt", 119315717514047, 101741582076661, 2020)]
        [TestCase("Day22.txt", 119315717514047, 101741582076661, 3)]
        [TestCase("Day22.txt", 10007, 17, 7)]
        // [TestCase("Day22_test.txt", 10, 1, 6)]
        // [TestCase("Day22_test2.txt", 10, 1, 6)]
        // [TestCase("Day22_test3.txt", 10, 1, 6)]
        // [TestCase("Day22_test4.txt", 10, 1, 6)]
        // [TestCase("deal with increment 3", 10, 1, 6)]
        // [TestCase("cut -4", 10, 1, 6)]

        public void Test3(string resourceName, long deckSize, long repeats, long finalCardPos)
        {
            var source = GetResource(resourceName);
            var answer = -1;
            if (deckSize < 100000 && repeats < 100)
            {
                var deck = new Deck((int)deckSize, true);

                var x = source.Select(instr => instr.Split(' ')).ToArray();

                for (var round = 0; round < repeats; round++)
                {
                    foreach (var instr in x)
                    {
                        deck = ExecuteInstruction(instr, deck);
                    }
                }

                answer = deck.cards[finalCardPos];
            }

            var d = new BigInteger(deckSize);

            var reversed1 = Reverse(source, deckSize, finalCardPos);
            var reversed2 = Reverse(source, deckSize, reversed1);
            Log("Last:" + finalCardPos);
            Log("Last-1:" + reversed1);
            Log("Last-2:" + reversed2);
            var X = new BigInteger(finalCardPos);
            var Y = new BigInteger(reversed1);
            var Z = new BigInteger(reversed2);
            //            var yminz = ((Y - Z) + deckSize) % deckSize;
            var a = (((Y - Z) + d) % d) * (modInverse(X - Y + d, d) % d);
            a = (a + d) % d;
            var ax = (a * X);
            ax = ax % d;
            var b = (Y - ax) % d;
            b = (b + d) % d;
            Log("A: " + a);
            Log("B: " + b);

            var A = a;
            var B = b;

            var Yver = (a * X + b) % d;
            var ZVer = (a * Y + b) % d;
            Assert.That(Y, Is.EqualTo(Yver));
            Assert.That(Z, Is.EqualTo(ZVer));
            var n = new BigInteger(repeats);
            var powand = BigInteger.ModPow(A, n, d);
            Log("pow(a, n, d) = " + powand);

            // (pow(A, n, D)*X + (pow(A, n, D)-1) * modinv(A-1, D) * B) % D)
            var p1 = powand * X;
            var p2_1 = (powand - 1);
            var p2_2 = modInverse(a - 1, d);
            var p2_3 = B;
            var p2 = p2_1 * p2_2 * p2_3;
            var p = (p1 + p2) % d;
            var pxxx = p % d;
            var res =
                (powand*X + (powand - 1) * modInverse(a - 1, d) * B) % d;

            Log(res.ToString);

            if (answer != -1)
            {
                var intres = int.Parse(res.ToString());
                Assert.That(intres, Is.EqualTo(answer));
            }

            // decksize: 119315717514047 
            // too low:   76866793081881
            //            78613970589919
            // too high:  84291970833532
            //           




        }

        private long Reverse(string[] instructions, long deckSize, long finalCardPos)
        {
            long res = finalCardPos;
            foreach (var instr in instructions.Reverse())
            {
                var next = -1L;
                if (instr.StartsWith("cut"))
                    next = ReverseCut(res, int.Parse(instr.Split(' ').Last()), deckSize);
                else if (instr.StartsWith("deal with"))
                    next = ReverseDealWithIncrement(res, int.Parse(instr.Split(' ').Last()), deckSize);
                else
                    next = ReverseDeal(res, deckSize);
                Assert.That(next, Is.GreaterThan(-1));
                res = next;
            }

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