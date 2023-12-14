using System.Collections.Generic;

namespace AdventofCode.Utils;

public class AStar
{
    public IEnumerable<Coord> FindPath(Coord start, Coord target, Graph graph)
    {

        var frontier = new PriorityQueue<Coord, long>();
        frontier.Enqueue(start, 0);
        var came_from = new Dictionary<Coord, Coord>();
        var cost_so_far = new Dictionary<Coord, long>();
        came_from[start] = null;
        cost_so_far[start] = 0;

        while (frontier.Count != 0)
        {
            var current = frontier.Dequeue();

            if (current.Equals(target))
                break;

            foreach (var next in graph.neighbors(current))
            {
                var new_cost = cost_so_far[current] + graph.cost(current, next);
                if (!cost_so_far.TryGetValue(next, out var cost) || new_cost < cost)
                {
                    cost_so_far[next] = new_cost;
                    var priority = new_cost + graph.estimatedCost(target, next);
                    frontier.Enqueue(next, priority);
                    came_from[next] = current;
                }
            }
        }

        var res = new List<Coord>();
        var n = target;
        while (n != null)
        {
            res.Add(n);
            n = came_from[n];
        }

        res.Reverse();
        return res;

    }
}

public class Graph
{
    private readonly IEnumerable<Coord> _empty = new Coord[] { };
    private readonly Dictionary<(Coord from, Coord to), long> _cost = new();
    private readonly Dictionary<Coord, List<Coord>> _edges = new();
    public int EdgeCount => _edges.Count;

    public IEnumerable<Coord> neighbors(Coord coord)
    {
        if (_edges.TryGetValue(coord, out var neighbours))
            return neighbours;
        return _empty;
    }

    public void AddEdge(Coord from, Coord to, long? dist = null)
    {
        if (!_edges.TryGetValue(from, out var edges))
        {
            edges = new List<Coord>();
            _edges.Add(from, edges);
        }

        edges.Add(to);
        if (dist.HasValue)
            _cost.Add((from, to), dist.Value);
    }

    public long cost(Coord from, Coord to)
    {
        if (_cost.TryGetValue((from, to), out var res))
            return res;
        return from.Dist(to);
    }

    public long estimatedCost(Coord from, Coord to)
    {
        return from.Dist(to);
    }

    public bool HasEdge(Coord p1, Coord p2)
    {

        if (_edges.TryGetValue(p1, out var edges) && edges.Contains(p2)) return true;
        return false;
    }
}