using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdventofCode
{
    public class SparseBuffer<T>
    {
        private static Coord Origin = new Coord(0, 0);
        private Coord topLeft = Origin;
        private Coord bottomRight = Origin;
        private readonly T _def;
        private readonly Dictionary<Coord, T> _board = new Dictionary<Coord, T>();
        public int Top => topLeft.Y;
        public int Bottom => bottomRight.Y;
        public int Left => topLeft.X;
        public int Right => bottomRight.X;

        public SparseBuffer(T def = default)
        {
            _def = def;
        }

        public SparseBuffer(T def, Coord origin)
        {
            _def = def;
            topLeft = origin;
            bottomRight = origin;
        }

        public T this[Coord coord]
        {
            get
            {
                if (!_board.TryGetValue(coord, out var res))
                    return _def;
                return res;
            }
            set
            {
                if (value.Equals(_def))
                    _board.Remove(coord);
                else
                {
                    _board[coord] = value;
                    if (coord.X < topLeft.X || coord.Y < topLeft.Y)
                        topLeft = Coord.FromXY(Math.Min(topLeft.X, coord.X), Math.Min(topLeft.Y, coord.Y));
                    if (coord.X > bottomRight.X || coord.Y > bottomRight.Y)
                        bottomRight = Coord.FromXY(Math.Max(bottomRight.X, coord.X), Math.Max(bottomRight.Y, coord.Y));

                }

            }
        }

        public IEnumerable<int> AllRowIndices
        {
            get
            {
                for (int i = Top; i <= Bottom; i++)
                    yield return i;
            }
        }
        
        public IEnumerable<Coord> LeftEdge => CoordsInCol(Left);
        public IEnumerable<Coord> RightEdge => CoordsInCol(Right);
        public IEnumerable<Coord> TopEdge => CoordsInRow(Top);
        public IEnumerable<Coord> BottomEdge => CoordsInRow(Bottom);
        public IEnumerable<Coord> CoordsInCol(int col)
        {
            return AllRowIndices.Select(r => new Coord(r, col));
        }
        public IEnumerable<Coord> CoordsInRow(int row)
        {
            return AllColIndices.Select(c => new Coord(row, c));
        }


        public IEnumerable<int> AllColIndices
        {
            get
            {
                for (var i = Left; i <= Right; i++)
                    yield return i;
            }
        }



        public void RemoveDefaults()
        {
            foreach (var k in Keys.ToArray())
            {
                if (this[k].Equals(_def))
                    RemoveKey(k);
            }
        }

        public void Set(int x, int y, T value)
        {
            var c = Coord.FromXY(x, y);
            this[c] = value;

        }

        public string ToString(Func<T, Coord, string> func, int cellWidth = 1)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Top: {topLeft.Y} Bottom: {bottomRight.Y} Left: {topLeft.X} Right: {bottomRight.X}");
            sb.Append("      ");
            var padding = (cellWidth - 1) / 2;
            for (var x = topLeft.X; x <= bottomRight.X; x++)
            {
                sb.Append((Math.Abs(x) % 10).ToString().PadLeft(1 + padding).PadRight(cellWidth));
            }
            sb.AppendLine();
            sb.AppendLine("     ╔" + new string('═', Width * cellWidth) + '╗');
            for (var y = topLeft.Y; y <= bottomRight.Y; y++)
            {
                if (y < 0)
                    sb.Append($"{y:D3} ║");
                else
                {
                    sb.Append($"{y:D4} ║");
                }
                for (var x = topLeft.X; x <= bottomRight.X; x++)
                {
                    sb.Append(func(Get(x, y), Coord.FromXY(x, y)));
                }

                sb.Append('║');
                sb.AppendLine();
            }
            sb.AppendLine("     ╚" + new string('═', Width * cellWidth) + '╝');

            return sb.ToString();
        }

        public int Width => bottomRight.X - topLeft.X + 1;
        public int Height => bottomRight.Y - topLeft.Y + 1;

        public IEnumerable<Coord> Keys => _board.Keys;
        public T Default => _def;

        public IEnumerable<Coord> AllKeysInMap
        {
            get
            {
                for (var x = Left; x <= Right; x++)
                    for (var y = Top; y <= Bottom; y++)
                        yield return new Coord(y, x);
            }
        }

        public Coord TopLeft => new(Top, Left);
        public Coord BottomRight => new(Bottom, Right);

        public string ToString(Func<T, string> func)
        {
            return ToString((v, c) => func(v));
        }
        public string ToString()
        {
            return ToString((v, c) => v.ToString());
        }

        private Coord Reuse(Coord coord, int x, int y)
        {
            if (coord.X == x && coord.Y == y)
                return coord;
            return new Coord(x, y);
        }

        public T Get(int x, int y)
        {
            return this[Coord.FromXY(x, y)];
        }

        public SparseBuffer<T> Clone()
        {
            var res = new SparseBuffer<T>(_def);
            foreach (var x in _board.Keys)
                res[x] = _board[x];
            return res;
        }

        public static SparseBuffer<T> FromInput(T[][] input, T def)
        {
            var floor = new SparseBuffer<T>(def);

            for (var row = 0; row < input.Length; row++)
            {
                for (var col = 0; col < input[row].Length; col++)
                {
                    var c = Coord.FromXY(col, row);
                    floor[c] = input[row][col];
                }
            }

            return floor;
        }

        public int Count(T c)
        {
            return _board.Values.Count(x => x.Equals(c));
        }

        public bool InsideBounds(Coord coord)
        {
            return coord.Col >= topLeft.Col &&
                   coord.Col <= bottomRight.Col &&
                   coord.Row >= topLeft.Row &&
                   coord.Row <= bottomRight.Row;
        }

        public void RemoveKey(Coord coord)
        {
            _board.Remove(coord);
            if (coord.X == topLeft.X || coord.Y == topLeft.Y)
                topLeft = Coord.FromXY(Keys.Select(k => k.X).Min(), Keys.Select(k => k.Y).Min());
            if (coord.X == bottomRight.X || coord.Y == bottomRight.Y)
                bottomRight = Coord.FromXY(Keys.Select(k => k.X).Max(), Keys.Select(k => k.Y).Max());

        }

        public bool HasKey(Coord coord)
        {
            return _board.ContainsKey(coord);
        }

        public void AddBorders(T c)
        {
            var top = Top - 1;
            var left = Left - 1;
            var right = Right + 1;
            var bottom = Bottom + 1;
            for (var row = top; row <= bottom; row++)
            {
                this[new Coord(row, left)] = c;
                this[new Coord(row, right)] = c;
            }
            for (var col = left; col <= right; col++)
            {
                this[new Coord(top, col)] = c;
                this[new Coord(right, col)] = c;
            }
        }
    }
}