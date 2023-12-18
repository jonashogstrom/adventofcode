using System;
using System.Collections.Generic;

namespace AdventofCode.Utils;

public class GenAStar<T>
{
    private readonly Func<T, IEnumerable<T>> _nextStateFunc;

    public Func<T,  int> EstimatedCostToTargetFunc { get; }
    public Func<T, T, int> ActualCostFunc { get; }
    public Func<T, bool> IsTargetFunc { get; }

    public GenAStar(
        Func<T, IEnumerable<T>> nextStateFunc, 
        Func<T, int> estimatedCostToTarget, 
        Func<T, T, int> actualCostFunc, 
        Func<T, bool> isTargetFunc)
    {
        _nextStateFunc = nextStateFunc;
        EstimatedCostToTargetFunc = estimatedCostToTarget;
        ActualCostFunc = actualCostFunc;
        IsTargetFunc = isTargetFunc;
    }
    public IEnumerable<T> GetValidNextStates(T state)
    {
        return _nextStateFunc(state);
    }

    public long Cost(T from, T to)
    {
        return ActualCostFunc(from, to);
    }

    public long EstimatedCostToTarget(T from)
    {
        return EstimatedCostToTargetFunc(from);
    }

    public bool IsTarget(T state)
    {
        return IsTargetFunc(state);
    }

    public List<T> FindPath(T start)
    {

        var frontier = new PriorityQueue<T, long>();
        frontier.Enqueue(start, 0);
        var cameFrom = new Dictionary<T, T>();
        var costSoFar = new Dictionary<T, long>();
        cameFrom[start] = default;
        costSoFar[start] = 0;
        T target = default;
        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();

            if (IsTarget(current))
            {
                target = current;
                break;
            }

            foreach (var next in GetValidNextStates(current))
            {
                var newCost = costSoFar[current] + Cost(current, next);
                if (!costSoFar.TryGetValue(next, out var cost) || newCost < cost)
                {
                    costSoFar[next] = newCost;
                    var priority = newCost + EstimatedCostToTarget(next);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }
        }

        var res = new List<T>();
        var n = target;
        while (n != null)
        {
            res.Add(n);
            n = cameFrom[n];
        }

        res.Reverse();
        return res;
    }
}