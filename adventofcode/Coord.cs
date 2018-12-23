using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace adventofcode
{
    [DebuggerDisplay("({Row},{Col})")]
    internal class Coord
    {
        public static Coord N = new Coord(-1, 0);
        public static Coord E = new Coord(0, 1);
        public static Coord S = new Coord(1, 0);
        public static Coord W = new Coord(0, -1);
        public static Coord Origo = new Coord(0, 0);

        public static Coord NW = N.Move(W);
        public static Coord NE = N.Move(E);
        public static Coord SE = S.Move(E);
        public static Coord SW = S.Move(W);

        private static readonly Coord[] Directions4 = {N, E, S, W};
        private static readonly Coord[] Directions8 = {N, NE, E, SE, S, SW, W, NW};

        public static readonly Dictionary<Coord, char> trans2NESW = new Dictionary<Coord, char>()
        {
            {N, 'N'}, {S, 'S'}, {E, 'E'}, {W, 'W'},
        };
        public static readonly Dictionary<char, Coord> trans2Coord = new Dictionary<char, Coord>()
        {
            {'N', N}, {'S', S}, {'E', E}, {'W', W},
            {'^', N}, {'v', S}, {'>', E}, {'<', W},
        };

        public int Row { get; set; }
        public int Col { get; set; }

        public Coord(int row, int col)
        {
            Row = row;
            Col = col;
        }

        protected bool Equals(Coord other)
        {
            return Row == other.Row && Col == other.Col;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Coord) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Row * 397) ^ Col;
            }
        }

        public static Coord CharToDir(char c)
        {
            return trans2Coord[c];
        }

        public char DirToNESW()
        {
            return trans2NESW[this];
        }

        public Coord RotateCCW90()
        {
            return new Coord(-Col, Row);
        }

        public Coord RotateCW90()
        {
            return new Coord(Col, -Row);
        }

        public IEnumerable<Coord> GenAdjacent4()
        {
            foreach (var d in Directions4)
                yield return Move(d);
        }

        public IEnumerable<Coord> GenAdjacent8()
        {
            foreach (var d in Directions8)
                yield return Move(d);
        }

        public Coord Move(Coord coord)
        {
            return new Coord(Row + coord.Row, Col + coord.Col);
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

        public int x;
        public int y;
        public int z;
    }
}