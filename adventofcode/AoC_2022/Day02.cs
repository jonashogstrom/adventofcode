using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;

namespace AdventofCode.AoC_2022
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    // not 6575 - 
    [TestFixture]
    class Day02 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(15, 12, "Day02_test.txt")]
        [TestCase(11063, 10349, "Day02.txt")]
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
            LogAndReset("Parse", sw);

            var dic = new Dictionary<char, Shape>();
            dic['A'] = Shape.Rock;
            dic['B'] = Shape.Paper;
            dic['C'] = Shape.Scissors;
            dic['X'] = Shape.Rock;
            dic['Y'] = Shape.Paper;
            dic['Z'] = Shape.Scissors;

            var strategyDic = new Dictionary<char, GameRes>();
            strategyDic['X'] = GameRes.lose;
            strategyDic['Y'] = GameRes.draw;
            strategyDic['Z'] = GameRes.win;

            LogAndReset("*1", sw);

            var sum = 0;
            foreach (var s in source)
            {
                var parts = s.Split(' ');
                var opponent = dic[parts[0][0]];
                var me = dic[parts[1][0]];
                var res = ComputeGame(me, opponent);
                var gameScore = ((int)res) * 3 + (int)me + 1;
                LogLevel = 0;
                Log($"{me}, {opponent} => {res} / {gameScore}", 0);
                sum += gameScore;

            }

            part1 = sum;

            LogAndReset("*2", sw);

            var sum2 = 0;
            foreach (var s in source)
            {
                var parts = s.Split(' ');
                var opponent = dic[parts[0][0]];

                var strategy = strategyDic[parts[1][0]];
                var me = ComputePlay(opponent, strategy);

                var res = ComputeGame(me, opponent);
                var gameScore = ((int)res) * 3 + (int)me + 1;
                LogLevel = 0;
                Log($"{me}, {opponent} => {res} / {gameScore}", 0);
                sum2 += gameScore;

            }

            part2 = sum2;
            return (part1, part2);
        }

        private Shape ComputePlay(Shape opponent, GameRes strategy)
        {
            if (strategy == GameRes.draw)
                return opponent;
            if (strategy == GameRes.lose)
                return (Shape)(((int)opponent + 2) % 3);
            return (Shape)(((int)opponent + 1) % 3);

        }

        private GameRes ComputeGame(Shape me, Shape opponent)
        {
            if (me == opponent)
                return GameRes.draw;
            var diff = (int)me - (int)opponent;
            if ((diff + 3) % 3 == 1)
                return GameRes.win;
            return GameRes.lose;
        }
    }

    internal enum GameRes
    {
        lose, draw, win
    }

    internal enum Shape
    {
        Rock, Paper, Scissors
    }
}