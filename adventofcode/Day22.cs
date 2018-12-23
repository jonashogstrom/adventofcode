using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace adventofcode
{
    internal class Day22 : BaseDay
    {
        private int _maxRow;
        private int _maxCol;

        protected override void Setup()
        {
            Source = InputSource.test;
                        Source = InputSource.prod;

            LogLevel = UseTestData ? 5 : 0;

            Part1TestSolution = 114L;
            Part2TestSolution = 45;
            Part1Solution = 11972L;
            Part2Solution = 1092;
        }

        protected override void DoRun(string[] input)
        {
            var depth = GetInts(input[0]).First();
            var x = GetInts(input[1]);
            var targetCoord = new Coord(x.Last(), x.First());
            var rows = targetCoord.Row * 3;
            var cols = targetCoord.Col * 3;
            var ground = EmptyArr<Region>(rows, cols);
            for (var row = 0; row < rows; row++)
                for (var col = 0; col < cols; col++)
                    ground[row][col] = new Region(new Coord(row, col), ground, depth);
            long sumRisk = 0;
            var targetCell = ground[targetCoord.Row][targetCoord.Col];

            targetCell._geologicIndex = 0;
            targetCell._erosionLevel = null;
            targetCell._risk = null;
            targetCell._type = null;


            var initRegion = ground[0][0];

            initRegion.TimeToReach[Equip.Torch] = 0;
            initRegion.TimeToReach[Equip.Gear] = 7;
            initRegion.ShortestPath = 0;

            for (var row = 0; row <= targetCoord.Row; row++)
                for (var col = 0; col <= targetCoord.Col; col++)
                    sumRisk += ground[row][col].RiskIndex;
            Part1 = sumRisk;

            if (LogLevel > 2)
                PrintGround(ground, 15);

            ExpandDistances(ground, initRegion, targetCoord, rows, cols);
            if (LogLevel > 2)
                PrintGround(ground);
            var timeToReach = targetCell.TimeToReach[Equip.Torch];
            Part2 = timeToReach;

            var gear = Equip.Torch;
            var cell = targetCell;
            if (LogLevel > 2)
                PrintPath(cell, gear);
        }

        private void PrintPath(Region cell, Equip gear)
        {
            var path = new List<string>();

            while (cell.Camefrom.ContainsKey(gear))
            {
                var camef = cell.Camefrom[gear];
                path.Add($"{camef.Item1.Coord.Col,4},{camef.Item1.Coord.Row,4} ({camef.Item1.Type,6}, {camef.Item2,7}) =>" +
                         $" {cell.Coord.Col,4},{cell.Coord.Row,4} ({cell.Type,6}, {gear,7}) Time: {cell.TimeToReach[gear]} min");
                gear = camef.Item2;
                cell = camef.Item1;
            }

            path.Reverse();
            foreach (var p in path)
                Log(p);
        }

        private void PrintGround(Region[][] ground)
        {
            var gt = new Dictionary<GroundType, char>()
            {
                {GroundType.Narrow, '|'},
                {GroundType.Rocky, '.'},
                {GroundType.Wet, '='},
            };
            var sb = new StringBuilder();
            for (var row = 0; row <= _maxRow; row++)
            {
                for (var col = 0; col <= _maxCol; col++)
                {
                    var g = ground[row][col];
                    sb.Append($" [({row.ToString().PadLeft(4)},{col.ToString().PadLeft(4)})" +
                        gt[g.Type] +

                              " T=" + DistToStr(g, Equip.Torch).PadLeft(4) +
                              " G=" + DistToStr(g, Equip.Gear).PadLeft(4) +
                              " N=" + DistToStr(g, Equip.Neither).PadLeft(4) +
                              "]");
                }
                sb.AppendLine();
            }
            Log(sb.ToString);
        }

        private void PrintGround(Region[][] ground, int size)
        {
            var gt = new Dictionary<GroundType, char>()
            {
                {GroundType.Narrow, '|'},
                {GroundType.Rocky, '.'},
                {GroundType.Wet, '='},
            };
            var sb = new StringBuilder();
            for (var row = 0; row <= size; row++)
            {
                for (var col = 0; col <= size; col++)
                {
                    var g = ground[row][col];
                    sb.Append(gt[g.Type]);
                }
                sb.AppendLine();
            }
            Log(sb.ToString);
        }

        private static string DistToStr(Region g, Equip e)
        {
            var res = g.TimeToReach[e];
            if (res > 50000)
                return "";
            return res.ToString();
        }

        private void ExpandDistances(Region[][] regions, Region initRegion, Coord target, int rows, int cols)
        {
            _maxCol = 0;
            _maxRow = 0;
            var bestTarget = int.MaxValue;
            var candidates = new List<Region> { initRegion };
            var gen = 0;
            while (candidates.Any())
            {
                var c = candidates.First();
                Log(() => "Gen: " + gen++ + " " + candidates.Count + " candidates, currentPathLength = " + c.ShortestPath, 2);

                candidates.RemoveAt(0);
                if (gen % 10 == 0)
                    candidates = candidates.OrderBy(x => x.ShortestPath).ToList();
                _maxCol = Math.Max(_maxCol, c.Coord.Col);
                _maxRow = Math.Max(_maxRow, c.Coord.Row);
                var r = c;
                foreach (var d in c.Coord.GenAdjacent4())
                {
                    if (d.Row >= 0 && d.Col >= 0)
                    {
                        if (d.Row < rows && d.Col < cols)
                        {
                            var neighbor = regions[d.Row][d.Col];

                            var newEQ = GetValidEquip(neighbor);
                            var oldEQ = GetValidEquip(r);
                            var eqs = newEQ.Intersect(oldEQ).ToList();
                            foreach (var eq in eqs)
                            {
                                bestTarget = TryMove(target, r.TimeToReach[eq] + 1, neighbor, eq, r, eq, bestTarget,
                                    candidates);
                            }
                        }
                    }
                }
            }
        }

        private static Equip[] GetValidEquip(Region r)
        {
            if (r.Type == GroundType.Rocky)
            {
                return new[] { Equip.Torch, Equip.Gear };
            }
            if (r.Type == GroundType.Wet)
            {
                return new[] { Equip.Neither, Equip.Gear };
            }
            return new[] { Equip.Neither, Equip.Torch };
        }

        private static Equip GetInvalidEquip(Region r)
        {
            if (r.Type == GroundType.Rocky)
            {
                return Equip.Neither;
            }
            if (r.Type == GroundType.Wet)
            {
                return Equip.Torch;
            }
            return Equip.Gear;
        }

        private int TryMove(Coord target, int timeToReach, Region neighbor, Equip newEquipment, Region r, Equip oldEquip, int bestTarget,
            List<Region> newCandidates)
        {
            if (timeToReach < neighbor.TimeToReach[newEquipment])
            {
                neighbor.TimeToReach[newEquipment] = timeToReach;
                neighbor.Camefrom[newEquipment] = new Tuple<Region, Equip>(r, oldEquip);

                if (neighbor.Coord.Equals(target) && timeToReach < bestTarget)
                {
                    if (newEquipment == Equip.Torch)
                        bestTarget = timeToReach;
                    else
                        bestTarget = timeToReach + 7;

                    Log($"Reached {neighbor.Coord.Row},{neighbor.Coord.Col} after time " + bestTarget);
                }

                var otherEquip = GetValidEquip(neighbor).First(x => x != newEquipment);
                if (neighbor.TimeToReach[otherEquip] > timeToReach + 7)
                {
                    neighbor.TimeToReach[otherEquip] = timeToReach + 7;
                    neighbor.Camefrom[otherEquip] = new Tuple<Region, Equip>(neighbor, newEquipment);
                }

                if (timeToReach < neighbor.ShortestPath)
                    neighbor.ShortestPath = timeToReach;

                if (timeToReach <= bestTarget && !newCandidates.Contains(neighbor))
                    newCandidates.Add(neighbor);
            }

            return bestTarget;
        }

        private int Min(int i, int i1, int i2)
        {
            return Math.Min(i, Math.Min(i1, i2));
        }
    }

    internal class Region
    {
        private readonly Region[][] _ground;
        private readonly int _depth;
        public long? _risk;
        public long? _geologicIndex;
        public long? _erosionLevel;
        public GroundType? _type;


        public Region(Coord coord, Region[][] ground, int depth)
        {
            _ground = ground;
            _depth = depth;
            Coord = coord;
            TimeToReach = new Dictionary<Equip, int>()
            {
                {Equip.Neither, int.MaxValue/2},
                {Equip.Torch, int.MaxValue/2},
                {Equip.Gear, int.MaxValue/2},
            };
            ShortestPath = int.MaxValue;
            Camefrom = new Dictionary<Equip, Tuple<Region, Equip>>();
        }

        public Dictionary<Equip, Tuple<Region, Equip>> Camefrom { get; }

        public Coord Coord { get; }
        public GroundType Type
        {
            get
            {
                if (!_type.HasValue)
                {
                    _type = (GroundType)(Erosion % 3);
                }

                return _type.Value;
            }
        }

        public long RiskIndex => (int)Type;

        public long Erosion
        {
            get
            {
                if (!_erosionLevel.HasValue)
                {
                    _erosionLevel = (GeologicIndex + _depth) % 20183;
                }
                return _erosionLevel.Value;
            }
        }

        public long GeologicIndex
        {
            get
            {
                if (!_geologicIndex.HasValue)
                {
                    if (Coord.Row == 0 && Coord.Col == 0)
                        _geologicIndex = 0;
                    else if (Coord.Row == 0)
                        _geologicIndex = Coord.Col * 16807;
                    else if (Coord.Col == 0)
                        _geologicIndex = Coord.Row * 48271;
                    else
                    {
                        _geologicIndex = _ground[Coord.Row - 1][Coord.Col].Erosion *
                                         _ground[Coord.Row][Coord.Col - 1].Erosion;
                    }
                }

                return _geologicIndex.Value;
            }
        }

        public Dictionary<Equip, int> TimeToReach { get; }

        public long ShortestPath { get; set; }
    }

    internal enum GroundType
    {
        Rocky, Wet, Narrow
    }

    internal enum Equip
    {
        Neither, Torch, Gear
    }
}