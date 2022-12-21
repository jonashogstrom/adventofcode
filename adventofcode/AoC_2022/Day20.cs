using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2022
{
    // not -6061
    // not -2925

    // NOT 2342 // too low

    // NOT 8494
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day20 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(3, 1623178306, "Day20_test.txt")]
        [TestCase(8028, 8798438007673, "Day20.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            LogLevel = resourceName.Contains("test") ? 20 : -1;
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1 = 0;
            Part2Type part2 = 0;
            var sw = Stopwatch.StartNew();

            // parse input here
            var encryptedFile = source.AsInt().Select(x => (long)x).ToArray();
            var count = encryptedFile.Length;
            var dist = encryptedFile.Distinct().Count();

            LogAndReset("Parse", sw);

            // solve part 1 here

            var mixed = Mix(encryptedFile).ToList();

            part1 = CalculateCoordinates(mixed, encryptedFile);


            LogAndReset("*1", sw);

            var mixed2 = encryptedFile.Select(x => x * 811589153).ToList();
            mixed2 = Mix2(mixed2, 10).ToList();

            part2 = CalculateCoordinates(mixed2, encryptedFile);

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private static long CalculateCoordinates(List<long> mixed, long[] encryptedFile)
        {
            var posZero = mixed.IndexOf(0);
            var el1 = mixed.ElementAt((posZero + 1000) % encryptedFile.Length);
            var el2 = mixed.ElementAt((posZero + 2000) % encryptedFile.Length);
            var el3 = mixed.ElementAt((posZero + 3000) % encryptedFile.Length);
            var res = el1 + el2 + el3;
            return res;
        }

        private long[] Mix(IEnumerable<long> encryptedFile)
        {
            var count = encryptedFile.Count();
            var mixArea = new MyLinkedList<long>(encryptedFile);
            for (int pos = 0; pos < count; pos++)
            {
                var node = mixArea.FindByAddIndex(pos);
                mixArea.MoveNode(node, node.Value % (count - 1));
            }
            return mixArea.Values.ToArray();
        }

        private long[] Mix2(IEnumerable<long> encryptedFile, int rounds)
        {
            var count = encryptedFile.Count();
            var mixArea = new MyLinkedList<long>(encryptedFile);
            for (int c = 0; c < rounds; c++)
            {
                for (int pos = 0; pos < count; pos++)
                {
                    var node = mixArea.FindByAddIndex(pos);
                    mixArea.MoveNode(node, node.Value % (count - 1));
                }
                Log($"After {c + 1} rounds of mixing:");
                Log(string.Join(", ", mixArea.Values));
                Log("");

            }
            return mixArea.Values.ToArray();
        }
    }


    internal class MyLinkedList<T>
    {
        private MyLinkedListNode<T> _first;
        private Dictionary<int, MyLinkedListNode<T>> _nodes = new Dictionary<int, MyLinkedListNode<T>>();
        public MyLinkedList(IEnumerable<T> items)
        {
            _first = CreateNode(items.First());
            var temp = _first;
            var last = _first;
            foreach (var i in items.Skip(1))
            {
                temp.AddAfter(CreateNode(i));
                temp = temp.Next;
                last = temp;
            }
            _first.Prev = last;
            last.Next = _first;
        }

        public MyLinkedListNode<T> CreateNode(T value)
        {
            var res = new MyLinkedListNode<T>(value);
            _nodes[_nodes.Count] = res;
            return res;
        }

        public MyLinkedListNode<T> First => _first;

        public IEnumerable<T> Values
        {
            get
            {
                var temp = _first;
                yield return temp.Value;
                temp = temp.Next;
                while (temp != _first)
                {
                    yield return temp.Value;
                    temp = temp.Next;
                }
            }
        }

        public MyLinkedListNode<T> FindByAddIndex(int i)
        {
            return _nodes[i];
        }

        public void MoveNode(MyLinkedListNode<T> node, long steps)
        {
            if (steps == 0)
                return;
            // if (Math.Abs(steps) == _nodes.Count)
            //     return;
            if (steps > 0)
                MoveForward(node, Math.Abs(steps));
            else
                MoveBackward(node, Math.Abs(steps));
        }

        private void MoveBackward(MyLinkedListNode<T> node, long steps)
        {
            var target = node.Prev;
            if (_first == node)
            {
                _first = node.Prev;
            }
            Unlink(node);
            for (int i = 1; i < steps; i++)
                target = target.Prev;

            target.Prev.AddAfter(node);
        }

        private void MoveForward(MyLinkedListNode<T> node, long steps)
        {
            var target = node.Next;
            if (_first == node)
            {
                _first = node.Next;
            }

            Unlink(node);
            for (int i = 1; i < steps; i++)
                target = target.Next;

            target.AddAfter(node);
        }

        private void Unlink(MyLinkedListNode<T> node)
        {
            node.Next.Prev = node.Prev;
            node.Prev.Next = node.Next;
            node.Prev = null;
            node.Next = null;

        }
    }

    [DebuggerDisplay("({Prev.Value})<< {Value} >>({Next.Value}")]
    internal class MyLinkedListNode<T>
    {
        public MyLinkedListNode(T value)
        {
            Value = value;
        }

        public T Value { get; }

        public MyLinkedListNode<T> Next { get; set; }
        public MyLinkedListNode<T> Prev { get; set; }

        public void AddAfter(MyLinkedListNode<T> x1)
        {
            var next = Next;
            x1.Next = next;
            if (next != null)
                next.Prev = x1;
            Next = x1;
            x1.Prev = this;
        }
    }
}