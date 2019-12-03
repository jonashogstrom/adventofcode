using System.Collections.Generic;

namespace adventofcode
{
    internal class SparseBuffer<T>
    {
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