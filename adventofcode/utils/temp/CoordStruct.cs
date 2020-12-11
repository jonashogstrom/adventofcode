using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AdventofCodexxx.X
{
    [DebuggerDisplay("({Row},{Col})")]
    public class CoordStruct
    {
        public static CoordStruct N = new CoordStruct(-1, 0);
        public static CoordStruct E = new CoordStruct(0, 1);
        public static CoordStruct S = new CoordStruct(1, 0);
        public static CoordStruct W = new CoordStruct(0, -1);
        public static CoordStruct Origin = new CoordStruct(0, 0);
        public static CoordStruct[] NSWE = new[]{N, S, W, E};

        public static CoordStruct NW = N.Move(W);
        public static CoordStruct NE = N.Move(E);
        public static CoordStruct SE = S.Move(E);
        public static CoordStruct SW = S.Move(W);

        public static readonly CoordStruct[] Directions4 = { N, E, S, W };
        public static readonly CoordStruct[] Directions8 = { N, NE, E, SE, S, SW, W, NW };

        public static readonly Dictionary<CoordStruct, char> trans2NESW = new Dictionary<CoordStruct, char>()
        {
            {N, 'N'}, {S, 'S'}, {E, 'E'}, {W, 'W'},
        };
        public static readonly Dictionary<CoordStruct, char> trans2Arrow = new Dictionary<CoordStruct, char>()
        {
            {N, 'v'}, {S, '^'}, {E, '>'}, {W, '<'},
        };
        public static readonly Dictionary<char, CoordStruct> trans2Coord = new Dictionary<char, CoordStruct>()
        {
            {'N', N}, {'S', S}, {'E', E}, {'W', W},
            {'^', N}, {'v', S}, {'>', E}, {'<', W},
            {'U', N}, {'D', S}, {'R', E}, {'L', W},
        };

        public int Row { get; set; }
        public int Col { get; set; }

        public int X => Col;
        public int Y => Row;

        public CoordStruct(int row, int col)
        {
            CoordCounter++;
            Row = row;
            Col = col;
        }

        public static int CoordCounter;

        public static CoordStruct FromXY(int x, int y)
        {
            return new CoordStruct(y, x);
        }
        private bool Equals(CoordStruct other)
        {
            return Row == other.Row && Col == other.Col;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CoordStruct)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Row * 397) ^ Col;
            }
        }

        public static CoordStruct CharToDir(char c)
        {
            return trans2Coord[c];
        }

        public char DirToNESW()
        {
            return trans2NESW[this];
        }

        public CoordStruct RotateCCW90()
        {
            return new CoordStruct(-Col, Row);
        }

        public CoordStruct RotateCW90()
        {
            return new CoordStruct(Col, -Row);
        }

        public IEnumerable<CoordStruct> GenAdjacent4()
        {
            foreach (var d in Directions4)
                yield return Move(d);
        }

        public IEnumerable<CoordStruct> GenAdjacent8()
        {
            foreach (var d in Directions8)
                yield return Move(d);
        }

        public CoordStruct Move(CoordStruct coordStruct, int count = 1)
        {
            return new CoordStruct(Row + coordStruct.Row * count, Col + coordStruct.Col * count);
        }

        public int Dist(CoordStruct pos)
        {
            return Math.Abs(Row - pos.Row) + Math.Abs(Col - pos.Col); 
        }

        public override string ToString()
        {
            return $"{nameof(Row)}: {Row}, {nameof(Col)}: {Col}";
        }
    }

    public class Coord3d
    {
        public Coord3d(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        protected bool Equals(Coord3d other)
        {
            return x == other.x && y == other.y && z == other.z;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Coord3d) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = x;
                hashCode = (hashCode * 397) ^ y;
                hashCode = (hashCode * 397) ^ z;
                return hashCode;
            }
        }

        public int x;
        public int y;
        public int z;

        public Coord3d Clone()
        {
            return new Coord3d(x, y, z);
        }

        public Coord3d Move(int x, int y, int z)
        {
            return new Coord3d(this.x + x, this.y+y, this.z+z);
        }

        public Coord3d Move(Coord3d v)
        {
            return Move(v.x, v.y, v.z);
        }

        public int absValue()
        {
            return Math.Abs(x) + Math.Abs(y) + Math.Abs(z);
        }
    }
}