using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace AdventofCodexxx.X
{
    public class SparseBuffer<T>
    {
        private CoordStruct topLeft = new CoordStruct(0, 0);
        private CoordStruct bottomRight = new CoordStruct(0, 0);
        private readonly T _def;
        private readonly Dictionary<CoordStruct, T> _board = new Dictionary<CoordStruct, T>();

        public SparseBuffer(T def = default)
        {
            _def = def;
        }

        public T this[CoordStruct coordStruct]
        {
            get
            {
                if (!_board.TryGetValue(coordStruct, out var res))
                    return _def;
                return res;
            }
            set
            {
                _board[coordStruct] = value;
                if (coordStruct.X < topLeft.X || coordStruct.Y < topLeft.Y)
                    topLeft = CoordStruct.FromXY(Math.Min(topLeft.X, coordStruct.X), Math.Min(topLeft.Y, coordStruct.Y));
                if (coordStruct.X > bottomRight.X || coordStruct.Y > bottomRight.Y)
                    bottomRight = CoordStruct.FromXY(Math.Max(bottomRight.X, coordStruct.X), Math.Max(bottomRight.Y, coordStruct.Y));

            }
        }

        public void Set(int x, int y, T value)
        {
            var c = CoordStruct.FromXY(x, y);
            this[c] = value;

        }

        public string ToString(Func<T, CoordStruct, string> func, int cellWidth = 1)
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
                sb.Append($"{Math.Abs(y):D4} ║");
                for (int x = topLeft.X; x <= bottomRight.X; x++)
                {
                    sb.Append(func(Get(x, y), CoordStruct.FromXY(x, y)));
                }

                sb.Append('║');
                sb.AppendLine();
            }
            sb.AppendLine("     ╚" + new string('═', Width*cellWidth) + '╝');

            return sb.ToString();
        }

        public int Width => bottomRight.X - topLeft.X + 1;
        public int Height => bottomRight.Y - topLeft.Y + 1;

        public IEnumerable<CoordStruct> Keys => _board.Keys;

        public string ToString(Func<T, string> func)
        {
            return ToString((v, c) => func(v));
        }

        private CoordStruct Reuse(CoordStruct coordStruct, int x, int y)
        {
            if (coordStruct.X == x && coordStruct.Y == y)
                return coordStruct;
            return new CoordStruct(x, y);
        }

        public T Get(int x, int y)
        {
            return this[CoordStruct.FromXY(x, y)];
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
                    var c = CoordStruct.FromXY(col, row);
                    floor[c] = input[row][col];
                }
            }

            return floor;
        }

    }
}