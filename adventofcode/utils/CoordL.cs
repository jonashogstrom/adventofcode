using System;
using System.Collections.Generic;

namespace AdventofCode.Utils
{
    public class CoordL
    {
        public static CoordL N = new CoordL(-1, 0);
        public static CoordL E = new CoordL(0, 1);
        public static CoordL S = new CoordL(1, 0);
        public static CoordL W = new CoordL(0, -1);
        public static CoordL Origin = new CoordL(0, 0);
        public static CoordL[] NSWE = new[] { N, S, W, E };

        public static CoordL NW = N.Move(W);
        public static CoordL NE = N.Move(E);
        public static CoordL SE = S.Move(E);
        public static CoordL SW = S.Move(W);

        public static readonly CoordL[] Directions4 = { N, E, S, W };
        public static readonly CoordL[] Directions8 = { N, NE, E, SE, S, SW, W, NW };
        public static readonly HexDirection[] HexNeighbors =
        {
            HexDirection.sw, HexDirection.w, HexDirection.nw,
            HexDirection.ne, HexDirection.e, HexDirection.se,
        };

        public static readonly Dictionary<CoordL, char> trans2NESW = new Dictionary<CoordL, char>()
        {
            {N, 'N'}, {S, 'S'}, {E, 'E'}, {W, 'W'},
        };
        public static readonly Dictionary<CoordL, char> trans2Arrow = new Dictionary<CoordL, char>()
        {
            {N, 'v'}, {S, '^'}, {E, '>'}, {W, '<'},
        };
        public static readonly Dictionary<char, CoordL> trans2Coord = new Dictionary<char, CoordL>()
        {
            {'N', N}, {'S', S}, {'E', E}, {'W', W},
            {'^', N}, {'v', S}, {'>', E}, {'<', W},
            {'U', N}, {'D', S}, {'R', E}, {'L', W},
        };

        public long Row { get; set; }
        public long Col { get; set; }

        public long X => Col;
        public long Y => Row;

        public CoordL(long row, long col)
        {
            CoordCounter++;
            Row = row;
            Col = col;
        }

        public static long CoordCounter;

        public static CoordL FromXY(long x, long y)
        {
            return new CoordL(y, x);
        }

        private int GetDir(long v1, long v2)
        {
            return v1.CompareTo(v2);
        }

        public IEnumerable<CoordL> PathTo(CoordL c2, bool inclusive = true)
        {
            var rDir = c2.Row.CompareTo(Row);
            var cDir = c2.Col.CompareTo(Col);
            //var res = new List<CoordL>();

            var rDist = Math.Abs(Row - c2.Row);
            var cDist = Math.Abs(Col - c2.Col);
            if (rDist != 0 && cDist != 0 && rDist != cDist)
                throw new Exception($"Path is not a multiple of 45 degrees! {this} => {c2} (rDist: {rDist}, cDist: {cDist})");
            var steps = Math.Max(rDist, cDist) + 1;
            for (int s = 0; s < steps; s++)
            {
                if (inclusive || (s > 1 && s < steps - 1))
                {
                    var c = Col + s * cDir;
                    var r = Row + s * rDir;
                    yield return new CoordL(r, c);
                }
            }
        }

        /// <summary>
        /// Sample CoordL strings supported: "x, y", "(x y)", "x;y"
        /// Supported caracters: (, ;)
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static CoordL Parse(string s)
        {
            var parts = s.Split(new[] { '(', ',', ' ', ';', ')' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                throw new Exception("Invalid CoordL string: " + s);
            return new CoordL(long.Parse(parts[1]), long.Parse(parts[0]));
        }

        private bool Equals(CoordL other)
        {
            return Row == other.Row && Col == other.Col;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CoordL)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (int)((Row * 397) ^ Col);
            }
        }

        public static CoordL CharToDir(char c)
        {
            return trans2Coord[c];
        }

        public char DirToNESW()
        {
            return trans2NESW[this];
        }

        public CoordL RotateCCW90()
        {
            return new CoordL(-Col, Row);
        }
        public CoordL RotateCCWDegrees(int degrees)
        {
            if (degrees % 90 != 0)
                throw new Exception("can only rotate even 90 degrees");
            var res = this;
            for (var i = 0; i < degrees / 90; i++)
                res = res.RotateCCW90();
            return res;
        }

        public CoordL RotateCWDegrees(int degrees)
        {
            if (degrees % 90 != 0)
                throw new Exception("can only rotate even 90 degrees");
            var res = this;
            for (var i = 0; i < degrees / 90; i++)
                res = res.RotateCW90();
            return res;
        }

        public CoordL RotateCW90()
        {
            return new CoordL(Col, -Row);
        }

        public IEnumerable<CoordL> GenAdjacent4()
        {
            foreach (var d in Directions4)
                yield return Move(d);
        }

        public IEnumerable<CoordL> GenAdjacent8()
        {
            foreach (var d in Directions8)
                yield return Move(d);
        }

        public CoordL Move(CoordL CoordL, int count = 1)
        {
            return new CoordL(Row + CoordL.Row * count, Col + CoordL.Col * count);
        }

        public CoordL HexMove(HexDirection hexDirection)
        {
            if (hexDirection == HexDirection.e)
                return Move(E);
            if (hexDirection == HexDirection.w)
                return Move(W);

            if (Y % 2 == 0)
            {
                switch (hexDirection)
                {
                    case HexDirection.nw:
                        return Move(N);
                    case HexDirection.ne:
                        return Move(NE);
                    case HexDirection.sw:
                        return Move(S);
                    case HexDirection.se:
                        return Move(SE);
                    default:
                        throw new Exception();
                }

            }
            else
            {
                switch (hexDirection)
                {
                    case HexDirection.nw:
                        return Move(NW);
                    case HexDirection.ne:
                        return Move(N);
                    case HexDirection.sw:
                        return Move(SW);
                    case HexDirection.se:
                        return Move(S);
                    default:
                        throw new Exception();
                }
            }
        }

        public long Dist(CoordL pos)
        {
            return Math.Abs(Row - pos.Row) + Math.Abs(Col - pos.Col);
        }

        public override string ToString()
        {
            return $"{nameof(Row)}: {Row}, {nameof(Col)}: {Col}";
        }
    }
}