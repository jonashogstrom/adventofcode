﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventofCode.utils;

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

    public SplitNode(string raw, params char[] splits)
    {
        this.raw = raw;
        ParseSplits(raw, splits.Select(c=>new[]{c}).ToArray());
    }

    public SplitNode(string raw, params char[][] splits)
    {
        this.raw = raw;
        ParseSplits(raw, splits);
    }

    private void ParseSplits(string raw, char[][] splits)
    {
        if (splits.Any())
        {
            foreach (var part in raw.Split(splits.First(), StringSplitOptions.RemoveEmptyEntries))
            {
                _children.Add(new SplitNode(part, splits.Skip(1).ToArray()));
            }
        }
    }
}