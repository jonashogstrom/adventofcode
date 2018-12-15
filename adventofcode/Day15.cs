using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;

using System;
using System.Collections;
using Tuple = System.Tuple;

namespace adventofcode
{
    // 241251 too high
    // 217840 too low // not right
    // 220563 too high

    //Part2: 22366 too low
    internal class Day15 : BaseDay
    {
        protected override void Setup()
        {
            UseTestData = true;
            UseTestData = false;

            Part1TestSolution = null;
            Part2TestSolution = null;
            Part1Solution = 217890;
            Part2Solution = 43645;
        }

        protected override void DoRun(string[] input)
        {
            var board = new List<string>();
            int answer = 0;
            foreach (var row in input)
            {
                if (row.StartsWith("//"))
                    answer = int.Parse(row.Substring(2).Trim());
                else if (row.Trim() == "" && board.Count > 0)
                {
                    var attack = 4;
                    while (RunBoard(board, answer, attack) == RaceEnum.Goblin)
                        attack++;
                    board.Clear();
                }
                else board.Add(row);

            }

            if (board.Count > 0)
            {
                var attack = 3;
                
                while (RunBoard(board, answer, attack) == RaceEnum.Goblin)
                {
                    attack++;
                }
                //Part2 = attack;
            }

        }

        private RaceEnum RunBoard(List<string> strboard, int answer, int elfAttack)
        {
            var board = ParseBoard(strboard, elfAttack);
            var round = 0;
            PrintBoard(board, round);
            var battleTerminated = false;
            var startelves = board.Soldiers.Count(s => s.Race == RaceEnum.Elf);
            while (!board.BattleOver())
            {
                // perform round
                foreach (var x in board.Soldiers.OrderBy(s => s.ReadingOrder).ToArray())
                {
                    // perform turn
                    if (!board.Soldiers.Contains(x))
                        continue; // soldier already dead

                    if (board.BattleOver())
                    {
                        battleTerminated = true;

                        Log("BattleOverInMidRound");
                  //      PrintBoard(board, round);
                        break; // battle over
                    }

                    if (!x.CanAttack())
                    {
                        var s = x.Move();
//                        if (!string.IsNullOrEmpty(s))
//                            Log(s);
                        //PrintBoard(board, round);
                    }

                    if (x.CanAttack())
                    {
                        var s = x.Attack();
                        //Log(s);
                        var elves = board.Soldiers.Count(s2 => s2.Race == RaceEnum.Elf);
                        if (elves != startelves && elfAttack > 3)
                        {
                            Log("Lost an elf!");
                            return RaceEnum.Goblin;
                        }

                    }
                }

                if (!battleTerminated)
                {
                    round++;
//                    PrintBoard(board, round);
                }
            }
            PrintBoard(board, round);
            var sum = board.Soldiers.Sum(s => s.HP);
            var res = sum * round;
            if (answer != 0 && answer != res)
                Log($"Error:  Rounds {round} score: {sum} Total: {res} Expected {answer} Diff: {answer-res}");
            else
                Log($"OK: Rounds {round} score: {sum} Total: {res}");
            if (Part1 == null)
                Part1 = res;

            var lastmanstanding = board.Soldiers.First().Race;
            if (lastmanstanding == RaceEnum.Elf)
                Part2 = res;
            return lastmanstanding;

        }

        private void PrintBoard(Board board, int round)
        {
            var s = new StringBuilder();
            s.AppendLine("===");
            s.AppendLine("After " + round + " rounds");
            for (int row = 0; row < board.Rows; row++)
            {
                var scores = "";
                for (int col = 0; col < board.Cols; col++)
                {
                    if (board.Walls[row][col])
                        s.Append('#');
                    else
                    {
                        var sold = board.Soldiers.FirstOrDefault(s2 => s2.Row == row && s2.Col == col);
                        if (sold != null)
                        {
                            var r = sold.Race == RaceEnum.Elf ? 'E' : 'G';
                            s.Append(r);
                            scores += r + "(" + sold.HP + "), ";
                        }
                        else s.Append('.');
                    }

                }

                s.AppendLine("  " + scores);
            }

            s.AppendLine();
            Log(s.ToString());
        }

