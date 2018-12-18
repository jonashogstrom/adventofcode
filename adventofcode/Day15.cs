using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace adventofcode
{
    internal class Day15 : BaseDay
    {
        protected override void Setup()
        {
            UseTestData = true;
            UseTestData = false;
            LogLevel = UseTestData ? 5 : 0;
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
                    RunBoardWithAttackIncrease(board, answer);
                }
                else board.Add(row);
            }

            if (board.Count > 0)
            {
                RunBoardWithAttackIncrease(board, answer);
            }
        }

        private void RunBoardWithAttackIncrease(List<string> board, int answer)
        {
            var attack = 3;
            if (UseTestData)
                Part1TestSolution = answer;
            while (RunBoard(board, answer, attack) == RaceEnum.Goblin)
                attack++;
            board.Clear();
        }

        private RaceEnum RunBoard(List<string> input, int answer, int elfAttack)
        {
            Log(() => "Running board with elfAttack power " + elfAttack, 2);
            var board = ParseBoard(input, elfAttack);
            var round = 0;
            PrintBoard(board, round);
            var battleTerminated = false;
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

                        Log(() => "Battle terminated in the middle of the round", 2);
                        break; // battle over
                    }

                    if (!x.CanAttack())
                    {
                        var moved = x.Move();
                        if (LogLevel >= 4 && moved)
                        {
                            PrintBoard(board, round);
                        }
                    }

                    if (x.CanAttack())
                    {
                        var kill = x.Attack();

                        if (x.Race == RaceEnum.Goblin && kill && elfAttack > 3)
                        {
                            Log("Lost an elf!", 2);
                            return RaceEnum.Goblin;
                        }

                    }
                }

                if (!battleTerminated)
                {
                    round++;
                    if (LogLevel >= 3)
                    {
                        Log("***************************vvvvvvvvvvvvvvvvvv***************************");
                        PrintBoard(board, round);
                        Log("***************************^^^^^^^^^^^^^^^^^^***************************");
                    }
                }
            }

            PrintBoard(board, round);
            var sum = board.Soldiers.Sum(s => s.HP);
            var res = sum * round;
            if (elfAttack == 3)
            {
                if (answer != 0 && answer != res)
                    Log(() => $"Error:  Rounds {round} score: {sum} Total: {res} Expected {answer} Diff: {answer - res}", 1);
                else
                    Log(() => $"OK: Rounds {round} score: {sum} Total: {res}", 1);
                if (Part1 == null)
                    Part1 = res;
            }

            var lastmanstanding = board.Soldiers.First().Race;
            if (lastmanstanding == RaceEnum.Elf)
            {
                Part2 = res;
                Log(() => $"Elves wins without casualties at {elfAttack} attack points", 1);
            }
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
            Log(() => s.ToString(), 2);
        }

        private Board ParseBoard(List<string> strboard, int elfattack)
        {
            var board = new Board(strboard.Count, strboard[0].Length, this);

            for (int row = 0; row < board.Rows; row++)
                for (int col = 0; col < board.Cols; col++)
                {
                    if (strboard[row][col] == '#')
                        board.Walls[row][col] = true;
                    else if (strboard[row][col] == 'G')
                        board.Soldiers.Add(new Soldier(RaceEnum.Goblin, row, col, board, 3, this));
                    else if (strboard[row][col] == 'E')
                        board.Soldiers.Add(new Soldier(RaceEnum.Elf, row, col, board, elfattack, this));
                }

            return board;
        }
    }

    internal class Board
    {
        private readonly BaseDay _d;
        public int Rows { get; }
        public int Cols { get; }

        public Board(int rows, int cols, BaseDay d)
        {
            _d = d;
            Rows = rows;
            Cols = cols;
            Soldiers = new List<Soldier>();
            Walls = BaseDay.EmptyArr<bool>(rows, cols);
        }


        public bool BattleOver()
        {
            return Soldiers.GroupBy(s => s.Race).Count() == 1;
        }

        public List<Soldier> Soldiers { get; }
        public bool[][] Walls { get; }

        public object ReadingPos(Coord tuple)
        {
            return tuple.Row * Cols + tuple.Col;
        }
    }

    [DebuggerDisplay("{Race} ({Row},{Col}), HP: {HP} (attack: {AttackPower})")]
    internal class Soldier
    {
        private readonly Board _b;
        private readonly BaseDay _d;
        private readonly Coord[] _dirs = {
            new Coord(-1, 0),
            new Coord(0, -1),
            new Coord(0, +1),
            new Coord(+1, 0),
        };

        public Soldier(RaceEnum race, int row, int col, Board b, int attackPower, BaseDay d)
        {
            _b = b;
            _d = d;
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
            var enemies = _b.Soldiers.Where(s => s.Race != Race).ToList();
            var close = enemies.Where(IsAdjacent).ToList();
            return close.Any();
        }

        private bool IsAdjacent(Soldier soldier)
        {
            return Math.Abs(soldier.Row - Row) + Math.Abs(soldier.Col - Col) == 1;
        }

        public bool Attack()
        {
            var adjacentEnemies = _b.Soldiers.Where(s => s.Race != Race && IsAdjacent(s)).ToList();
            var target = adjacentEnemies.OrderBy(s => s.HP).ThenBy(s => s.ReadingOrder).First();
            var oldHP = HP;
            target.HP -= AttackPower;
            var dead = "";
            var res = false;
            if (target.HP <= 0)
            {
                _b.Soldiers.Remove(target);
                dead = ">> DEAD!";
                res = true;
            }
            _d.Log(() => $"{Race} ({Row},{Col}) attacks on {target.Row},{target.Col}, reducing HP from {oldHP} to {target.HP} {dead}", 5);
            return res;

        }

        public int AttackPower { get; set; }

        public bool Move()
        {
            var inRange = new HashSet<Coord>();
            foreach (var enemy in _b.Soldiers.Where(s => !SameRace(s)))
            {
                foreach (var d in _dirs)
                {
                    inRange.Add(new Coord(enemy.Row + d.Row, enemy.Col + d.Col));
                }
            }

            inRange = new HashSet<Coord>(inRange.Where(r => !_b.Walls[r.Row][r.Col] && !_b.Soldiers.Any(s => s.Row == r.Row && s.Col == r.Col)));

            if (!inRange.Any())
            {
                _d.Log(() => $"{Race} ({Row},{Col}) No square In Range", 5);
                return false;
            }

            var distances = BaseDay.EmptyArr(_b.Rows, _b.Cols, int.MaxValue);
            ExpandDistances(distances, new[] { new Coord(Row, Col) }, inRange);

            var reachable = inRange.Where(x => distances[x.Row][x.Col] != int.MaxValue).ToList();
            if (!reachable.Any())
            {
                _d.Log(() => $"{Race} ({Row},{Col}) No reachable squares", 5);
                return false;
            }
            var selectedSquare = reachable.OrderBy(x => distances[x.Row][x.Col]).ThenBy(x => _b.ReadingPos(x)).ToList().FirstOrDefault();

            // search from target square

            var distances2 = BaseDay.EmptyArr(_b.Rows, _b.Cols, int.MaxValue);
            var myNeighbours = new HashSet<Coord>();
            foreach (var d in _dirs)
                myNeighbours.Add(new Coord(Row + d.Row, Col + d.Col));

            ExpandDistances(distances2, new[] { new Coord(selectedSquare.Row, selectedSquare.Col) }, myNeighbours);

            var bestDir = new Coord(0, 0);
            var bestDist = int.MaxValue;
            foreach (var d in _dirs)
            {
                var tempDist = distances2[Row + d.Row][Col + d.Col];
                if (tempDist < bestDist)
                {
                    bestDir = d;
                    bestDist = tempDist;
                }
            }

            if (bestDist != int.MaxValue)
            {
                _d.Log(() => $"{Race} ({Row},{Col}) => {Row + bestDir.Row},{Col + bestDir.Col}", 5);

                Row = Row + bestDir.Row;
                Col = Col + bestDir.Col;
                return true;
            }

            _d.Log(() => $"{Race} ({Row},{Col}) can't move", 4);
            return false;
        }

        /// <summary>
        /// Flood fill distances in a breadth first approach, terminate after a full round when any of the target coordinates have been found
        /// </summary>
        private void ExpandDistances(int[][] distances, IEnumerable<Coord> initCoords, HashSet<Coord> targetCoords)
        {
            var dist = 0;
            var candidates = initCoords.ToList();
            while (candidates.Any())
            {
                var foundTarget = false;
                var newCandidates = new List<Coord>();
                foreach (var c in candidates)
                {
                    if (targetCoords.Contains(c))
                        foundTarget = true;
                    distances[c.Row][c.Col] = dist;
                    if (!foundTarget)
                    {
                        foreach (var d in _dirs)
                        {
                            var newRow = c.Row + d.Row;
                            var newCol = c.Col + d.Col;
                            if (distances[newRow][newCol] <= dist)
                                continue; // already found a better or equal path
                            if (_b.Walls[newRow][newCol])
                            {
                                continue; // wall blocking
                            }

                            if (_b.Soldiers.Any(s => s.Col == newCol && s.Row == newRow))
                                continue; // soldier blocking
                            newCandidates.Add(new Coord(newRow, newCol));
                        }
                    }
                }

                candidates = newCandidates.Distinct().ToList();

                dist++;
            }
        }

        private bool SameRace(Soldier soldier)
        {
            return Race == soldier.Race;
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