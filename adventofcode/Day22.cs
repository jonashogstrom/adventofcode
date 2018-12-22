using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace adventofcode
{
    internal class Day22 : BaseDay
    {
        protected override void Setup()
        {
            Source = InputSource.test;
               Source = InputSource.prod;

            LogLevel = UseTestData ? 5 : 0;

            Part1TestSolution = 114L;
            Part2TestSolution = null;
            Part1Solution = null;
            Part2Solution = null;
        }

        protected override void DoRun(string[] input)
        {
            var depth = GetInts(input[0]).First();
            var x = GetInts(input[1]);
            var target = new Coord(x.Last(), x.First());
            var ground = EmptyArr<Region>(target.Row + 1, target.Col + 1);
            for (var row = 0; row <= target.Row; row++)
                for (var col = 0; col <= target.Col; col++)
                    ground[row][col] = new Region(new Coord(row, col), ground, depth);
            long sumRisk = 0;
            ground[target.Row][target.Col]._geologicIndex = 0;
            PrintGround(ground, target);
            for (var row = 0; row <= target.Row; row++)
                for (var col = 0; col <= target.Col; col++)
                    sumRisk += ground[row][col].RiskIndex;
            Part1 = sumRisk;
        }

        private void PrintGround(Region[][] ground, Coord target)
        {
            var gt = new Dictionary<GroundType, char>()
            {
                {GroundType.Narrow, '|'},
                {GroundType.Rocky, '.'},
                {GroundType.Wet, '='},
            };
            var sb = new StringBuilder();
            for (var row = 0; row <= target.Row; row++)
            {
                for (var col = 0; col <= target.Col; col++)
                {
                    sb.Append(gt[ground[row][col].Type]);
                }

                sb.AppendLine();
            }
            Log(sb.ToString);

        }
    }

    internal class Region
    {
        private readonly Region[][] _ground;
        private readonly int _depth;
        private long? _risk;
        public long? _geologicIndex;
        private long? _erosionLevel;
        private GroundType? _type;

        public Region(Coord coord, Region[][] ground, int depth)
        {
            _ground = ground;
            _depth = depth;
            Coord = coord;
        }

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

        public long RiskIndex
        {
            get
            {
                return (int)Type;

            }
        }

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


    }

    internal enum GroundType
    {
        Rocky, Wet, Narrow
    }
}