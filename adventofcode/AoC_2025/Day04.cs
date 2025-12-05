using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Accord;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2025
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day04 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(13, 43, "<day>_test.txt")]
        [TestCase(1578, 10132, "<day>.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            LogLevel = resourceName.Contains("test") ? 20 : -1;
            var source = GetResource2(ref resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1 = 0;
            Part2Type part2 = 0;
            var sw = Stopwatch.StartNew();

            var map = source.ToSparseBuffer('.');

            LogAndReset("Parse", sw);

            var rolls = new HashSet<Coord>(map.Keys);
            part1 = rolls.Count(r => r.GenAdjacent8().Count(rolls.Contains) < 4);

            LogAndReset("*1", sw);

            var q = new QHashSet<Coord>(rolls);
            while (q.Any())
            {
                var r = q.Dequeue();
                var neighbours = r.GenAdjacent8().Where(rolls.Contains).ToArray();
                if (neighbours.Length < 4)
                {
                    part2++;
                    rolls.Remove(r);
                    map[r] = '.';
                    foreach (var n in neighbours)
                    {
                        q.EnqueueUnique(n);
                    }
                }
            }

            Log(map.ToString());
            LogAndReset("*2", sw);

            return (part1, part2);
        }
    }

    internal class QHashSet<T>
    {
        private readonly Queue<T> _q;
        private readonly HashSet<T> _h;
        private int _count;

        public QHashSet(HashSet<T> initial)
        {
            _q  = new Queue<T>(initial);
            _h =  new HashSet<T>(initial);
            _count = _h.Count;
        }

        public bool Any()
        {
            return _count > 0;
        }

        public T Dequeue()
        {
                var value = _q.Dequeue();
                _h.Remove(value);
                _count--;
                return value;
        }

        public bool Contains(T value)
        {
            return _h.Contains(value);
        }

        public void EnqueueUnique(T value)
        {
            if (!_h.Contains(value) && _h.Add(value))
            {
                _q.Enqueue(value);
                _count++;
            }
        }

        public Part2Type Count()
        {
            return _count;
        }
    }
}