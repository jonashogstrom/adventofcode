using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace AdventofCode
{
    internal class SparseBuffer<T>
    {
        private Coord topLeft = new Coord(0,0);
        private Coord bottomRight = new Coord(0,0);
        private readonly T _def;
        private readonly Dictionary<int, Dictionary<int, T>> _board = new Dictionary<int, Dictionary<int, T>>();

        public SparseBuffer(T def = default)
        {
            _def = def;
        }

        public T this[Coord coord]
        {
            get => Get(coord.X, coord.Y);
            set => Set(coord.X, coord.Y, value);
        }
        
        public void Set(int x, int y, T value)
        {
            if (!_board.TryGetValue(x, out var col))
            {
                col = new Dictionary<int, T>();
                _board[x] = col;
            }

            col[y] = value;

            if (x < topLeft.X || y < topLeft.Y)
                topLeft = new Coord(Math.Min(topLeft.Y, y), Math.Min(topLeft.X, x));
            if (x > bottomRight.X || y > bottomRight.Y)
                bottomRight = new Coord(Math.Max(bottomRight.Y, y), Math.Max(bottomRight.X, x));
        }


        public string ToString(Func<T, string> func)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Top: {topLeft.Y} Bottom: {bottomRight.Y} Left: {topLeft.X} Right: {bottomRight.X}");
            for (int y = topLeft.Y; y <= bottomRight.Y; y++)
            {
                sb.Append('*');
                for (int x = topLeft.X; x <= bottomRight.X; x++)
                {
                    sb.Append(func(this.Get(x, y)));
                }

                sb.Append('*');
                sb.AppendLine();
            }

            return sb.ToString();
        }
        private Coord Reuse(Coord coord, int x, int y)
        {
            if (coord.X == x && coord.Y == y)
                return coord;
            return new Coord(x, y);
        }

        public T Get(int x, int y)
        {
            if (!_board.TryGetValue(x, out var col))
                return _def;
            if (!col.TryGetValue(y, out var res))
                return _def;
            return res;
        }
    }
}