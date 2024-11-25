using System;
using System.Collections.Generic;

namespace AdventofCode.Utils;

public class AStar
{
    public (List<Coord> res, Dictionary<Coord, long> cost_so_far, Dictionary<Coord, Coord> came_from, List<Coord>, long, Dictionary<Coord, long>) FindPath(Coord start, Coord target, Graph graph)
    {

        var frontier = new PriorityQueue<Coord, long>();
        frontier.Enqueue(start, 0);
        var cameFrom = new Dictionary<Coord, Coord>();
        var costSoFar = new Dictionary<Coord, long>();
        cameFrom[start] = null;
        costSoFar[start] = 0;
        var pathSoFar = new Dictionary<Coord, List<Coord>>
        {
            [start] = new List<Coord> {start}
        };

        while (frontier.Count>0)
        {
            var current = frontier.Dequeue();

            // if (current.Equals(target))
            //     break;

            foreach (var next in graph.neighbors(current))
            {
                var newCost = costSoFar[current] + graph.Cost(pathSoFar[current], current, next);
                if (!costSoFar.TryGetValue(next, out var cost) || newCost < cost)
                {
                    var path = new List<Coord>(pathSoFar[current]) { next };
                    pathSoFar[next] = path;
                    costSoFar[next] = newCost;
                    var priority = newCost + graph.EstimatedCost(target, next);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                    // if (next.Equals(new Coord(0, 4)))
                    // {
                    //     Console.WriteLine("Test");
                    // }
                }
            }
        }

        var res = new List<Coord>();
        var n = target;
        while (n != null)
        {
            res.Add(n);
            n = cameFrom[n];
        }

        res.Reverse();
        return (res, costSoFar, cameFrom, pathSoFar[target], costSoFar[target], costSoFar);

    }
}

public class Graph
{
    private readonly IEnumerable<Coord> _empty = new Coord[] { };
    private readonly Dictionary<(Coord from, Coord to), long> _cost = new();
    private readonly Dictionary<Coord, HashSet<Coord>> _edges = new();
    public int EdgeCount => _edges.Count;

    public Func<Coord, Coord, int> EstimatedCostFunc { get; set; }
    public Func<IList<Coord>, Coord, Coord, int> ActualCostFunc { get; set; }

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
            edges = new HashSet<Coord>();
            _edges[from] = edges;
        }

        edges.Add(to);
        if (dist.HasValue)
            _cost.Add((from, to), dist.Value);
    }

    public long Cost(IList<Coord> path, Coord from, Coord to)
    {
        if (ActualCostFunc != null)
        {
            var cost = ActualCostFunc(path, from, to);
            return cost;
        }
        if (_cost.TryGetValue((from, to), out var res))
            return res;
        return from.Dist(to);
    }

    public long EstimatedCost(Coord from, Coord to)
    {
        if (EstimatedCostFunc != null)
            return EstimatedCostFunc(from, to);
        return from.Dist(to);
    }

    public bool HasEdge(Coord p1, Coord p2)
    {

        if (_edges.TryGetValue(p1, out var edges) && edges.Contains(p2)) return true;
        return false;
    }
}