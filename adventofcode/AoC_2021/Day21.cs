using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Accord.Collections;
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
        [TestCase(739785, 444356092776315, "Day21_test.txt")]
        [TestCase(752247, 221109915584112, "Day21.txt")]
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
            var p1InitPos = int.Parse(source[0].Split(' ').Last());
            var p2InitPos = int.Parse(source[1].Split(' ').Last());
            players.Add(new Player(p1InitPos));
            players.Add(new Player(p2InitPos));
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
                Log(s,30);
                if (p.IsWinner)
                {
                    part1 = players[(i + 1) % players.Count].Score * diceRolls;
                    break;
                }
                i++;
            }

            var players2 = new List<Player2>();
            players2.Add(new Player2(p1InitPos));
            players2.Add(new Player2(p2InitPos));

            var states = new Dictionary<string, GameState>();

            var queue = new PriorityQueue<GameState>(1000000);
            var winnerCount = new[] { 0L, 0L };
            var initState = new GameState()
            {
                Players = players2.ToArray(),
                Roll = 0,
                StateCount = 1
            };
            queue.Enqueue(initState, 0);
            states[GetStateString(initState.Players, initState.Roll)] = initState;

            while (queue.Any())
            {
                var statenode = queue.Dequeue();
                var state = statenode.Value;

                var playerInTurnIndex = state.Roll / 3;

                var playerInTurn = state.Players[playerInTurnIndex];
                var newPlayers = playerInTurn.DiracMove();
                foreach (var p in newPlayers)
                {
                    if (state.Roll % 3 == 2)
                        p.Score += p.Pos;

                    if (p.IsWinner)
                    {
                        winnerCount[playerInTurnIndex] += state.StateCount;
                    }
                    else
                    {
                        var playersCopy = (Player2[])state.Players.Clone();
                        playersCopy[playerInTurnIndex] = p;

                        var newRoll = (state.Roll + 1) % 6;
                        var stateString = GetStateString(playersCopy, newRoll);
                        if (!states.TryGetValue(stateString, out var newState))
                        {
                            newState = new GameState()
                            {
                                Players = playersCopy,
                                Roll = newRoll,
                                StateCount = state.StateCount
                            };
                            states[stateString] = newState;
                            queue.Enqueue(newState,
                                (newState.Players[0].Score + newState.Players[1].Score) * 10 + newState.Roll);
                        }
                        else
                        {
                            newState.StateCount += state.StateCount;
                        }
                    }
                }
            }

            part2 = winnerCount.Max();
            LogAndReset("*1", sw);
            LogAndReset("*2", sw);

            return (part1, part2);
        }

        public static string GetStateString(Player2[] players, int roll)
        {
            return $"P1:{players[0].Pos}|{players[0].Score} P2:{players[1].Pos}|{players[1].Score} Roll:{roll}";
        }
    }
    internal class GameState
    {
        public Player2[] Players { get; set; }
        public int Roll { get; set; }
        public long StateCount { get; set; }

        public override string ToString()
        {
            return StateCount + " - " + Day21.GetStateString(Players, Roll);
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
        public bool IsWinner => Score >=21;

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
            yield return new Player2(newPos(1)) { Score = Score };
            yield return new Player2(newPos(2)) { Score = Score };
            yield return new Player2(newPos(3)) { Score = Score };
        }
    }
}