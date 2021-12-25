using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2021
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day23 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(12521, 44169, "Day23_test.txt")]
        [TestCase(11332, 49936, "Day23.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            //LogLevel = resourceName.Contains("test") ? 20 : -1;
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1 = 0;
            Part2Type part2 = 0;
            var sw = Stopwatch.StartNew();
            var part2Source = source.ToList();
            part2Source.Insert(3, "  #D#C#B#A#");
            part2Source.Insert(4, "  #D#B#A#C#");

            var world = part2Source.ToSparseBuffer('#', c => c == ' ' ? '#' : c);
            Log(world.ToString(c => c.ToString()));
            LogAndReset("Parse", sw);
            var pods = new List<Pod>();
            foreach (var c in world.Keys)
            {
                if (world[c] != '.' && world[c] != '#')
                {
                    pods.Add(new Pod(c, world[c]));
                    //                    world[c] = '.';
                }
            }

            part2 = FindSolution(pods);
            LogAndReset("*1", sw);

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private long FindSolution(List<Pod> pods)
        {
            var possibleTargets = new List<Coord>()
            {
                new Coord(1, 1),
                new Coord(1, 2),
                new Coord(1, 4),
                new Coord(1, 6),
                new Coord(1, 8),
                new Coord(1, 10),
                new Coord(1, 11),

                new Coord(2, 3),
                new Coord(2, 5),
                new Coord(2, 7),
                new Coord(2, 9),

                new Coord(3, 3),
                new Coord(3, 5),
                new Coord(3, 7),
                new Coord(3, 9),

                new Coord(4, 3),
                new Coord(4, 5),
                new Coord(4, 7),
                new Coord(4, 9),

                new Coord(5, 3),
                new Coord(5, 5),
                new Coord(5, 7),
                new Coord(5, 9),
            };
            return FindRec(pods, possibleTargets, new String[] { });
        }

        private long _best = long.MaxValue;

        private long FindRec(List<Pod> pods, List<Coord> possibleTargets, IEnumerable<string> moves)
        {
            if (pods.All(p => p.IsHome()))
            {
                var result = pods.Sum(p => p.Cost);
                if (result < _best)
                {
                    _best = result;
                    Log($"Found new solution: TotalCost: {result}");
                    foreach (var m in moves)
                        Log(m);

                }

                return result;
            }

            var best = long.MaxValue;
            for (var i = 0; i < pods.Count; i++)
            {
                var p = pods[i];

                var validMoves = p.ValidMoves(possibleTargets, pods).ToArray();
                foreach (var c in validMoves)
                {
                    var newPods = new List<Pod>(pods);
                    newPods[i] = p.Move(c);
                    var newTotalCost = newPods.Sum(x => x.Cost);
                    var minimalRestCost = p.MinimalRestCost(newPods);
                    if (newTotalCost+ minimalRestCost < _best) // don't continue if we already spend too much
                    {
                        var move = $"Move {p.Name} from {p.Coord.Y};{p.Coord.X} to {c.Y};{c.X}. new Total cost: {newTotalCost}";
                        best = Math.Min(best, FindRec(newPods, possibleTargets, moves.Append(move)));
                    }
                }
            }

            return best;
        }
    }

    internal class Pod
    {
        private readonly int _homeCol;
        private Coord _lowerHomeCoord1;
        private Coord _lowerHomeCoord2;
        private Coord _lowerHomeCoord3;
        public Coord Coord { get; set; }
        public char Name { get; }

        public Pod(Coord coord, char name)
        {
            Coord = coord;
            Name = name;
            var num = (byte)name - (byte)'A';
            CostPerStep = (int)Math.Pow(10, num);
            _homeCol = num * 2 + 3;
            Moves = 0;
            Steps = 0;
            _lowerHomeCoord1 = new Coord(5, _homeCol);
            _lowerHomeCoord2 = new Coord(4, _homeCol);
            _lowerHomeCoord3 = new Coord(3, _homeCol);
        }

        public int MinimalRestCost(List<Pod> pods)
        {
            var res = 0;
            foreach (var p in pods)
                if (!p.IsHome())
                    res += p.StepsToMove(new Coord(2, p._homeCol)) * p.CostPerStep;
            var podsInRightColumn = new Dictionary<char, int>(0);
            podsInRightColumn['A'] = 0;
            podsInRightColumn['B'] = 0;
            podsInRightColumn['C'] = 0;
            podsInRightColumn['D'] = 0;
            foreach (var p in pods)
                if (p.IsHome())
                    podsInRightColumn[p.Name]++;

            var extraCost = 0;
            foreach (var c in podsInRightColumn.Keys)
            {
                var num = (byte)c - (byte)'A';
                var moveCost = (int)Math.Pow(10, num);
                if (podsInRightColumn[c] == 0)
                    extraCost += 6 * moveCost;
                else if (podsInRightColumn[c] == 1)
                    extraCost += 3 * moveCost;
                else if (podsInRightColumn[c] == 2)
                    extraCost += 1 * moveCost;
            }
            
            return res+extraCost;
        }

        private int StepsToMove(Coord target)
        {
            return (Math.Abs(Coord.X - target.X) + (Coord.Y - 1) + target.Y - 1);
        }

        public IEnumerable<Coord> ValidMoves(List<Coord> possibleTargets, List<Pod> pods)
        {
            if (IsHome())
            {
                var _allHome = true;
                for (var r = Coord.Row; r <= 6; r++)
                {
                    _allHome = _allHome && pods.Any(p => p.Coord.Col == _homeCol && p.Coord.Row == r);
                }
                // no need to move anymore
                if (_allHome)
                    yield break;
            }
                
            if (CanMoveTo(_lowerHomeCoord1, pods))
            {
                yield return _lowerHomeCoord1;
            }
            else if (CanMoveTo(_lowerHomeCoord2, pods))
            {
                yield return _lowerHomeCoord2;
            }
            if (CanMoveTo(_lowerHomeCoord3, pods))
            {
                yield return _lowerHomeCoord3;
            }
            else
            {

                foreach (var c in possibleTargets)
                {
                    if (Moves == 0)
                    {
                        if (c.Row == 1 && CanMoveTo(c, pods))
                            yield return c;
                    }
                    else if (Moves == 1)
                    {
                        // move home is the only valid move
                        if (c.Col == _homeCol && CanMoveTo(c, pods))
                        {
                            yield return c;
                        }
                    }
                    // no pod can move more than two times
                }
            }
        }

        private bool CanMoveTo(Coord target, List<Pod> pods)
        {
            var maxT = Math.Max(Coord.X, target.X);
            var minT = Math.Min(Coord.X, target.X);
            foreach (var p in pods)
            {
                if (p.Coord.Equals(target))
                    return false; // coord is already occupied
                if (p.Coord.Row > target.Row && p.Coord.Col == target.Col && p.Name != Name)
                    return false; // mismatched pod below target - locked in
                if (p.Coord.Row == 1 && p.Coord.X > minT && p.Coord.X < maxT)
                    return false; // some other pod blocking the way in the coorridor
                if (p.Coord.X == target.X && p.Coord.Y < target.Y)
                    return false; // blocking the entrance to the room
                if (p.Coord.X == Coord.X && p.Coord.Y < Coord.Y)
                    return false; // blocking the exit
            }
            return true;
        }

        public Pod Move(Coord target)
        {
            var steps = StepsToMove(target);
            return new Pod(target, Name)
            {
                Moves = Moves + 1,
                Steps = Steps + steps
            };
        }

        public bool IsHome()
        {
            return Coord.X == _homeCol && Coord.Y != 1;
        }

        public override string ToString()
        {
            var isHomeRes = IsHome() ? 'X' : ' ';
            return $"{isHomeRes} {Name}: {Coord} (m: {Moves} s:{Steps}) ";
        }

        public int Steps { get; set; }

        public int Cost => Steps * CostPerStep;

        public int Moves { get; set; }

        public int CostPerStep { get; set; }
    }
}