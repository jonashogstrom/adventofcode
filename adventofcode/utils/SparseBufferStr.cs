using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdventofCode
{
    public class SparseBufferStr<T>
    {
        private static CoordStr Origin = new CoordStr(0, 0);
        private CoordStr topLeft = Origin;
        private CoordStr bottomRight = Origin;
        private readonly T _def;
        private readonly Dictionary<CoordStr, T> _board = new Dictionary<CoordStr, T>();
        public int Top => topLeft.Y;
        public int Bottom => bottomRight.Y;
        public int Left => topLeft.X;
        public int Right => bottomRight.X;

        public SparseBufferStr(T def = default)
        {
            _def = def;
        }

        public SparseBufferStr(T def, CoordStr origin)
        {
            _def = def;
            topLeft = origin;
            bottomRight = origin;
        }

        public T this[CoordStr coord]
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
                        topLeft = CoordStr.FromXY(Math.Min(topLeft.X, coord.X), Math.Min(topLeft.Y, coord.Y));
                    if (coord.X > bottomRight.X || coord.Y > bottomRight.Y)
                        bottomRight = CoordStr.FromXY(Math.Max(bottomRight.X, coord.X), Math.Max(bottomRight.Y, coord.Y));

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
        
        public IEnumerable<CoordStr> LeftEdge => CoordsInCol(Left);
        public IEnumerable<CoordStr> RightEdge => CoordsInCol(Right);
        public IEnumerable<CoordStr> TopEdge => CoordsInRow(Top);
        public IEnumerable<CoordStr> BottomEdge => CoordsInRow(Bottom);
        public IEnumerable<CoordStr> CoordsInCol(int col)
        {
            foreach (var r in AllRowIndices)
                yield return new CoordStr(r, col);
        }
        public IEnumerable<CoordStr> CoordsInRow(int row)
        {
            foreach (var c in AllColIndices)
                yield return new CoordStr(row, c);
        }


        public IEnumerable<int> AllColIndices
        {
            get
            {
                for (int i = Left; i <= Right; i++)
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
            var c = CoordStr.FromXY(x, y);
            this[c] = value;

        }

        public string ToString(Func<T, CoordStr, string> func, int cellWidth = 1)
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
                    sb.Append(func(Get(x, y), CoordStr.FromXY(x, y)));
                }

                sb.Append('║');
                sb.AppendLine();
            }
            sb.AppendLine("     ╚" + new string('═', Width * cellWidth) + '╝');

            return sb.ToString();
        }

        public int Width => bottomRight.X - topLeft.X + 1;
        public int Height => bottomRight.Y - topLeft.Y + 1;

        public IEnumerable<CoordStr> Keys => _board.Keys;
        public T Default => _def;

        public IEnumerable<CoordStr> AllKeysInMap
        {
            get
            {
                for (int x = Left; x <= Right; x++)
                    for (int y = Top; y <= Bottom; y++)
                        yield return new CoordStr(y, x);
            }
        }

        public CoordStr TopLeft => new(Top, Left);
        public CoordStr BottomRight => new(Bottom, Right);

        public string ToString(Func<T, string> func)
        {
            return ToString((v, c) => func(v));
        }
        public string ToString()
        {
            return ToString((v, c) => v.ToString());
        }

        private CoordStr Reuse(CoordStr coord, int x, int y)
        {
            if (coord.X == x && coord.Y == y)
                return coord;
            return new CoordStr(x, y);
        }

        public T Get(int x, int y)
        {
            return this[CoordStr.FromXY(x, y)];
        }

        public SparseBufferStr<T> Clone()
        {
            var res = new SparseBufferStr<T>(_def);
            foreach (var x in _board.Keys)
                res[x] = _board[x];
            return res;
        }

        public static SparseBufferStr<T> FromInput(T[][] input, T def)
        {
            var floor = new SparseBufferStr<T>(def);

            for (int row = 0; row < input.Length; row++)
            {
                for (int col = 0; col < input[row].Length; col++)
                {
                    var c = CoordStr.FromXY(col, row);
                    floor[c] = input[row][col];
                }
            }

            return floor;
        }

        public int Count(T c)
        {
            return _board.Values.Count(x => x.Equals(c));
        }

        public bool InsideBounds(CoordStr coord)
        {
            return coord.Col >= topLeft.Col &&
                   coord.Col <= bottomRight.Col &&
                   coord.Row >= topLeft.Row &&
                   coord.Row <= bottomRight.Row;
        }

        public void RemoveKey(CoordStr coord)
        {
            _board.Remove(coord);
            if (coord.X == topLeft.X || coord.Y == topLeft.Y)
                topLeft = CoordStr.FromXY(Keys.Select(k => k.X).Min(), Keys.Select(k => k.Y).Min());
            if (coord.X == bottomRight.X || coord.Y == bottomRight.Y)
                bottomRight = CoordStr.FromXY(Keys.Select(k => k.X).Max(), Keys.Select(k => k.Y).Max());

        }

        public bool HasKey(CoordStr coord)
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
                this[new CoordStr(row, left)] = c;
                this[new CoordStr(row, right)] = c;
            }
            for (var col = left; col <= right; col++)
            {
                this[new CoordStr(top, col)] = c;
                this[new CoordStr(right, col)] = c;
            }
        }
    }
}