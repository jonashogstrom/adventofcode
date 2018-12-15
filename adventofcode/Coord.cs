using System.Diagnostics;

namespace adventofcode
{
    [DebuggerDisplay("({Row},{Col})")]
    internal class Coord
    {
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
    }
}