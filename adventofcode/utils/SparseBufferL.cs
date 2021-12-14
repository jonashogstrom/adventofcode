using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdventofCode.Utils
{
    public class SparseBufferL<T>
    {
        private CoordL topLeft = new CoordL(0, 0);
        private CoordL bottomRight = new CoordL(0, 0);
        private readonly T _def;
        private readonly Dictionary<CoordL, T> _board = new Dictionary<CoordL, T>();

        public SparseBufferL(T def = default)
        {
            _def = def;
        }

        public T this[CoordL CoordL]
        {
            get
            {
                if (!_board.TryGetValue(CoordL, out var res))
                    return _def;
                return res;
            }
            set
            {
                _board[CoordL] = value;
                if (CoordL.X < topLeft.X || CoordL.Y < topLeft.Y)
                    topLeft = CoordL.FromXY(Math.Min(topLeft.X, CoordL.X), Math.Min(topLeft.Y, CoordL.Y));
                if (CoordL.X > bottomRight.X || CoordL.Y > bottomRight.Y)
                    bottomRight = CoordL.FromXY(Math.Max(bottomRight.X, CoordL.X), Math.Max(bottomRight.Y, CoordL.Y));

            }
        }

        public void Set(long x, long y, T value)
        {
            var c = CoordL.FromXY(x, y);
            this[c] = value;

        }

        public string ToString(Func<T, CoordL, string> func, int cellWidth = 1)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Top: {topLeft.Y} Bottom: {bottomRight.Y} Left: {topLeft.X} Right: {bottomRight.X}");
            sb.Append("      ");
            var padding = (cellWidth - 1) / 2;
            for (long x = topLeft.X; x <= bottomRight.X; x++)
            {
                sb.Append((Math.Abs(x) % 10).ToString().PadLeft(1 + padding).PadRight(cellWidth));
            }
            sb.AppendLine();
            sb.AppendLine("     ╔" + new string('═', (int)Width * cellWidth) + '╗');
            for (long y = topLeft.Y; y <= bottomRight.Y; y++)
            {
                sb.Append($"{Math.Abs(y):D4} ║");
                for (long x = topLeft.X; x <= bottomRight.X; x++)
                {
                    sb.Append(func(Get(x, y), CoordL.FromXY(x, y)));
                }

                sb.Append('║');
                sb.AppendLine();
            }
            sb.AppendLine("     ╚" + new string('═', (int)Width * cellWidth) + '╝');

            return sb.ToString();
        }

        public long Width => bottomRight.X - topLeft.X + 1;
        public long Height => bottomRight.Y - topLeft.Y + 1;

        public IEnumerable<CoordL> Keys => _board.Keys;

        public string ToString(Func<T, string> func)
        {
            return ToString((v, c) => func(v));
        }

        private CoordL Reuse(CoordL CoordL, int x, int y)
        {
            if (CoordL.X == x && CoordL.Y == y)
                return CoordL;
            return new CoordL(x, y);
        }

        public T Get(long x, long y)
        {
            return this[CoordL.FromXY(x, y)];
        }

        public SparseBufferL<T> Clone()
        {
            var res = new SparseBufferL<T>(_def);
            foreach (var x in _board.Keys)
                res[x] = _board[x];
            return res;
        }

        public static SparseBufferL<T> FromInput(T[][] input, T def)
        {
            var floor = new SparseBufferL<T>(def);

            for (int row = 0; row < input.Length; row++)
            {
                for (int col = 0; col < input[row].Length; col++)
                {
                    var c = CoordL.FromXY(col, row);
                    floor[c] = input[row][col];
                }
            }

            return floor;
        }

        public int Count(T c)
        {
            return _board.Values.Count(x => x.Equals(c));
        }

        public bool InsideBounds(CoordL CoordL)
        {
            return CoordL.Col >= topLeft.Col &&
                   CoordL.Col <= bottomRight.Col &&
                   CoordL.Row >= topLeft.Row &&
                   CoordL.Row <= bottomRight.Row;
        }

        public void RemoveKey(CoordL CoordL)
        {
            _board.Remove(CoordL);
            if (CoordL.X == topLeft.X || CoordL.Y == topLeft.Y)
                topLeft = CoordL.FromXY(Keys.Select(k => k.X).Min(), Keys.Select(k => k.Y).Min());
            if (CoordL.X == bottomRight.X || CoordL.Y == bottomRight.Y)
                bottomRight = CoordL.FromXY(Keys.Select(k => k.X).Max(), Keys.Select(k => k.Y).Max());

        }
    }
}