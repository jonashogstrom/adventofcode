using System;
using System.Collections.Generic;
using System.Linq;

namespace adventofcode.AoC_2019
{
    public class IntCodeComputer
    {
        public int[] memory;
        public int pointer;
        public bool terminated { get; set; }

        private readonly Dictionary<int, IOperation> _ops = new Dictionary<int, IOperation>();

        public IntCodeComputer()
        {
            RegisterOp(new Add());
            RegisterOp(new Mul());
            RegisterOp(new Quit());
        }

        private void RegisterOp(IOperation op)
        {
            _ops[op.OpCode] = op;
        }

        public int runprogram(string program, int noun, int verb)
        {
            // Console.WriteLine($"======================= {noun} {verb}");
            memory = program.Split(',').Select(int.Parse).ToArray();
            pointer = 0;
            memory[1] = noun;
            memory[2] = verb;
            terminated = false;

            while (!terminated)
            {
                if (pointer >= memory.Length)
                    throw new Exception("pointer outside of memory");
                if (!_ops.ContainsKey(memory[pointer]))
                    throw new Exception("unknown operation");
                var op = _ops[memory[pointer]];
                op.Execute(this);
            }

            return memory[0];
        }
    }

    internal class Add : IntFunc
    {
        public override int OpCode => 1;

        protected override int calc(int arg1, int arg2)
        {
            return arg1 + arg2;
        }
    }

    internal abstract class IntFunc : BaseOp, IOperation
    {
        public abstract int OpCode { get; }
        public int ArgCount => 3;
        public void Execute(IntCodeComputer comp)
        {
            var args = GetArgs(comp, ArgCount);

            int a1 = comp.memory[args[0]];
            var a2 = comp.memory[args[1]];
            var res = calc(a1, a2);
            // Console.WriteLine($"{a1} ({opCode}) {a2} => {res}");
            comp.memory[args[2]] = res;
            comp.pointer += ArgCount+1;
        }

        protected abstract int calc(int arg1, int arg2);
    }

    internal class Mul : IntFunc
    {
        public override int OpCode => 2;
        protected override int calc(int arg1, int arg2)
        {
            return arg1 * arg2;
        }
    }

    public class BaseOp
    {
        protected int[] GetArgs(IntCodeComputer comp, int argCount)
        {
            var args = new int[argCount];
            for (int i = 0; i < argCount; i++)
            {
                args[i] = comp.memory[comp.pointer + i + 1];
            }

            return args;
        }
    }

    internal class Quit : BaseOp, IOperation
    {
        public int OpCode => 99;
        public int ArgCount => 0;
        public void Execute(IntCodeComputer comp)
        {
            comp.terminated = true;
        }
    }

    interface IOperation
    {
        int OpCode { get; }
        int ArgCount { get; }
        void Execute(IntCodeComputer comp);
    }
}