using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using NUnit.Framework;

namespace AdventofCode.AoC_2020
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day22 : TestBaseClass<Part1Type, Part2Type>
    {
        private int _gameindex;
        public bool Debug { get; set; }

        // not 1968
        [Test]
        [TestCase(306, 291, "Day22_test.txt")]
        [TestCase(35370, 36246, "Day22.txt")]
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

            var g = source.AsGroups().ToArray();
            var piles = ParsePiles(g);
            LogAndReset("Parse", sw);

            var pile = PlayCrabCombat(piles.p1, piles.p2);
            part1 = GetScoreForPile(pile);

            LogAndReset("*1", sw);
            piles = ParsePiles(g);
            
            LogLevel = 5;
            _gameindex = 0;
            var x = RecursiveCombat(piles.p1, piles.p2, new HashSet<string>());
            part2 = GetScoreForPile(x.pile);

            Log($"== Post-game Results ==", 2);
            Log($"Player {x.winner}:" + string.Join(", ", x.pile), 2);
            Log($"Total number of games played: {_gameindex}", 2);

            LogAndReset("*2", sw);
            return (part1, part2);
        }

        private (Queue<int> p1, Queue<int> p2) ParsePiles(IList<string>[] g)
        {
            var player1 = ParsePile(g[0]);
            var player2 = ParsePile(g[1]);
            return (player1, player2);
        }

        private Queue<int> ParsePile(IList<string> playerInfo)
        {
            return new Queue<int>(GetIntInput(playerInfo.Skip(1).ToArray()));
        }

        private static Queue<int> PlayCrabCombat(Queue<int> player1, Queue<int> player2)
        {
            while (player1.Any() && player2.Any())
            {
                var p1 = player1.Dequeue();
                var p2 = player2.Dequeue();
                if (p1 > p2)
                {
                    player1.Enqueue(p1);
                    player1.Enqueue(p2);
                }
                else
                {
                    player2.Enqueue(p2);
                    player2.Enqueue(p1);
                }
            }

            var pile = player1.Any() ? player1 : player2;
            return pile;
        }

        private static int GetScoreForPile(Queue<int> pile)
        {
            return Enumerable.Range(1, pile.Count).Reverse().Zip(pile, (x, y) => x * y).Sum();
        }

//        private Dictionary<string, (int winner, Queue<int> pile, int rounds)> _gameResults = new Dictionary<string, (int winner, Queue<int> pile, int rounds)>();

        private (int winner, Queue<int> pile) RecursiveCombat(Queue<int> player1, Queue<int> player2, HashSet<string> states)
        {
            _gameindex++;
            var localGame = _gameindex;
            var initialState = GetState(player1, player2);
            Log($"=== Game {localGame} ===", 10);
            while (player1.Any() && player2.Any())
            {
                Log(() => $"-- Round {states.Count + 1} (Game{localGame}) --", 10);
                Log(() => $"Player 1's deck:" + string.Join(", ", player1), 10);
                Log(() => $"Player 2's deck:" + string.Join(", ", player2), 10);
                var s = GetState(player1, player2);
                if (states.Contains(s))
                {
//                    _gameResults[initialState] = (1, player1, states.Count);
                    return (1, player1);
                }

                // if (_gameResults.TryGetValue(s, out var result))
                // {
                //     Log($"Shortcut game in round {states.Count+1} (realgame: {result.rounds})");
                //     return (result.winner, result.pile);
                // }

                states.Add(s);

                var p1 = player1.Dequeue();
                Log(() => $"Player 1 plays: {p1}", 10);
                var p2 = player2.Dequeue();
                Log(() => $"Player 2 plays: {p2}", 10);

                var winner = -1;
                if (player1.Count >= p1 && player2.Count >= p2)
                {
                    Log(() => "Playing a sub-game to determine the winner...", 10);
                    var res = RecursiveCombat(
                        new Queue<int>(player1.Take(p1)),
                        new Queue<int>(player2.Take(p2)),
                        new HashSet<string>());
                    winner = res.winner;

                }
                else if (p1 > p2)
                {
                    winner = 1;
                }
                else
                {
                    winner = 2;
                }
                Log(() => $"Player {winner} wins round {states.Count} of game {localGame}", 10);

                if (winner == 1)
                {
                    player1.Enqueue(p1);
                    player1.Enqueue(p2);
                }
                else
                {
                    player2.Enqueue(p2);
                    player2.Enqueue(p1);
                }
            }

            if (player1.Any())
            {
                return (1, player1);
            }
            return (2, player2);
        }

        private string GetState(Queue<int> player1, Queue<int> player2)
        {
            return string.Join(",", player1)+ ":" + string.Join(",", player2);
        }
    }
}
