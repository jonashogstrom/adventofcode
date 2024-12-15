using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AdventofCode
{
    [DebuggerDisplay("({Row},{Col})")]
    public struct CoordStr
    {
        public static CoordStr N = new CoordStr(-1, 0);
        public static CoordStr E = new CoordStr(0, 1);
        public static CoordStr S = new CoordStr(1, 0);
        public static CoordStr W = new CoordStr(0, -1);
        public static CoordStr Origin = new CoordStr(0, 0);
        public static CoordStr[] NSWE = new[] { N, S, W, E };

        public static CoordStr NW = N.Move(W);
        public static CoordStr NE = N.Move(E);
        public static CoordStr SE = S.Move(E);
        public static CoordStr SW = S.Move(W);

        public static readonly CoordStr[] Directions4 = { N, E, S, W };
        public static readonly CoordStr[] Directions8 = { N, NE, E, SE, S, SW, W, NW };

        public static readonly HexDirection[] HexNeighbors =
        {
            HexDirection.sw, HexDirection.w, HexDirection.nw,
            HexDirection.ne, HexDirection.e, HexDirection.se,
        };

        public static readonly Dictionary<CoordStr, char> trans2NESW = new Dictionary<CoordStr, char>()
        {
            { N, 'N' }, { S, 'S' }, { E, 'E' }, { W, 'W' },
        };

        public static readonly Dictionary<CoordStr, string> trans28Dir = new Dictionary<CoordStr, string>()
        {
            { N, "N" }, { S, "S" }, { E, "E" }, { W, "W" },
            { NE, "NE" }, { SE, "SE" }, { NW, "NW" }, { SW, "SW" },
        };

        public static readonly Dictionary<CoordStr, char> trans2Arrow = new Dictionary<CoordStr, char>()
        {
            { N, '^' }, { S, 'v' }, { E, '>' }, { W, '<' },
        };

        public static readonly Dictionary<char, CoordStr> trans2Coord = new Dictionary<char, CoordStr>()
        {
            { 'N', N }, { 'S', S }, { 'E', E }, { 'W', W },
            { '^', N }, { 'v', S }, { '>', E }, { '<', W },
            { 'U', N }, { 'D', S }, { 'R', E }, { 'L', W },
        };

        public int Row { get; set; }
        public int Col { get; set; }

        public int X => Col;
        public int Y => Row;

        public CoordStr(int row, int col)
        {
            CoordStrCounter++;
            Row = row;
            Col = col;
        }

        public static int CoordStrCounter;

        public static CoordStr FromXY(int x, int y)
        {
            return new CoordStr(y, x);
        }

        public bool IsNorthOf(CoordStr c) => this.Y < c.Y;
        public bool IsWestOf(CoordStr c) => this.X < c.X;


        private int GetDir(int v1, int v2)
        {
            return v1.CompareTo(v2);
        }

        /// <summary>
        /// Return a list of coordinates from current pos to target pos (c2) as long as the path is either NSWE or a 45 degree angle
        /// </summary>
        /// <param name="c2"></param>
        /// <param name="inclusive"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public IEnumerable<CoordStr> PathTo(CoordStr c2, bool inclusive = true)
        {
            var rDir = c2.Row.CompareTo(Row);
            var cDir = c2.Col.CompareTo(Col);
            //var res = new List<CoordStr>();

            var rDist = Math.Abs(Row - c2.Row);
            var cDist = Math.Abs(Col - c2.Col);
            if (rDist != 0 && cDist != 0 && rDist != cDist)
                throw new Exception(
                    $"Path is not a multiple of 45 degrees! {this} => {c2} (rDist: {rDist}, cDist: {cDist})");
            var steps = Math.Max(rDist, cDist) + 1;
            for (int s = 0; s < steps; s++)
            {
                if (inclusive || (s > 1 && s < steps - 1))
                {
                    var c = Col + s * cDir;
                    var r = Row + s * rDir;
                    yield return new CoordStr(r, c);
                }
            }
        }

        /// <summary>
        /// Sample CoordStr strings supported: "x, y", "(x y)", "x;y"
        /// Supported caracters: (, ;)
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static CoordStr Parse(string s)
        {
            var parts = s.Split(new[] { '(', ',', ' ', ';', ')' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                throw new Exception("Invalid coord string: " + s);
            return new CoordStr(int.Parse(parts[1]), int.Parse(parts[0]));
        }

        private bool Equals(CoordStr other)
        {
            return Row == other.Row && Col == other.Col;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CoordStr)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Row * 397) ^ Col;
            }
        }

        public static CoordStr CharToDir(char c)
        {
            return trans2Coord[c];
        }

        public static bool TryCharToDir(char c, out CoordStr dir)
        {
            return trans2Coord.TryGetValue(c, out dir);
        }

        public char DirToNESW()
        {
            return trans2NESW[this];
        }

        public CoordStr RotateCCW90()
        {
            return new CoordStr(-Col, Row);
        }

        public CoordStr RotateCCWDegrees(int degrees)
        {
            if (degrees % 90 != 0)
                throw new Exception("can only rotate even 90 degrees");
            var res = this;
            for (var i = 0; i < degrees / 90; i++)
                res = res.RotateCCW90();
            return res;
        }

        public CoordStr RotateCWDegrees(int degrees)
        {
            if (degrees % 90 != 0)
                throw new Exception("can only rotate even 90 degrees");
            var res = this;
            for (var i = 0; i < degrees / 90; i++)
                res = res.RotateCW90();
            return res;
        }

        public CoordStr RotateCW90()
        {
            return new CoordStr(Col, -Row);
        }

        public IEnumerable<CoordStr> GenAdjacent4()
        {
            foreach (var d in Directions4)
                yield return Move(d);
        }

        public IEnumerable<CoordStr> GenAdjacent8()
        {
            foreach (var d in Directions8)
                yield return Move(d);
        }

        public CoordStr Move(CoordStr coord, int count = 1)
        {
            return new CoordStr(Row + coord.Row * count, Col + coord.Col * count);
        }

        public CoordStr MoveX(int col)
        {
            return new CoordStr(Row, Col + col);
        }

        public CoordStr MoveY(int row)
        {
            return new CoordStr(Row + row, Col);
        }


        public int Dist(CoordStr pos)
        {
            return Math.Abs(Row - pos.Row) + Math.Abs(Col - pos.Col);
        }

        public override string ToString()
        {
            return $"{nameof(Row)}: {Row}, {nameof(Col)}: {Col}";
        }

        public CoordStr Move1Toward(CoordStr target)
        {
            var xDir = Clamp(target.X - X, -1, 1);
            var yDir = Clamp(target.Y - Y, -1, 1);
            return new CoordStr(Y + yDir, X + xDir);
        }

        private int Clamp(int v, int min, int max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }

        public bool OnSameLine(CoordStr other)
        {
            return X == other.X || Y == other.Y;
        }

        public CoordStr Multiply(int factor)
        {
            return new CoordStr(Row * factor, Col * factor);
        }

        public CoordStr Subtract(CoordStr other)
        {
            return CoordStr.FromXY(X - other.X, Y - other.Y);
        }
    }
}