        private Board ParseBoard(List<string> strboard, int elfattack)
        {
            var res = new Board(strboard.Count, strboard[0].Length);

            for (int row = 0; row < res.Rows; row++)
                for (int col = 0; col < res.Cols; col++)
                {
                    if (strboard[row][col] == '#')
                        res.Walls[row][col] = true;
                    else if (strboard[row][col] == 'G')
                        res.Soldiers.Add(new Soldier(RaceEnum.Goblin, row, col, res, 3));
                    else if (strboard[row][col] == 'E')
                        res.Soldiers.Add(new Soldier(RaceEnum.Elf, row, col, res, elfattack));
                }

            return res;
        }
    }

    internal class Board
    {
        public int Rows { get; }
        public int Cols { get; }

        public Board(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            Soldiers = new List<Soldier>();
            Walls = BaseDay.EmptyArr<bool>(rows, cols, true);
        }


        public bool BattleOver()
        {
            return Soldiers.GroupBy(s => s.Race).Count() == 1;
        }

        public List<Soldier> Soldiers { get; }
        public int CompletedRounds { get; set; }
        public bool[][] Walls { get; private set; }

        public object ReadingPos(Tuple<int, int> tuple)
        {
            return tuple.Item1 * Cols + tuple.Item2;
        }
    }

    internal class Soldier
    {
        private readonly Board _b;

        public Soldier(RaceEnum race, int row, int col, Board b, int attackPower)
        {
            _b = b;
            Race = race;
            Row = row;
            Col = col;
            AttackPower = attackPower;
            HP = 200;
        }

