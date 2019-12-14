using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using NUnit.Framework;

namespace AdventofCode.AoC_2019
{
    using Part1Type = Int32;
    using Part2Type = Int32;

    [TestFixture]
    class Day13 : TestBaseClass<Part1Type, Part2Type>
    {
        [Test]
        [TestCase(372, 19297, "Day13.txt")]
        public void Test1(Part1Type exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = Compute(source[0]);
            DoAsserts(res, exp1, exp2);
        }

        private (Part1Type Part1, Part2Type Part2) Compute(string source)
        {
            // part1
            var comp1 = new IntCodeComputer(source);
            comp1.Execute();
            var game1 = new GameScreen();
            ParseScreen(comp1, game1);

            // part2
            var comp2 = new IntCodeComputer(source);
            comp2.WriteMemory(0, 2);
            comp2.Execute();

            var game2 = new GameScreen();
            ParseScreen(comp2, game2);
            PrintGame(game2);
            while (game2.BlocksLeft > 0)
            {
                comp2.AddInput(game2.recommendedPaddleDir);
                comp2.Execute();

                ParseScreen(comp2, game2);
            //    PrintGame(game2);
                if (game2.ballPos.Y > game2.paddlePos.Y)
                {
                    Log("Ball out of screen???");
                    break;
                }
            }
            PrintGame(game2);
            return (game1.BlocksLeft, game2.Score);
        }

        private void PrintGame(GameScreen game)
        {
            Log(game.Screen.ToString(id =>
            {
                switch (id)
                {
                    case 0: return " ";
                    case 1: return "#";
                    case 2: return "X";
                    case 3: return "-";
                    case 4: return "O";
                    default: throw new Exception();
                }
            }));
            Log("Score: " + game.Score);
            Log("recommended dir: " + game.recommendedPaddleDir);
        }

        private static void ParseScreen(IntCodeComputer comp, GameScreen Game)
        {
            while (comp.OutputQ.Any())
            {
                var x = (int)comp.OutputQ.Dequeue();
                var y = (int)comp.OutputQ.Dequeue();
                var id = (int)comp.OutputQ.Dequeue();
                if (x == -1)
                    Game.Score = id;
                else
                {
                    Game.Screen.Set(x, y, id);
                    switch (id)
                    {
                        case 0: break; // emoty
                        case 1: // Wall
                            break; 

                        case 2: // block
                            break;
                        case 3: // paddle
                            Game.paddlePos = Coord.FromXY(x, y);
                            break;
                        case 4: // ball
                            if (Game.ballPos != null)
                                Game.Screen[Game.ballPos] = 0;
                            Game.ballPos = Coord.FromXY(x, y);

                            break;
                        default: throw new Exception();
                    }
                }
            }

            Game.BlocksLeft = Game.Screen.Keys.Count(k => Game.Screen[k] == 2);

            if (Game.ballPos != null && Game.paddlePos != null)
            {
                if (Game.ballPos.X == Game.paddlePos.X) Game.recommendedPaddleDir = 0;
                if (Game.ballPos.X < Game.paddlePos.X) Game.recommendedPaddleDir = -1;
                if (Game.ballPos.X > Game.paddlePos.X) Game.recommendedPaddleDir = 1;
            }
        }
    }

    internal class GameScreen
    {

        public SparseBuffer<int> Screen = new SparseBuffer<int>();
        public int Score;
        public Coord ballPos;
        public Coord paddlePos;
        public int recommendedPaddleDir;
        public int BlocksLeft { get; set; }
    }
}