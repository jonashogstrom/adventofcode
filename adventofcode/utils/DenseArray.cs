using System;
using System.Text;

namespace AdventofCode.Utils;

internal class DenseArray<T>
{
    private readonly T[][] _data;
    public DenseArray(int mapWidth, int mapHeight, T def = default)
    {
        _data = (T[][])Array.CreateInstance(typeof(T[]), mapHeight);
        for (var r = 0; r < mapHeight; r++)
        {
            _data[r] = new T[mapWidth];
            for (var c = 0; c < mapWidth; c++)
                _data[r][c] = def;
        }
    }

    public T GetValue(int row, int col)
    {
        return _data[row][col];
    }

    public void SetValue(int row, int col, T value)
    {
        _data[row][col] = value;
    }

    public override string ToString()
    {
        return ToString(c => c.ToString());
    }

    public string ToString(Func<T, string> f)
    {
        var sb = new StringBuilder();
        foreach (var t in _data)
        {
            foreach (var c in t)
            {
                sb.Append(f(c));
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }
}