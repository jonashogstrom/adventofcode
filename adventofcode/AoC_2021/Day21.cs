using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2021
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day21 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(739785, null, "Day21_test.txt")]
        [TestCase(752247, null, "Day21.txt")]
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
            var players = new List<Player>();
            players.Add(new Player(int.Parse(source[0].Split(' ').Last())));
            players.Add(new Player(int.Parse(source[1].Split(' ').Last())));
            LogAndReset("Parse", sw);

            int i = 0;
            var winner = -1;
            var dice = 1;
            var diceRolls = 0;
            while (winner == -1)
            {
                var playerIndex = i % players.Count;
                var p = players[playerIndex];
                var s = $"Player {playerIndex + 1} rolls ";
                for (int roll = 0; roll < 3; roll++)
                {
                    s += dice + ",";
                    p.Move(ref dice);
                    diceRolls++;
                }
            
                p.Score += p.Pos;
                s += $" and moves to space {p.Pos} for a total score of {p.Score}";
                Log(s);
                if (p.IsWinner)
                {
                    part1 = players[(i + 1) % players.Count].Score * diceRolls;
                    break;
                }
                i++;
            }



            LogAndReset("*1", sw);
            LogAndReset("*2", sw);

            return (part1, part2);
        }
    }

    internal class Player
    {
        public int Pos { get; private set; }
        public int Score { get; set; }
        public bool IsWinner => Score >= 1000;

        public Player(int initPos)
        {
            Pos = initPos;
        }

        public void Move(ref int dice)
        {
            Pos = ((Pos + dice) - 1) % 10 + 1;
            dice++;
            if (dice == 101)
                dice = 1;
        }
    }

    internal class Player2
    {
        public int Pos { get; private set; }
        public int Score { get; set; }
        public bool IsWinner => Score >= 1000;

        public Player2(int initPos)
        {
            Pos = initPos;
        }

        private int newPos(int steps)
        {
            return (Pos + steps - 1) % 10 + 1;
        }
        public IEnumerable<Player2> DiracMove()
        {
            yield return new Player2(newPos(1));
            yield return new Player2(newPos(2));
            yield return new Player2(newPos(3));
        }
    }
}