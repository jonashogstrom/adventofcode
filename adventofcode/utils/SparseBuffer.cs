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
                _board[coord] = value;
                if (coord.X < topLeft.X || coord.Y < topLeft.Y)
                    topLeft = Coord.FromXY(Math.Min(topLeft.X, coord.X), Math.Min(topLeft.Y, coord.Y));
                if (coord.X > bottomRight.X || coord.Y > bottomRight.Y)
                    bottomRight = Coord.FromXY(Math.Max(bottomRight.X, coord.X), Math.Max(bottomRight.Y, coord.Y));

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
            for (int x = topLeft.X; x <= bottomRight.X; x++)
            {
                sb.Append((Math.Abs(x) % 10).ToString().PadLeft(1 + padding).PadRight(cellWidth));
            }
            sb.AppendLine();
            sb.AppendLine("     ╔" + new string('═', Width * cellWidth) + '╗');
            for (int y = topLeft.Y; y <= bottomRight.Y; y++)
            {
                if (y < 0)
                    sb.Append($"{y:D3} ║");
                else
                {
                    sb.Append($"{y:D4} ║");
                }
                for (int x = topLeft.X; x <= bottomRight.X; x++)
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

        public string ToString(Func<T, string> func)
        {
            return ToString((v, c) => func(v));
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

            for (int row = 0; row < input.Length; row++)
            {
                for (int col = 0; col < input[row].Length; col++)
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
    }
}