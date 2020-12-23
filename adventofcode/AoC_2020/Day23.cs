using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2020
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day23 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(67384529, 149245887792, "389125467")]
        [TestCase(36542897, 562136730660, "942387615")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            LogLevel = 1;
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1;
            Part2Type part2;

            var sw = Stopwatch.StartNew();
            var stack = InitStack(source[0], out var stackPointers, 9);
            var p = stack.First;

            LogAndReset("Parse", sw);
            foreach (var x in Enumerable.Range(1, 100))
            {
                MakeMove(stack, p, stackPointers, x);
                p = p.Next;
            }

            part1 = CalculateResult(stackPointers);

            LogAndReset("*1", sw);

            stack = InitStack(source[0], out stackPointers, 1000000);
            p = stack.First;
            foreach (var x in Enumerable.Range(1, 10000000))
            {
                MakeMove(stack, p, stackPointers, x);
                p = p.Next;
            }

            var p1 = stackPointers[1];
            part2 = p1.Next.Value;
            part2 = part2 * p1.Next.Next.Value;

            LogAndReset("*2", sw);
            return (part1, part2);
        }

        private static MyLinkedList<int> InitStack(string source, out Dictionary<int, MyLinkedListNode<int>> stackPointers, int endValue)
        {
            var ints = source.ToCharArray().Select(c => int.Parse(c.ToString())).ToArray();
            var stack = new MyLinkedList<int>(ints);

            var last = stack.First;
            while (last.Next != null)
                last = last.Next;
            for (int i = source.Length + 1; i <= endValue; i++)
            {
                last.AddAfter(new MyLinkedListNode<int>(i));
                last = last.Next;
            }
            stackPointers = new Dictionary<int, MyLinkedListNode<int>>();

            var p = stack.First;
            var temp = p;

            while (true)
            {
                stackPointers[temp.Value] = temp;
                if (temp.Next == null)
                {
                    temp.Next = p;
                    p.Prev = temp;
                    break;
                }

                temp = temp.Next;
            }

            return stack;
        }

        private static long CalculateResult(Dictionary<int, MyLinkedListNode<int>> stackPointers)
        {
            var s = 0L;
            var xx = stackPointers[1];
            var tempxx = xx.Next;
            while (tempxx != xx)
            {
                s = s * 10 + tempxx.Value;
                tempxx = tempxx.Next;
            }

            return s;
        }

        private void MakeMove(MyLinkedList<int> stack, MyLinkedListNode<int> p,
            Dictionary<int, MyLinkedListNode<int>> stackPointers, int i)
        {
            Log(()=>"", 10);
            Log(()=>$"-- Move {i} --", 10);

            Log(()=> GetCupString(stack, p), 10);
            var x1 = ExtractNodes(p.Next, 3);
            // var x2 = ExtractNode(p.Next, stack);
            // var x3 = ExtractNode(p.Next, stack);
            Log(()=>$"Pick up: {x1.Value} {x1.Next.Value} {x1.Next.Next.Value}", 10);
            var destination = (p.Value - 1) ;
            if (destination == 0)
                destination = stackPointers.Count;
            while (destination == x1.Value || destination == x1.Next.Value || destination == x1.Next.Next.Value)
            {
                destination--;
                if (destination == 0)
                    destination = stackPointers.Count;
            }
            Log(()=>$"destination: {destination}", 10);

            var destNode = stackPointers[destination];
            destNode.AddListAfter(x1);
            // node.AddAfter(x2);
            // x2.AddAfter(x3);
        }

        private MyLinkedListNode<int> ExtractNodes(MyLinkedListNode<int> node, int count)
        {
            var res = node;
            var before = res.Prev;
            var after = node;
            for (int i = 0; i < count; i++)
                after = after.Next;
            var last = after.Prev;
            before.Next = after;
            after.Prev = before;
            last.Next = null;
            return res;



        }

        private static string GetCupString(MyLinkedList<int> stack, MyLinkedListNode<int> p)
        {
            var s = "cups: ";
            foreach (var v in stack.Values)
            {
                if (v == p.Value)
                    s += $"({v}) ";
                else s += v + " ";
            }

            return s;
        }

        private MyLinkedListNode<int> ExtractNode(MyLinkedListNode<int> item, MyLinkedList<int> stack)
        {
            var res = item;
            var prev = res.Prev;
            var next = res.Next;
            prev.Next = next;
            next.Prev = prev;
            return res;
        }
    }

    internal class MyLinkedList<T>
    {
        private MyLinkedListNode<T> _first;

        public MyLinkedList(IEnumerable<T> items)
        {
            _first = new MyLinkedListNode<T>(items.First());
            var temp = _first;
            foreach (var i in items.Skip(1))
            {
                temp.AddAfter(new MyLinkedListNode<T>(i));
                temp = temp.Next;
            }
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
    }

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

        public void AddListAfter(MyLinkedListNode<T> node)
        {
            var last = node;
            while (last.Next != null)
                last = last.Next;
            Next.Prev = last;
            last.Next = Next;
            node.Prev = this;
            Next = node;
        }
    }
}