        public RaceEnum Race { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
        public int HP { get; set; }
        public int ReadingOrder => Row * _b.Cols + Col;

        public bool CanAttack()
        {
            return _b.Soldiers.Where(s => s.Race != Race).Any(IsAdjacent);
        }

        private bool IsAdjacent(Soldier soldier)
        {
            return Math.Abs(soldier.Row - Row) + Math.Abs(soldier.Col - Col) == 1;
        }

        public string Attack()
        {
            var adjacentEnemies = _b.Soldiers.Where(s => s.Race != Race && IsAdjacent(s)).ToList();
            var target = adjacentEnemies.OrderBy(s => s.HP).ThenBy(s => s.ReadingOrder).First();
            var res = $"{Race} ({Row},{Col}) attacks on {target.Row},{target.Col}, reducing HP from {target.HP} to {target.HP-AttackPower}";
            target.HP -= AttackPower;

            if (target.HP <= 0)
            {
                _b.Soldiers.Remove(target);
                res += " >> dead!";
            }

            return res;
        }

        public int AttackPower { get; set; }

        public string Move()
        {
            var start = new Path(Row, Col, new Path[0], _b);
            var paths = new List<Path> { start };
            var pos = 0;
            var moreFound = true;
            var dirs = new Tuple<int, int>[]
            {
                new Tuple<int, int>(-1, 0),
                new Tuple<int, int>(0, -1),
                new Tuple<int, int>(0, +1),
                new Tuple<int, int>(+1, 0),
            };

            var inRange = new List<Tuple<int, int>>();
            foreach (var enemy in _b.Soldiers.Where(s => !SameRace(s)))
            {
                foreach (var d in dirs)
                    inRange.Add(new Tuple<int, int>(enemy.Row + d.Item1, enemy.Col + d.Item2));
            }

            inRange = inRange.Distinct().Where(r=>!_b.Walls[r.Item1][r.Item2] && !_b.Soldiers.Any(s=>s.Row == r.Item1 && s.Col ==r.Item2)).ToList();

            // Search from self
            int[][] distances = new int[_b.Rows][];
            {
                for (int row = 0; row < _b.Rows; row++)
                {
                    distances[row] = new int[_b.Cols];
                    for (int col = 0; col < _b.Cols; col++)
                        distances[row][col] = int.MaxValue;
                }

                distances[Row][Col] = 0;

                var dist = 0;
                while (moreFound)
                {
                    moreFound = false;
                    for (int row = 0; row < _b.Rows; row++)
                    {
                        for (int col = 0; col < _b.Cols; col++)
                            if (distances[row][col] == dist)
                            {
                                foreach (var d in dirs)
                                {
                                    var newRow = row + d.Item1;
                                    var newCol = col + d.Item2;
                                    if (distances[newRow][newCol] <= dist + 1)
                                        continue; // already found a better path
                                    if (_b.Walls[newRow][newCol])
                                    {
                                        continue; // wall blocking
                                    }

                                    if (_b.Soldiers.Any(s => s.Col == newCol && s.Row == newRow))
                                        continue; // soldier blocking
                                    distances[newRow][newCol] = dist + 1;
                                    moreFound = true;
                                }

                            }
                    }
                    dist++;
                }

            }
            if (!inRange.Any())
            {
                return $"{Race} ({Row},{Col}) No square In Range";
            }

            var reachable = inRange.Where(x => distances[x.Item1][x.Item2] != int.MaxValue).ToList();
            if (!reachable.Any())
            {
                return $"{Race} ({Row},{Col}) No reachable squares";
            }
            var selectedSquare = reachable.OrderBy(x=>distances[x.Item1][x.Item2]).ThenBy(x => _b.ReadingPos(x)).ToList().FirstOrDefault();

            // search from target square

            int[][] distances2 = new int[_b.Rows][];
            {
                for (int row = 0; row < _b.Rows; row++)
                {
                    distances2[row] = new int[_b.Cols];
                    for (int col = 0; col < _b.Cols; col++)
                        distances2[row][col] = int.MaxValue;
                }

                //                //          var candidates = new List<Tuple<int, int>>();
                //                foreach (var enemy in _b.Soldiers.Where(s => !SameRace(s)))
                //                {
                distances2[selectedSquare.Item1][selectedSquare.Item2] = 0;
                //                    //                candidates.Add(new Tuple<int, int>(enemy.Row, col));
                //                }

                var dist = 0;
                moreFound = true;
                while (moreFound)
                {
                    moreFound = false;
                    for (int row = 0; row < _b.Rows; row++)
                    {
                        for (int col = 0; col < _b.Cols; col++)
                            if (distances2[row][col] == dist)
                            {
                                foreach (var d in dirs)
                                {
                                    var newRow = row + d.Item1;
                                    var newCol = col + d.Item2;
                                    if (distances2[newRow][newCol] <= dist + 1)
                                        continue; // already found a better path
                                    if (_b.Walls[newRow][newCol])
                                    {
                                        continue; // wall blocking
                                    }

                                    if (_b.Soldiers.Any(s => s.Col == newCol && s.Row == newRow))
                                        continue; // soldier blocking
                                    distances2[newRow][newCol] = dist + 1;
                                    moreFound = true;
                                }

                            }
                    }

                    dist++;
                }
            }

            var bestDir = new Tuple<int, int>(0, 0);
            var bestDist = int.MaxValue;
            foreach (var d in dirs)
            {
                var tempDist = distances2[Row + d.Item1][Col + d.Item2];
                if (tempDist < bestDist)
                {
                    bestDir = d;
                    bestDist = tempDist;
                }
            }

            if (bestDist != int.MaxValue)
            {
                string s = $"{Race} ({Row},{Col}) => ";

                Row = Row + bestDir.Item1;
                Col = Col + bestDir.Item2;
                s += $"{Row},{Col}";
                return s;
            }

            return $"{Race} ({Row},{Col}) can't move";
        }

        private bool SameRace(Soldier soldier)
        {
            return this.Race == soldier.Race;
        }

        internal class Path
        {
            public int Row { get; }
            public int Col { get; }
            public IEnumerable<Path> Trail { get; }
            public int ReadingOrder { get; }

            public Path(int row, int col, IEnumerable<Path> trail, Board b)
            {
                Row = row;
                Col = col;
                Trail = trail;
                ReadingOrder = row * b.Cols + col;
            }
        }

    }
    internal enum RaceEnum
    {
        Goblin,
        Elf
    }

}