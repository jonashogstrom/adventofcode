using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventofCode.AoC_2019
{
    public class IntCodeComputer
    {
        public int[] Memory;
        public int Pointer;
        public bool Terminated { get; set; }

        private readonly Dictionary<int, IOperation> _ops = new Dictionary<int, IOperation>();

        public IntCodeComputer()
        {
            RegisterOp(new GenericIntFunc("Add", 1, (i, j) => i + j));
            RegisterOp(new GenericIntFunc("Mul", 2, (i, j) => i * j));
            RegisterOp(new Terminate());
        }

        private void RegisterOp(IOperation op)
        {
            _ops[op.OpCode] = op;
        }

        public int RunProgram(string program, int noun, int verb)
        {
            // Console.WriteLine($"======================= {noun} {verb}");
            Memory = program.Split(',').Select(int.Parse).ToArray();
            Pointer = 0;
            Memory[1] = noun;
            Memory[2] = verb;
            Terminated = false;

            while (!Terminated)
            {
                if (Pointer >= Memory.Length)
                    throw new Exception($"Pointer outside of memory: {Pointer}");
                if (!_ops.ContainsKey(Memory[Pointer]))
                    throw new Exception($"Unknown operation: {Memory[Pointer]}");
                var op = _ops[Memory[Pointer]];
                op.Execute(this);
            }

            return Memory[0];
        }
    }

/*
 internal class Add : IntFunc
    {
        public override int OpCode => 1;

        protected override int calc(int arg1, int arg2)
        {
            return arg1 + arg2;
        }
    }
*/


    internal abstract class IntFunc : BaseOp, IOperation
    {
        protected IntFunc(int opCode)
        {
            OpCode = opCode;
        }

        public int OpCode { get; }
        public int ArgCount => 3;
        public void Execute(IntCodeComputer comp)
        {
            var args = GetArgs(comp, ArgCount);

            var a1 = comp.Memory[args[0]];
            var a2 = comp.Memory[args[1]];
            var res = calc(a1, a2);
           // Console.WriteLine($"{a1} ({Name}) {a2} => {res}");
            comp.Memory[args[2]] = res;
            comp.Pointer += ArgCount + 1;
        }

        protected abstract int calc(int arg1, int arg2);
    }

    internal class GenericIntFunc : IntFunc
    {
        private readonly Func<int, int, int> _func;

        public override string Name { get; }

        public GenericIntFunc(string name, int opCode, Func<int, int, int> func): base(opCode)
        {
            Name = name;
            _func = func;
        }

        protected override int calc(int arg1, int arg2)
        {
            return _func(arg1, arg2);
        }
    };

    public class BaseOp
    {
        public virtual string Name => this.GetType().Name;

        protected int[] GetArgs(IntCodeComputer comp, int argCount)
        {
            var args = new int[argCount];
            for (int i = 0; i < argCount; i++)
            {
                args[i] = comp.Memory[comp.Pointer + i + 1];
            }

            return args;
        }
    }

    internal class Terminate : BaseOp, IOperation
    {
        public int OpCode => 99;
        public int ArgCount => 0;
        public void Execute(IntCodeComputer comp)
        {
            comp.Terminated = true;
        }
    }

    interface IOperation
    {
        string Name { get; }
        int OpCode { get; }
        int ArgCount { get; }
        void Execute(IntCodeComputer comp);
    }
}