using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventofCode;

public class SplitNode
{
    private string raw;
    private readonly IList<SplitNode> _children = new List<SplitNode>();
    public IList<SplitNode> Children => _children;
    public IEnumerable<int> IntChildren => Children.Select(c => c.i);
    public IEnumerable<int> SafeIntChildren
    {
        get { foreach(var x in Children)
            if (int.TryParse(x.Trimmed, out var value))
                yield return value;
        }
    }

    public IEnumerable<string> StrChildren => Children.Select(c => c.raw.Trim());
    public string Trimmed => raw.Trim();
    public int i => int.Parse(Trimmed);
    public SplitNode First => Children[0];
    public SplitNode Second => Children[1];
    public SplitNode Third => Children[2];
    public SplitNode Last => Children.Last();
    public long l => long.Parse(Trimmed);

    public SplitNode(string raw, params char[] splits)
    {
        this.raw = raw;
        ParseSplits(raw, splits.Select(c=>new[]{c}).ToArray());
    }

    private SplitNode(string raw, int dummy, char[][] splits)
    {
        this.raw = raw;
        ParseSplits(raw, splits);
    }

    public static SplitNode Create(string raw, char[] split1)
    {
        return new SplitNode(raw, -1, new char[][] { split1 });
    }

    private void ParseSplits(string raw, char[][] splits)
    {
        if (splits.Any())
        {
            foreach (var part in raw.Split(splits.First(), StringSplitOptions.RemoveEmptyEntries))
            {
                _children.Add(new SplitNode(part, -1, splits.Skip(1).ToArray()));
            }
        }
    }
}